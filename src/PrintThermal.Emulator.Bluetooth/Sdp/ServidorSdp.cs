// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;

namespace PrintThermal.Emulator.Bluetooth.Sdp;

/// <summary>
/// Servidor SDP (Bluetooth Core, vol. 3, parte B, §4): ServiceSearch (0x02), ServiceAttribute (0x04) y
/// ServiceSearchAttribute (0x06), con estado de continuación cuando la respuesta no entra en el máximo pedido o en el MTU.
/// </summary>
public sealed class ServidorSdp
{
    private const byte ErrorPdu = 0x01;

    private readonly SortedDictionary<uint, SortedDictionary<ushort, ElementoSdp>> _registros = [];

    public void Agregar(uint handle, IReadOnlyDictionary<ushort, ElementoSdp> atributos) =>
        _registros[handle] = new SortedDictionary<ushort, ElementoSdp>(atributos.ToDictionary(a => a.Key, a => a.Value));

    /// <summary>Contesta un PDU SDP. <paramref name="mtu"/> es el MTU L2CAP del otro lado (tope de la respuesta).</summary>
    public byte[] Responder(ReadOnlySpan<byte> pedido, int mtu)
    {
        if (pedido.Length < 5)
        {
            return Error(0, 0x0003);
        }
        var pdu = pedido[0];
        var transaccion = BinaryPrimitives.ReadUInt16BigEndian(pedido[1..]);
        var largo = BinaryPrimitives.ReadUInt16BigEndian(pedido[3..]);
        if (largo != pedido.Length - 5)
        {
            return Error(transaccion, 0x0004);
        }
        try
        {
            var p = pedido[5..];
            return pdu switch
            {
                0x02 => BusquedaDeServicios(transaccion, p, mtu),
                0x04 => Atributos(transaccion, p, mtu),
                0x06 => BusquedaDeAtributos(transaccion, p, mtu),
                _ => Error(transaccion, 0x0003),
            };
        }
        catch (FormatException)
        {
            return Error(transaccion, 0x0003);
        }
        catch (ErrorSdpException e)
        {
            return Error(transaccion, e.Codigo);
        }
    }

    private sealed class ErrorSdpException(ushort codigo) : Exception
    {
        public ushort Codigo { get; } = codigo;
    }

    private byte[] BusquedaDeServicios(ushort transaccion, ReadOnlySpan<byte> p, int mtu)
    {
        var pos = 0;
        var patron = ElementoSdp.Leer(p, ref pos);
        var maximo = LeerU16(p, ref pos);
        LeerContinuacion(p, ref pos); // la lista de handles entra siempre en una sola respuesta
        var handles = Coincidentes(patron).Take(Math.Min(maximo, Math.Max(0, (mtu - 10) / 4))).ToList();
        var r = new List<byte>();
        AgregarU16(r, (ushort)handles.Count);
        AgregarU16(r, (ushort)handles.Count);
        foreach (var h in handles)
        {
            r.AddRange([(byte)(h >> 24), (byte)(h >> 16), (byte)(h >> 8), (byte)h]);
        }
        r.Add(0);
        return Pdu(0x03, transaccion, r);
    }

    private byte[] Atributos(ushort transaccion, ReadOnlySpan<byte> p, int mtu)
    {
        var pos = 0;
        if (p.Length < 4)
        {
            throw new FormatException();
        }
        var handle = BinaryPrimitives.ReadUInt32BigEndian(p);
        pos += 4;
        var maximo = LeerU16(p, ref pos);
        var ids = ElementoSdp.Leer(p, ref pos);
        var continuacion = LeerContinuacion(p, ref pos);
        if (!_registros.TryGetValue(handle, out var registro))
        {
            throw new ErrorSdpException(0x0002);
        }
        var completo = ListaDeAtributos(registro, ids).Codificar();
        return Trozo(0x05, transaccion, completo, continuacion, maximo, mtu);
    }

    private byte[] BusquedaDeAtributos(ushort transaccion, ReadOnlySpan<byte> p, int mtu)
    {
        var pos = 0;
        var patron = ElementoSdp.Leer(p, ref pos);
        var maximo = LeerU16(p, ref pos);
        var ids = ElementoSdp.Leer(p, ref pos);
        var continuacion = LeerContinuacion(p, ref pos);
        var listas = Coincidentes(patron).Select(h => ListaDeAtributos(_registros[h], ids)).ToArray();
        var completo = ElementoSdp.Secuencia(listas).Codificar();
        return Trozo(0x07, transaccion, completo, continuacion, maximo, mtu);
    }

    /// <summary>Un trozo de la respuesta completa: AttributeListByteCount, los bytes y el estado de continuación.</summary>
    private static byte[] Trozo(byte pdu, ushort transaccion, byte[] completo, int desde, ushort maximo, int mtu)
    {
        if (desde > completo.Length)
        {
            throw new ErrorSdpException(0x0005);
        }
        // Cabecera 5 + cuenta 2 + continuación de hasta 3 bytes.
        var tope = Math.Min(maximo, mtu - 10);
        if (tope <= 0)
        {
            throw new ErrorSdpException(0x0004);
        }
        var n = Math.Min(tope, completo.Length - desde);
        var r = new List<byte>();
        AgregarU16(r, (ushort)n);
        r.AddRange(completo.AsSpan(desde, n).ToArray());
        var siguiente = desde + n;
        if (siguiente < completo.Length)
        {
            r.Add(2);
            AgregarU16(r, (ushort)siguiente);
        }
        else
        {
            r.Add(0);
        }
        return Pdu(pdu, transaccion, r);
    }

    private IEnumerable<uint> Coincidentes(ElementoSdp patron)
    {
        if (patron.Tipo != TipoElementoSdp.Secuencia || patron.Hijos.Count == 0 || patron.Hijos.Any(u => u.Tipo != TipoElementoSdp.Uuid))
        {
            throw new FormatException();
        }
        foreach (var (handle, registro) in _registros)
        {
            var uuids = registro.Values.SelectMany(v => v.Recorrer()).Where(e => e.Tipo == TipoElementoSdp.Uuid)
                .Select(e => Convert.ToHexString(e.UuidCompleto())).ToHashSet();
            if (patron.Hijos.All(u => uuids.Contains(Convert.ToHexString(u.UuidCompleto()))))
            {
                yield return handle;
            }
        }
    }

    private static ElementoSdp ListaDeAtributos(SortedDictionary<ushort, ElementoSdp> registro, ElementoSdp ids)
    {
        if (ids.Tipo != TipoElementoSdp.Secuencia || ids.Hijos.Count == 0)
        {
            throw new FormatException();
        }
        var rangos = ids.Hijos.Select(e => e switch
        {
            { Tipo: TipoElementoSdp.EnteroSinSigno, Tamano: 2 } => ((ushort)e.Valor, (ushort)e.Valor),
            { Tipo: TipoElementoSdp.EnteroSinSigno, Tamano: 4 } => ((ushort)(e.Valor >> 16), (ushort)e.Valor),
            _ => throw new FormatException(),
        }).ToList();
        var pares = new List<ElementoSdp>();
        foreach (var (id, valor) in registro)
        {
            if (rangos.Any(r => id >= r.Item1 && id <= r.Item2))
            {
                pares.Add(ElementoSdp.Uint16(id));
                pares.Add(valor);
            }
        }
        return ElementoSdp.Secuencia([.. pares]);
    }

    private static ushort LeerU16(ReadOnlySpan<byte> p, ref int pos)
    {
        if (pos + 2 > p.Length)
        {
            throw new FormatException();
        }
        var v = BinaryPrimitives.ReadUInt16BigEndian(p[pos..]);
        pos += 2;
        return v;
    }

    /// <summary>Lee el estado de continuación; devuelve el desplazamiento que codifica (0 si no hay).</summary>
    private static int LeerContinuacion(ReadOnlySpan<byte> p, ref int pos)
    {
        if (pos >= p.Length)
        {
            throw new FormatException();
        }
        var n = p[pos++];
        if (n == 0)
        {
            return 0;
        }
        if (n != 2 || pos + 2 > p.Length)
        {
            throw new ErrorSdpException(0x0005);
        }
        var desde = BinaryPrimitives.ReadUInt16BigEndian(p[pos..]);
        pos += 2;
        return desde;
    }

    private static void AgregarU16(List<byte> r, ushort v)
    {
        r.Add((byte)(v >> 8));
        r.Add((byte)v);
    }

    private static byte[] Pdu(byte pdu, ushort transaccion, List<byte> parametros) =>
        [pdu, (byte)(transaccion >> 8), (byte)transaccion, (byte)(parametros.Count >> 8), (byte)parametros.Count, .. parametros];

    private static byte[] Error(ushort transaccion, ushort codigo) =>
        [ErrorPdu, (byte)(transaccion >> 8), (byte)transaccion, 0x00, 0x02, (byte)(codigo >> 8), (byte)codigo];
}

/// <summary>Registro SDP del perfil de puerto serie (SPP) sobre RFCOMM.</summary>
public static class RegistroSpp
{
    public const ushort PerfilPuertoSerie = 0x1101;

    /// <summary>
    /// Los atributos del registro que usó la prueba corta (handle, clases de servicio, protocolos L2CAP + RFCOMM con el canal,
    /// grupo de navegación pública), más el perfil SPP 1.2, la base de idioma y el nombre del servicio.
    /// </summary>
    public static IReadOnlyDictionary<ushort, ElementoSdp> Crear(uint handle, byte canal, string nombre) => new Dictionary<ushort, ElementoSdp>
    {
        [0x0000] = ElementoSdp.Uint32(handle),
        [0x0001] = ElementoSdp.Secuencia(ElementoSdp.Uuid16(PerfilPuertoSerie)),
        [0x0004] = ElementoSdp.Secuencia(
            ElementoSdp.Secuencia(ElementoSdp.Uuid16(0x0100)),
            ElementoSdp.Secuencia(ElementoSdp.Uuid16(0x0003), ElementoSdp.Uint8(canal))),
        [0x0005] = ElementoSdp.Secuencia(ElementoSdp.Uuid16(0x1002)),
        [0x0006] = ElementoSdp.Secuencia(ElementoSdp.Uint16(0x656E), ElementoSdp.Uint16(0x006A), ElementoSdp.Uint16(0x0100)),
        [0x0009] = ElementoSdp.Secuencia(ElementoSdp.Secuencia(ElementoSdp.Uuid16(PerfilPuertoSerie), ElementoSdp.Uint16(0x0102))),
        [0x0100] = ElementoSdp.Texto(nombre),
    };
}
