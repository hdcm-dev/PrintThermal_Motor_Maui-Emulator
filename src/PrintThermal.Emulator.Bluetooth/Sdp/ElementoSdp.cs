// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;
using System.Text;

namespace PrintThermal.Emulator.Bluetooth.Sdp;

/// <summary>Tipos de elemento de datos de SDP (Bluetooth Core, vol. 3, parte B, §3.2).</summary>
public enum TipoElementoSdp : byte
{
    Nulo = 0,
    EnteroSinSigno = 1,
    Entero = 2,
    Uuid = 3,
    Texto = 4,
    Booleano = 5,
    Secuencia = 6,
    Alternativa = 7,
    Url = 8,
}

/// <summary>Elemento de datos de SDP: cabecera (tipo y tamaño) y valor, todo en big-endian.</summary>
public sealed class ElementoSdp
{
    /// <summary>UUID base de Bluetooth: 0000xxxx-0000-1000-8000-00805F9B34FB.</summary>
    private static readonly byte[] UuidBase = Convert.FromHexString("0000000000001000800000805F9B34FB");

    private ElementoSdp(TipoElementoSdp tipo, ulong valor, int tamano, byte[] bytes, IReadOnlyList<ElementoSdp> hijos)
    {
        Tipo = tipo;
        Valor = valor;
        Tamano = tamano;
        Bytes = bytes;
        Hijos = hijos;
    }

    public TipoElementoSdp Tipo { get; }

    /// <summary>Valor de un entero o booleano.</summary>
    public ulong Valor { get; }

    /// <summary>Bytes del valor de un entero (1, 2, 4 u 8).</summary>
    public int Tamano { get; }

    /// <summary>Bytes de un UUID (2, 4 o 16, big-endian), de un texto o de una URL.</summary>
    public byte[] Bytes { get; }

    public IReadOnlyList<ElementoSdp> Hijos { get; }

    public static ElementoSdp Uint8(byte v) => new(TipoElementoSdp.EnteroSinSigno, v, 1, [], []);

    public static ElementoSdp Uint16(ushort v) => new(TipoElementoSdp.EnteroSinSigno, v, 2, [], []);

    public static ElementoSdp Uint32(uint v) => new(TipoElementoSdp.EnteroSinSigno, v, 4, [], []);

    public static ElementoSdp Uuid16(ushort v) => new(TipoElementoSdp.Uuid, 0, 2, [(byte)(v >> 8), (byte)v], []);

    public static ElementoSdp Uuid128(byte[] bigEndian) =>
        bigEndian.Length == 16 ? new(TipoElementoSdp.Uuid, 0, 16, (byte[])bigEndian.Clone(), []) : throw new ArgumentException("16 bytes");

    public static ElementoSdp Texto(string s) => new(TipoElementoSdp.Texto, 0, 0, Encoding.UTF8.GetBytes(s), []);

    public static ElementoSdp Secuencia(params ElementoSdp[] hijos) => new(TipoElementoSdp.Secuencia, 0, 0, [], hijos);

    /// <summary>El UUID llevado a 128 bits con el UUID base.</summary>
    public byte[] UuidCompleto()
    {
        if (Tipo != TipoElementoSdp.Uuid)
        {
            throw new InvalidOperationException("no es un UUID");
        }
        if (Bytes.Length == 16)
        {
            return (byte[])Bytes.Clone();
        }
        var u = (byte[])UuidBase.Clone();
        Bytes.CopyTo(u, 4 - Bytes.Length);
        return u;
    }

    public string ComoTexto() => Encoding.UTF8.GetString(Bytes);

    /// <summary>Este elemento y todos los que contiene.</summary>
    public IEnumerable<ElementoSdp> Recorrer()
    {
        yield return this;
        foreach (var h in Hijos)
        {
            foreach (var d in h.Recorrer())
            {
                yield return d;
            }
        }
    }

    public byte[] Codificar()
    {
        var b = new List<byte>();
        Escribir(b);
        return [.. b];
    }

    public void Escribir(List<byte> b)
    {
        var t = (byte)((byte)Tipo << 3);
        switch (Tipo)
        {
            case TipoElementoSdp.Nulo:
                b.Add(0);
                return;
            case TipoElementoSdp.EnteroSinSigno or TipoElementoSdp.Entero:
                b.Add((byte)(t | IndiceDeTamano(Tamano)));
                for (var i = Tamano - 1; i >= 0; i--)
                {
                    b.Add((byte)(Valor >> (8 * i)));
                }
                return;
            case TipoElementoSdp.Booleano:
                b.Add(t);
                b.Add((byte)(Valor != 0 ? 1 : 0));
                return;
            case TipoElementoSdp.Uuid:
                b.Add((byte)(t | IndiceDeTamano(Bytes.Length)));
                b.AddRange(Bytes);
                return;
            case TipoElementoSdp.Texto or TipoElementoSdp.Url:
                EscribirLongitud(b, t, Bytes.Length);
                b.AddRange(Bytes);
                return;
            default:
                var cuerpo = new List<byte>();
                foreach (var h in Hijos)
                {
                    h.Escribir(cuerpo);
                }
                EscribirLongitud(b, t, cuerpo.Count);
                b.AddRange(cuerpo);
                return;
        }
    }

    public static ElementoSdp Decodificar(ReadOnlySpan<byte> datos)
    {
        var pos = 0;
        var e = Leer(datos, ref pos);
        return pos == datos.Length ? e : throw new FormatException("sobran bytes después del elemento SDP");
    }

    public static ElementoSdp Leer(ReadOnlySpan<byte> d, ref int pos)
    {
        if (pos >= d.Length)
        {
            throw new FormatException("elemento SDP truncado");
        }
        var cabecera = d[pos++];
        var tipo = (TipoElementoSdp)(cabecera >> 3);
        var indice = cabecera & 0x07;
        int largo;
        if (tipo == TipoElementoSdp.Nulo)
        {
            return new(tipo, 0, 0, [], []);
        }
        if (indice <= 4)
        {
            largo = indice == 4 ? 16 : 1 << indice;
        }
        else
        {
            var n = indice == 5 ? 1 : indice == 6 ? 2 : 4;
            Exigir(d, pos, n);
            largo = n == 1 ? d[pos] : n == 2 ? BinaryPrimitives.ReadUInt16BigEndian(d[pos..]) : checked((int)BinaryPrimitives.ReadUInt32BigEndian(d[pos..]));
            pos += n;
        }
        Exigir(d, pos, largo);
        var valor = d.Slice(pos, largo);
        pos += largo;
        switch (tipo)
        {
            case TipoElementoSdp.EnteroSinSigno or TipoElementoSdp.Entero or TipoElementoSdp.Booleano:
                if (largo > 8)
                {
                    throw new FormatException("entero SDP de más de 64 bits");
                }
                ulong v = 0;
                foreach (var x in valor)
                {
                    v = (v << 8) | x;
                }
                return new(tipo, v, largo, [], []);
            case TipoElementoSdp.Uuid:
                if (largo is not (2 or 4 or 16))
                {
                    throw new FormatException("UUID SDP de tamaño inválido");
                }
                return new(tipo, 0, 0, valor.ToArray(), []);
            case TipoElementoSdp.Texto or TipoElementoSdp.Url:
                return new(tipo, 0, 0, valor.ToArray(), []);
            case TipoElementoSdp.Secuencia or TipoElementoSdp.Alternativa:
                var hijos = new List<ElementoSdp>();
                var p = 0;
                while (p < valor.Length)
                {
                    hijos.Add(Leer(valor, ref p));
                }
                return new(tipo, 0, 0, [], hijos);
            default:
                throw new FormatException($"tipo de elemento SDP desconocido: {(int)tipo}");
        }
    }

    private static void Exigir(ReadOnlySpan<byte> d, int pos, int n)
    {
        if (n < 0 || pos + n > d.Length)
        {
            throw new FormatException("elemento SDP truncado");
        }
    }

    private static byte IndiceDeTamano(int bytes) => bytes switch
    {
        1 => 0,
        2 => 1,
        4 => 2,
        8 => 3,
        16 => 4,
        _ => throw new ArgumentException($"tamaño SDP inválido: {bytes}"),
    };

    private static void EscribirLongitud(List<byte> b, byte t, int largo)
    {
        if (largo <= byte.MaxValue)
        {
            b.Add((byte)(t | 5));
            b.Add((byte)largo);
        }
        else if (largo <= ushort.MaxValue)
        {
            b.Add((byte)(t | 6));
            b.Add((byte)(largo >> 8));
            b.Add((byte)largo);
        }
        else
        {
            b.Add((byte)(t | 7));
            b.Add((byte)(largo >> 24));
            b.Add((byte)(largo >> 16));
            b.Add((byte)(largo >> 8));
            b.Add((byte)largo);
        }
    }
}
