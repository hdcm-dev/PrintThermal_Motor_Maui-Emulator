// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Hci.Netsim;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Tests.Bluetooth;

/// <summary>
/// El transporte gRPC contra un servidor HTTP/2 sin TLS que imita a netsim: lee las tramas gRPC del pedido, exige que la
/// primera registre el chip y contesta cada comando HCI con un Command Complete.
/// </summary>
public class HciNetsimTests
{
    private static async Task<byte[]?> LeerTramaAsync(Stream s)
    {
        var cab = new byte[5];
        var n = 0;
        while (n < 5)
        {
            var r = await s.ReadAsync(cab.AsMemory(n));
            if (r == 0)
            {
                return null;
            }
            n += r;
        }
        var m = new byte[BinaryPrimitives.ReadUInt32BigEndian(cab.AsSpan(1))];
        await s.ReadExactlyAsync(m);
        return m;
    }

    private static async Task EscribirTramaAsync(Stream s, byte[] m)
    {
        var t = new byte[5 + m.Length];
        BinaryPrimitives.WriteUInt32BigEndian(t.AsSpan(1), (uint)m.Length);
        m.CopyTo(t, 5);
        await s.WriteAsync(t);
        await s.FlushAsync();
    }

    /// <summary>PacketResponse{hci_packet: HCIPacket{EVENT, Command Complete del opcode, estado 0 + extra}}.</summary>
    private static byte[] RespuestaCc(ushort opcode, byte[] extra)
    {
        byte[] evento = [0x0E, (byte)(4 + extra.Length), 0x01, (byte)opcode, (byte)(opcode >> 8), 0x00, .. extra];
        byte[] hci = [0x08, 0x04, 0x12, (byte)evento.Length, .. evento];
        return [0x12, (byte)hci.Length, .. hci];
    }

    [Fact]
    public async Task ElTransporteHablaGrpcSobreHttp2ConNetsim()
    {
        var registro = new List<string>();
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0, l => l.Protocols = HttpProtocols.Http2));
        await using var app = builder.Build();
        app.MapPost(TransporteNetsim.Ruta, async (HttpContext ctx) =>
        {
            lock (registro)
            {
                registro.Add($"content-type={ctx.Request.ContentType} te={ctx.Request.Headers.TE} proto={ctx.Request.Protocol}");
            }
            ctx.Response.ContentType = "application/grpc";
            await ctx.Response.StartAsync();
            var inicial = await LeerTramaAsync(ctx.Request.Body);
            lock (registro)
            {
                registro.Add("inicial=" + Convert.ToHexString(inicial!));
            }
            while (await LeerTramaAsync(ctx.Request.Body) is { } m)
            {
                var pedido = MensajesNetsimDePrueba.Hci(m);
                var opcode = BinaryPrimitives.ReadUInt16LittleEndian(pedido);
                byte[] extra = opcode switch
                {
                    0x1005 => [0x1B, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00],
                    0x1009 => [0x00, 0x00, 0x00, 0x00, 0x00, 0x04],
                    _ => [],
                };
                await EscribirTramaAsync(ctx.Response.Body, RespuestaCc(opcode, extra));
            }
            ctx.Response.AppendTrailer("grpc-status", "0");
        });
        await app.StartAsync();
        var url = new Uri(app.Urls.First());

        await using var transporte = TransporteNetsim.Conectar(url, ControladorFalso.Propia, "58HB6");
        var p = new PerifericoBluetooth(transporte, new OpcionesPeriferico(),
            new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria());
        using var cts = new CancellationTokenSource();
        var corrida = p.EjecutarAsync(cts.Token);
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(ControladorFalso.Propia, p.DireccionLocal);
        lock (registro)
        {
            Assert.Contains(registro, r => r.StartsWith("content-type=application/grpc te=trailers proto=HTTP/2", StringComparison.Ordinal));
            Assert.Contains(registro, r => r.StartsWith("inicial=0A", StringComparison.Ordinal));
        }
        cts.Cancel();
        await corrida.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(EstadoPeriferico.Detenido, p.Estado);
    }

    [Fact]
    public async Task SinNetsimEscuchandoElServicioQuedaEnErrorYReintenta()
    {
        // Un puerto local sin nadie escuchando.
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var puerto = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        var url = new Uri($"http://127.0.0.1:{puerto}");

        var s = new ServicioPeriferico(_ => Task.FromResult<ITransporteHci>(TransporteNetsim.Conectar(url, ControladorFalso.Propia, "58HB6")),
            new OpcionesPeriferico(), new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria(), TimeSpan.FromMilliseconds(50));
        using var cts = new CancellationTokenSource();
        var corrida = s.EjecutarAsync(cts.Token);
        var limite = DateTime.UtcNow.AddSeconds(20);
        while (s.Intentos < 2)
        {
            Assert.True(DateTime.UtcNow < limite, "no reintentó");
            await Task.Delay(20);
        }
        Assert.NotNull(s.UltimoError);
        Assert.NotEqual(EstadoPeriferico.Anunciando, s.Estado);
        cts.Cancel();
        await corrida.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(EstadoPeriferico.Detenido, s.Estado);
    }
}

/// <summary>Lectura del PacketRequest de prueba: el campo hci_packet y, dentro, sus bytes.</summary>
internal static class MensajesNetsimDePrueba
{
    public static byte[] Hci(byte[] pedido)
    {
        var pos = 0;
        Assert.Equal(0x12, pedido[pos++]); // hci_packet (campo 2)
        var n = Varint(pedido, ref pos);
        var hci = pedido[pos..(pos + n)];
        pos = 0;
        Assert.Equal(0x08, hci[pos++]); // packet_type
        Assert.Equal(0x01, hci[pos++]); // COMMAND
        Assert.Equal(0x12, hci[pos++]); // packet (campo 2)
        var m = Varint(hci, ref pos);
        return hci[pos..(pos + m)];
    }

    private static int Varint(byte[] b, ref int pos)
    {
        int v = 0, s = 0;
        while (true)
        {
            var x = b[pos++];
            v |= (x & 0x7F) << s;
            if ((x & 0x80) == 0)
            {
                return v;
            }
            s += 7;
        }
    }
}
