// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace PrintThermal.Emulator.Bluetooth.Hci.Netsim;

/// <summary>
/// Transporte HCI por el gRPC de netsim (<c>netsim.packet.PacketStreamer/StreamPackets</c>, flujo bidireccional) sobre
/// HTTP/2 sin TLS, sin paquetes de gRPC: tramas gRPC (1 byte de compresión en 0, 4 bytes de largo big-endian, mensaje)
/// y protobuf a mano. El primer mensaje registra el chip Bluetooth con la dirección fija.
/// </summary>
public sealed class TransporteNetsim : ITransporteHci
{
    public const string Ruta = "/netsim.packet.PacketStreamer/StreamPackets";

    private readonly HttpClient _http;
    private readonly ContenidoDuplex _salida;
    private readonly Task<HttpResponseMessage> _respuesta;
    private readonly CancellationTokenSource _cierre = new();

    private TransporteNetsim(Uri servidor, DireccionBluetooth direccion, string nombre)
    {
        // Sin pings de HTTP/2: netsim corta el flujo con «grpc-status 14: Too many pings» si se le manda uno cada
        // 30 s sin datos (visto en la prueba real del 2026-11-27).
        _http = new HttpClient(new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(5),
        })
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };
        _salida = new ContenidoDuplex();
        _salida.Escribir(MensajesNetsim.PedidoInicial(direccion, nombre));
        var pedido = new HttpRequestMessage(HttpMethod.Post, new Uri(servidor, Ruta))
        {
            Version = HttpVersion.Version20,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact,
            Content = _salida,
        };
        pedido.Headers.TE.ParseAdd("trailers");
        // No se espera a los encabezados de la respuesta: netsim puede no mandarlos hasta tener un paquete.
        _respuesta = _http.SendAsync(pedido, HttpCompletionOption.ResponseHeadersRead, _cierre.Token);
    }

    /// <summary>Abre el flujo con netsim (por ejemplo <c>http://127.0.0.1:8554</c>) y registra el chip.</summary>
    public static TransporteNetsim Conectar(Uri servidor, DireccionBluetooth direccion, string nombre) => new(servidor, direccion, nombre);

    public ValueTask EnviarAsync(PaqueteHci paquete, CancellationToken ct = default)
    {
        _salida.Escribir(MensajesNetsim.PedidoHci(paquete));
        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<PaqueteHci> RecibirAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        using var unido = CancellationTokenSource.CreateLinkedTokenSource(ct, _cierre.Token);
        var respuesta = await _respuesta.WaitAsync(unido.Token);
        if (respuesta.StatusCode != HttpStatusCode.OK)
        {
            throw new IOException($"netsim respondió HTTP {(int)respuesta.StatusCode}");
        }
        VerificarEstado(respuesta.Headers);
        await using var entrada = await respuesta.Content.ReadAsStreamAsync(unido.Token);
        var cabecera = new byte[5];
        while (true)
        {
            if (!await LeerExactoAsync(entrada, cabecera, unido.Token))
            {
                VerificarEstado(respuesta.TrailingHeaders);
                yield break;
            }
            if (cabecera[0] != 0)
            {
                throw new IOException("netsim mandó un mensaje comprimido; no se admite");
            }
            var largo = BinaryPrimitives.ReadUInt32BigEndian(cabecera.AsSpan(1));
            if (largo > 1 << 20)
            {
                throw new IOException($"mensaje de netsim de {largo} bytes");
            }
            var mensaje = new byte[largo];
            if (!await LeerExactoAsync(entrada, mensaje, unido.Token))
            {
                throw new IOException("netsim cortó un mensaje a la mitad");
            }
            var r = MensajesNetsim.LeerRespuesta(mensaje);
            if (r.Error is not null)
            {
                throw new IOException($"netsim: {r.Error}");
            }
            if (r.Hci is { } hci)
            {
                yield return hci;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _salida.Completar();
        await _cierre.CancelAsync();
        try
        {
            (await _respuesta).Dispose();
        }
        catch (Exception)
        {
            // La conexión ya había fallado: no hay respuesta que liberar.
        }
        _http.Dispose();
        _cierre.Dispose();
    }

    private static void VerificarEstado(HttpHeaders encabezados)
    {
        if (encabezados.TryGetValues("grpc-status", out var estado) && estado.FirstOrDefault() is { } e && e != "0")
        {
            var mensaje = encabezados.TryGetValues("grpc-message", out var m) ? Uri.UnescapeDataString(m.FirstOrDefault() ?? "") : "";
            throw new IOException($"netsim cerró el flujo con grpc-status {e}: {mensaje}");
        }
    }

    private static async Task<bool> LeerExactoAsync(Stream s, byte[] destino, CancellationToken ct)
    {
        var leidos = 0;
        while (leidos < destino.Length)
        {
            var n = await s.ReadAsync(destino.AsMemory(leidos), ct);
            if (n == 0)
            {
                if (leidos == 0)
                {
                    return false;
                }
                throw new IOException("netsim cortó un mensaje a la mitad");
            }
            leidos += n;
        }
        return true;
    }

    /// <summary>Cuerpo del pedido que queda abierto: cada mensaje se manda como trama gRPC apenas se escribe.</summary>
    private sealed class ContenidoDuplex : HttpContent
    {
        private readonly Channel<byte[]> _cola = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions { SingleReader = true });

        public ContenidoDuplex() => Headers.ContentType = new MediaTypeHeaderValue("application/grpc");

        public void Escribir(byte[] mensaje)
        {
            var trama = new byte[5 + mensaje.Length];
            BinaryPrimitives.WriteUInt32BigEndian(trama.AsSpan(1), (uint)mensaje.Length);
            mensaje.CopyTo(trama, 5);
            _cola.Writer.TryWrite(trama);
        }

        public void Completar() => _cola.Writer.TryComplete();

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken ct)
        {
            await foreach (var trama in _cola.Reader.ReadAllAsync(ct))
            {
                await stream.WriteAsync(trama, ct);
                await stream.FlushAsync(ct);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
