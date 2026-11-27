// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Sdp;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Tests.Bluetooth;

/// <summary>REQ-01 de PAR-PTE-03: «58HB6» con el perfil de puerto serie (UUID 0x1101) en SDP.</summary>
public class SdpTests
{
    /// <summary>UUID de 128 bits del perfil de puerto serie, como lo pide Android (createRfcommSocketToServiceRecord).</summary>
    private static readonly byte[] Spp128 = Convert.FromHexString("0000110100001000800000805F9B34FB");

    /// <summary>ServiceSearchAttributeRequest (0x06) armado byte a byte: patrón {UUID}, máximo, atributos 0x0000-0xFFFF.</summary>
    private static byte[] PedidoBusquedaAtributos(ushort transaccion, byte[] uuidElemento, ushort maximo, byte[] continuacion)
    {
        var p = new List<byte>();
        p.AddRange([0x35, (byte)uuidElemento.Length]);
        p.AddRange(uuidElemento);
        p.AddRange([(byte)(maximo >> 8), (byte)maximo]);
        p.AddRange([0x35, 0x05, 0x0A, 0x00, 0x00, 0xFF, 0xFF]);
        p.Add((byte)continuacion.Length);
        p.AddRange(continuacion);
        return [0x06, (byte)(transaccion >> 8), (byte)transaccion, (byte)(p.Count >> 8), (byte)p.Count, .. p];
    }

    private static ServidorSdp ServidorConSpp()
    {
        var s = new ServidorSdp();
        s.Agregar(0x00010001, RegistroSpp.Crear(0x00010001, canal: 1, nombre: "58HB6"));
        return s;
    }

    /// <summary>Pide con continuación hasta el final y devuelve las listas de atributos completas.</summary>
    private static byte[] BuscarTodo(ServidorSdp s, byte[] uuidElemento, ushort maximo, int mtu)
    {
        var acumulado = new List<byte>();
        byte[] cont = [];
        for (var vuelta = 0; vuelta < 100; vuelta++)
        {
            var r = s.Responder(PedidoBusquedaAtributos((ushort)(vuelta + 1), uuidElemento, maximo, cont), mtu);
            Assert.True(r.Length <= mtu, $"respuesta de {r.Length} bytes con MTU {mtu}");
            Assert.Equal(0x07, r[0]);
            Assert.Equal(vuelta + 1, BinaryPrimitives.ReadUInt16BigEndian(r.AsSpan(1)));
            Assert.Equal(r.Length - 5, BinaryPrimitives.ReadUInt16BigEndian(r.AsSpan(3)));
            var cuenta = BinaryPrimitives.ReadUInt16BigEndian(r.AsSpan(5));
            Assert.True(cuenta <= maximo);
            acumulado.AddRange(r.AsSpan(7, cuenta).ToArray());
            var largoCont = r[7 + cuenta];
            cont = r.AsSpan(8 + cuenta, largoCont).ToArray();
            Assert.Equal(r.Length, 8 + cuenta + largoCont);
            if (largoCont == 0)
            {
                return [.. acumulado];
            }
        }
        Assert.Fail("la continuación no terminó");
        return [];
    }

    private static void Contiene(byte[] donde, byte[] que)
    {
        for (var i = 0; i + que.Length <= donde.Length; i++)
        {
            if (donde.AsSpan(i, que.Length).SequenceEqual(que))
            {
                return;
            }
        }
        Assert.Fail($"no aparece {Convert.ToHexString(que)} en {Convert.ToHexString(donde)}");
    }

    private static ElementoSdp Atributo(ElementoSdp registro, ushort id)
    {
        Assert.Equal(TipoElementoSdp.Secuencia, registro.Tipo);
        for (var i = 0; i + 1 < registro.Hijos.Count; i += 2)
        {
            if (registro.Hijos[i].Valor == id)
            {
                return registro.Hijos[i + 1];
            }
        }
        Assert.Fail($"falta el atributo 0x{id:X4}");
        return null!;
    }

    // REQ-01
    [Fact]
    public void ElRegistroSdpPublicaSppConNombre58HB6()
    {
        var crudo = BuscarTodo(ServidorConSpp(), [0x1C, .. Spp128], 0xFFFF, 672);

        // Bytes esperados escritos a mano (sin el codificador de producción): id de atributo (uint16) y valor.
        Contiene(crudo, [0x09, 0x00, 0x01, 0x35, 0x03, 0x19, 0x11, 0x01]); // ServiceClassIDList = {UUID16 0x1101}
        Contiene(crudo, [0x09, 0x00, 0x04, 0x35, 0x0C, 0x35, 0x03, 0x19, 0x01, 0x00, 0x35, 0x05, 0x19, 0x00, 0x03, 0x08, 0x01]); // {{L2CAP}, {RFCOMM, 1}}
        Contiene(crudo, [0x09, 0x01, 0x00, 0x25, 0x05, (byte)'5', (byte)'8', (byte)'H', (byte)'B', (byte)'6']); // ServiceName
        Assert.Equal(0x35, crudo[0]); // lista de registros: secuencia

        var listas = ElementoSdp.Decodificar(crudo);
        var registro = Assert.Single(listas.Hijos);

        var ids = registro.Hijos.Where((_, i) => i % 2 == 0).Select(e => (ushort)e.Valor).ToList();
        Assert.Equal(ids.Order().ToList(), ids); // en orden ascendente

        Assert.Equal(0x00010001UL, Atributo(registro, 0x0000).Valor);
        var clases = Atributo(registro, 0x0001);
        Assert.Contains(clases.Hijos, u => u.Tipo == TipoElementoSdp.Uuid && u.UuidCompleto().SequenceEqual(Spp128));

        var protocolos = Atributo(registro, 0x0004).Hijos;
        Assert.Equal(2, protocolos.Count);
        Assert.Equal(0x0100, BinaryPrimitives.ReadUInt16BigEndian(protocolos[0].Hijos[0].UuidCompleto().AsSpan(2))); // L2CAP
        Assert.Equal(0x0003, BinaryPrimitives.ReadUInt16BigEndian(protocolos[1].Hijos[0].UuidCompleto().AsSpan(2))); // RFCOMM
        Assert.Equal(1UL, protocolos[1].Hijos[1].Valor); // canal 1

        var idiomas = Atributo(registro, 0x0006).Hijos;
        var nombre = Atributo(registro, (ushort)(idiomas[2].Valor + 0x0000));
        Assert.Equal("58HB6", nombre.ComoTexto());
    }

    [Fact]
    public void LaBusquedaConUuidDe16BitsYConContinuacionDevuelveLoMismo()
    {
        var s = ServidorConSpp();
        var completo = BuscarTodo(s, [0x1C, .. Spp128], 0xFFFF, 672);
        var corto = BuscarTodo(s, [0x19, 0x11, 0x01], 0xFFFF, 672);
        var enTrozos = BuscarTodo(s, [0x19, 0x11, 0x01], 17, 48);
        Assert.Equal(completo, corto);
        Assert.Equal(completo, enTrozos);
    }

    [Fact]
    public void UnUuidQueNoEstaNoDevuelveRegistros()
    {
        var listas = ElementoSdp.Decodificar(BuscarTodo(ServidorConSpp(), [0x19, 0x11, 0x0A], 0xFFFF, 672));
        Assert.Empty(listas.Hijos);
    }

    [Fact]
    public void BusquedaDeServiciosYPedidoDeAtributosPorHandle()
    {
        var s = ServidorConSpp();
        // ServiceSearchRequest (0x02): patrón {0x1101}, hasta 10 registros, sin continuación.
        var r = s.Responder([0x02, 0x00, 0x01, 0x00, 0x08, 0x35, 0x03, 0x19, 0x11, 0x01, 0x00, 0x0A, 0x00], 672);
        Assert.Equal(0x03, r[0]);
        Assert.Equal(1, BinaryPrimitives.ReadUInt16BigEndian(r.AsSpan(5)));
        Assert.Equal(1, BinaryPrimitives.ReadUInt16BigEndian(r.AsSpan(7)));
        Assert.Equal(0x00010001U, BinaryPrimitives.ReadUInt32BigEndian(r.AsSpan(9)));
        Assert.Equal(0, r[13]);

        // ServiceAttributeRequest (0x04): handle, máximo, sólo 0x0004.
        var a = s.Responder([0x04, 0x00, 0x02, 0x00, 0x0C, 0x00, 0x01, 0x00, 0x01, 0x00, 0x64, 0x35, 0x03, 0x09, 0x00, 0x04, 0x00], 672);
        Assert.Equal(0x05, a[0]);
        var cuenta = BinaryPrimitives.ReadUInt16BigEndian(a.AsSpan(5));
        var lista = ElementoSdp.Decodificar(a.AsSpan(7, cuenta));
        Assert.Equal(2, lista.Hijos.Count);
        Assert.Equal(0x0004UL, lista.Hijos[0].Valor);

        // Handle inexistente: ErrorResponse 0x0002.
        var e = s.Responder([0x04, 0x00, 0x03, 0x00, 0x0C, 0x00, 0x09, 0x00, 0x09, 0x00, 0x64, 0x35, 0x03, 0x09, 0x00, 0x04, 0x00], 672);
        Assert.Equal(new byte[] { 0x01, 0x00, 0x03, 0x00, 0x02, 0x00, 0x02 }, e);
    }

    [Fact]
    public async Task ElPerifericoSeAnunciaComo58HB6ConDireccionFijaYEscaneoAbierto()
    {
        var c = new ControladorFalso();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico(), new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria());
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(EstadoPeriferico.Anunciando, p.Estado);
        Assert.Equal(ControladorFalso.Propia, p.DireccionLocal);

        var nombre = ControladorFalso.Parametros(await c.EsperarComandoAsync(0x0C13));
        Assert.Equal(248, nombre.Length);
        Assert.Equal("58HB6"u8.ToArray(), nombre[..5]);
        Assert.All(nombre[5..], b => Assert.Equal(0, b));

        Assert.Equal(new byte[] { 0x80, 0x06, 0x04 }, ControladorFalso.Parametros(await c.EsperarComandoAsync(0x0C24)));
        Assert.Equal(new byte[] { 0x01 }, ControladorFalso.Parametros(await c.EsperarComandoAsync(0x0C56)));
        Assert.Equal(new byte[] { 0x03 }, ControladorFalso.Parametros(await c.EsperarComandoAsync(0x0C1A)));

        var eir = ControladorFalso.Parametros(await c.EsperarComandoAsync(0x0C52));
        Assert.Equal(241, eir.Length);
        Assert.Equal(new byte[] { 0x00, 0x06, 0x09, (byte)'5', (byte)'8', (byte)'H', (byte)'B', (byte)'6', 0x03, 0x03, 0x01, 0x11 }, eir[..12]);

        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
