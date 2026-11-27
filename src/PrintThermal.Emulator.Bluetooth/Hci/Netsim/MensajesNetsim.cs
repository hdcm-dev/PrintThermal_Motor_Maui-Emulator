// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Text;

namespace PrintThermal.Emulator.Bluetooth.Hci.Netsim;

/// <summary>Respuesta de PacketStreamer: un error o un paquete HCI.</summary>
public sealed record RespuestaNetsim(string? Error, PaqueteHci? Hci);

/// <summary>
/// Mensajes de <c>netsim.packet.PacketStreamer/StreamPackets</c> codificados a mano en protobuf (sólo los campos que se usan).
/// Números de campo de los .proto de netsim: PacketRequest{initial_info=1, hci_packet=2, packet=3};
/// PacketResponse{error=1, hci_packet=2, packet=3}; HCIPacket{packet_type=1, packet=2};
/// ChipInfo{name=1, chip=2, device_info=3}; Chip{kind=1, manufacturer=3, product_name=4, address=9};
/// DeviceInfo{name=1, kind=2, version=3}; ChipKind.BLUETOOTH=1.
/// </summary>
public static class MensajesNetsim
{
    public static byte[] PedidoInicial(DireccionBluetooth direccion, string nombreDispositivo)
    {
        var chip = new EscritorProtobuf()
            .Varint(1, 1)
            .Cadena(3, "PrintThermal.Emulator")
            .Cadena(4, nombreDispositivo)
            .Cadena(9, direccion.ComoNetsim())
            .ToArray();
        var dispositivo = new EscritorProtobuf()
            .Cadena(1, nombreDispositivo)
            .Cadena(2, "PRINTER")
            .Cadena(3, "1")
            .ToArray();
        var info = new EscritorProtobuf()
            .Bytes(2, chip)
            .Bytes(3, dispositivo)
            .ToArray();
        return new EscritorProtobuf().Bytes(1, info).ToArray();
    }

    public static byte[] PedidoHci(PaqueteHci paquete)
    {
        var hci = new EscritorProtobuf().Varint(1, (ulong)paquete.Tipo).Bytes(2, paquete.Datos).ToArray();
        return new EscritorProtobuf().Bytes(2, hci).ToArray();
    }

    public static RespuestaNetsim LeerRespuesta(ReadOnlySpan<byte> mensaje)
    {
        string? error = null;
        PaqueteHci? hci = null;
        var r = new LectorProtobuf(mensaje.ToArray());
        while (r.Siguiente(out var campo, out var tipo))
        {
            if (campo == 1 && tipo == 2)
            {
                error = Encoding.UTF8.GetString(r.LeerBytes().Span);
            }
            else if (campo == 2 && tipo == 2)
            {
                var h = new LectorProtobuf(r.LeerBytes());
                ulong tipoHci = 0;
                byte[] datos = [];
                while (h.Siguiente(out var c2, out var t2))
                {
                    if (c2 == 1 && t2 == 0)
                    {
                        tipoHci = h.LeerVarint();
                    }
                    else if (c2 == 2 && t2 == 2)
                    {
                        datos = h.LeerBytes().ToArray();
                    }
                    else
                    {
                        h.Saltar(t2);
                    }
                }
                hci = new PaqueteHci((TipoPaqueteHci)tipoHci, datos);
            }
            else
            {
                r.Saltar(tipo);
            }
        }
        return new RespuestaNetsim(error, hci);
    }
}

/// <summary>Escritor mínimo de protobuf: varint y campos de longitud delimitada.</summary>
internal sealed class EscritorProtobuf
{
    private readonly MemoryStream _m = new();

    public EscritorProtobuf Varint(int campo, ulong valor)
    {
        Crudo((ulong)(campo << 3));
        Crudo(valor);
        return this;
    }

    public EscritorProtobuf Bytes(int campo, ReadOnlySpan<byte> valor)
    {
        Crudo((ulong)((campo << 3) | 2));
        Crudo((ulong)valor.Length);
        _m.Write(valor);
        return this;
    }

    public EscritorProtobuf Cadena(int campo, string valor) => Bytes(campo, Encoding.UTF8.GetBytes(valor));

    public byte[] ToArray() => _m.ToArray();

    private void Crudo(ulong v)
    {
        while (v >= 0x80)
        {
            _m.WriteByte((byte)(v | 0x80));
            v >>= 7;
        }
        _m.WriteByte((byte)v);
    }
}

/// <summary>Lector mínimo de protobuf.</summary>
internal sealed class LectorProtobuf(ReadOnlyMemory<byte> datos)
{
    private int _pos;

    public bool Siguiente(out int campo, out int tipo)
    {
        campo = tipo = 0;
        if (_pos >= datos.Length)
        {
            return false;
        }
        var etiqueta = LeerVarint();
        campo = (int)(etiqueta >> 3);
        tipo = (int)(etiqueta & 7);
        return true;
    }

    public ulong LeerVarint()
    {
        ulong v = 0;
        for (var desplazamiento = 0; desplazamiento < 64; desplazamiento += 7)
        {
            if (_pos >= datos.Length)
            {
                throw new FormatException("varint truncado");
            }
            var b = datos.Span[_pos++];
            v |= (ulong)(b & 0x7F) << desplazamiento;
            if ((b & 0x80) == 0)
            {
                return v;
            }
        }
        throw new FormatException("varint demasiado largo");
    }

    public ReadOnlyMemory<byte> LeerBytes()
    {
        var n = checked((int)LeerVarint());
        if (n < 0 || _pos + n > datos.Length)
        {
            throw new FormatException("campo protobuf truncado");
        }
        var r = datos.Slice(_pos, n);
        _pos += n;
        return r;
    }

    public void Saltar(int tipo)
    {
        switch (tipo)
        {
            case 0:
                LeerVarint();
                break;
            case 1:
                _pos += 8;
                break;
            case 2:
                LeerBytes();
                break;
            case 5:
                _pos += 4;
                break;
            default:
                throw new FormatException($"tipo de campo protobuf no admitido: {tipo}");
        }
    }
}
