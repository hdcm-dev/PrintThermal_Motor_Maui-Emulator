// cdd: REQ-HIS-Ticket-A-Png
using System.Text;

namespace PrintThermal.Emulator.Core.EscPos;

/// <summary>
/// Intérprete ESC/POS de 58 mm. Recibe los bytes en trozos de cualquier tamaño (la librería cliente manda trozos fijos de
/// 256 que no respetan los límites de los comandos), dibuja cada ticket y lo entrega al terminar; responde las consultas
/// de estado en tiempo real. Las cargas de GS v 0, GS ( k, GS ( L y FS q se consumen por longitud (Especificacion-58HB6 §4.3).
/// </summary>
/// <remarks>
/// Un ticket empieza con ESC @ o con el primer comando que imprime, y termina con GS V (corte), con un ESC @ que llega
/// después de contenido, o con <see cref="Finalizar"/>. Las consultas fuera de un ticket no forman parte de ninguno.
/// No es seguro para hilos: una instancia por conexión.
/// </remarks>
public sealed class Interprete
{
    private enum Alineacion { Izquierda, Centro, Derecha }

    private readonly record struct Celda(int Codigo, bool Negrita, int EscalaX, int EscalaY, int Subrayado);

    /// <summary>Tope de la carga de un comando (GS v 0 de 384 x 65535 puntos ocupa 3 MiB): más que esto se descarta.</summary>
    private const long CargaMaxima = 4L << 20;

    /// <summary>Avisos que guarda un ticket; los siguientes se cuentan en un último aviso (el evento <see cref="Aviso"/> los emite todos).</summary>
    private const int AvisosMaximos = 200;


    private readonly PerfilDeImpresora _perfil;
    private byte[] _pendiente = new byte[1024];
    private int _inicio;
    private int _largoPendiente;

    // Ticket en curso.
    private MemoryStream? _bytes;
    private List<string> _avisos = [];
    private Lienzo _lienzo;
    private int _y;
    private bool _contenido;
    private bool _altoAgotado;
    private int _avisosOmitidos;

    // Estado de la impresora.
    private Alineacion _alineacion;
    private bool _negrita;
    private int _subrayado;
    private int _escalaX = 1, _escalaY = 1;
    private int _interlineado;
    private Encoding _tabla = TablasDeCaracteres.Pc437;
    private readonly List<Celda> _linea = [];
    private int _anchoLinea;
    private int _moduloQr = 3;
    private CodigoQr.Correccion _correccionQr = CodigoQr.Correccion.L;
    private byte[] _datosQr = [];
    private int _altoBarras, _anchoModulo, _hri;

    public Interprete(PerfilDeImpresora? perfil = null)
    {
        _perfil = perfil ?? new PerfilDeImpresora();
        _lienzo = new Lienzo(_perfil.AnchoPuntos, _perfil.AltoMaximo, _perfil.AltoMaximo - _perfil.Margen);
        Reiniciar();
    }

    /// <summary>Un ticket terminó: bytes, PNG y avisos.</summary>
    public event Action<TicketInterpretado>? TicketTerminado;

    /// <summary>Bytes que la impresora devuelve al anfitrión (DLE EOT, GS I, GS ( L fn 48).</summary>
    public event Action<byte[]>? Respuesta;

    /// <summary>Todo aviso, dentro o fuera de un ticket (comandos desconocidos o ignorados, recortes, cortes sueltos).</summary>
    public event Action<string>? Aviso;

    /// <summary>Agrega un trozo recibido e interpreta todos los comandos que ya están completos.</summary>
    public void Alimentar(ReadOnlySpan<byte> trozo)
    {
        if (_inicio + _largoPendiente + trozo.Length > _pendiente.Length)
        {
            // Compacta sólo cuando hace falta lugar: con trozos de 1 byte sobre una carga grande no copia en cada llamada.
            if (_largoPendiente + trozo.Length > _pendiente.Length)
                Array.Resize(ref _pendiente, Math.Max(_pendiente.Length * 2, _largoPendiente + trozo.Length));
            Array.Copy(_pendiente, _inicio, _pendiente, 0, _largoPendiente);
            _inicio = 0;
        }
        trozo.CopyTo(_pendiente.AsSpan(_inicio + _largoPendiente));
        _largoPendiente += trozo.Length;

        while (_largoPendiente > 0)
        {
            int consumidos = Paso(_pendiente.AsSpan(_inicio, _largoPendiente));
            if (consumidos == 0) break;
            _inicio += consumidos;
            _largoPendiente -= consumidos;
            // Tope por ticket, medido entre comandos: un flujo sin fin no agota la memoria.
            if (_bytes is not null && _bytes.Length > _perfil.BytesMaximosPorTicket)
            {
                Avisar($"el ticket superó {_perfil.BytesMaximosPorTicket} bytes: se cierra acá");
                CerrarTicket();
            }
        }
        if (_largoPendiente == 0) _inicio = 0;
    }

    /// <summary>Fin de la conexión: cierra el ticket en curso si imprimió algo.</summary>
    public void Finalizar()
    {
        if (_largoPendiente > 0)
        {
            if (_bytes is not null) _bytes.Write(_pendiente, _inicio, _largoPendiente);
            Avisar($"comando incompleto al terminar: {_largoPendiente} bytes desde {Hex(_pendiente.AsSpan(_inicio, Math.Min(4, _largoPendiente)))}");
            _inicio = _largoPendiente = 0;
        }
        if (_bytes is null) return;
        if (_contenido || _linea.Count > 0)
        {
            CerrarTicket();
        }
        else
        {
            Avisar("ticket sin contenido descartado al terminar");
            _bytes = null;
        }
    }

    // ───────────────────────── despacho ─────────────────────────

    private int Paso(ReadOnlySpan<byte> b)
    {
        byte c = b[0];
        switch (c)
        {
            case 0x1B: return b.Length < 2 ? 0 : Esc(b);
            case 0x1D: return b.Length < 2 ? 0 : Gs(b);
            case 0x1C: return b.Length < 2 ? 0 : Fs(b);
            case 0x10: return b.Length < 2 ? 0 : Dle(b);
            case 0x0A:
                Imprime(b[..1]);
                ImprimirLinea(avanzarSiVacia: true);
                return 1;
            case 0x0D:
                Imprime(b[..1]);
                return 1;
            case 0x09:
                Imprime(b[..1]);
                Tabulador();
                return 1;
        }
        if (c < 0x20)
        {
            Imprime(b[..1]);
            Avisar($"byte de control {c:X2} ignorado en el byte {Posicion(1)}");
            return 1;
        }
        Imprime(b[..1]);
        AgregarCaracter(c);
        return 1;
    }

    private int Esc(ReadOnlySpan<byte> b)
    {
        byte f = b[1];
        int largo = f switch
        {
            0x40 or 0x32 => 2,
            0x61 or 0x21 or 0x45 or 0x47 or 0x2D or 0x74 or 0x64 or 0x4A or 0x33 or 0x4D => 3,
            0x20 or 0x52 or 0x7B or 0x56 or 0x3D or 0x25 or 0x55 or 0x3F or 0x72 or 0x54 => 3,
            0x57 => 10,
            0x63 or 0x24 or 0x5C => 4,
            0x70 => 5,
            0x2A => b.Length < 5 ? 0 : 5 + (b[3] | b[4] << 8) * (b[2] <= 1 ? 1 : 3),
            0x44 => HastaNul(b, 2, 34),
            _ => 2,
        };
        if (largo == 0 || b.Length < largo) return 0;
        var cmd = b[..largo];
        switch (f)
        {
            case 0x40:
                if (_bytes is not null && (_contenido || _linea.Count > 0)) CerrarTicket();
                Configura(cmd);
                Reiniciar();
                break;
            case 0x61:
                Configura(cmd);
                _alineacion = (cmd[2] % 48) switch { 1 => Alineacion.Centro, 2 => Alineacion.Derecha, _ => Alineacion.Izquierda };
                break;
            case 0x21:
                Configura(cmd);
                if ((cmd[2] & 0x01) != 0) Avisar("ESC ! con fuente B: se dibuja con la fuente A");
                _negrita = (cmd[2] & 0x08) != 0;
                _escalaY = (cmd[2] & 0x10) != 0 ? 2 : 1;
                _escalaX = (cmd[2] & 0x20) != 0 ? 2 : 1;
                _subrayado = (cmd[2] & 0x80) != 0 ? 1 : 0;
                break;
            case 0x45:
            case 0x47:
                Configura(cmd);
                _negrita = (cmd[2] & 1) == 1;
                break;
            case 0x2D:
                Configura(cmd);
                _subrayado = cmd[2] % 48 is 1 or 2 ? cmd[2] % 48 : 0;
                break;
            case 0x74:
                Configura(cmd);
                var tabla = TablasDeCaracteres.Para(cmd[2]);
                if (tabla is null) Avisar($"ESC t {cmd[2]}: tabla desconocida, se sigue con la vigente");
                else _tabla = tabla;
                break;
            case 0x64:
                Imprime(cmd);
                ImprimirLinea(avanzarSiVacia: false);
                Avanzar(cmd[2] * _interlineado);
                break;
            case 0x4A:
                Imprime(cmd);
                ImprimirLinea(avanzarSiVacia: false);
                Avanzar(cmd[2]);
                break;
            case 0x32:
                Configura(cmd);
                _interlineado = _perfil.Interlineado;
                break;
            case 0x33:
                Configura(cmd);
                _interlineado = cmd[2];
                break;
            case 0x4D:
                Configura(cmd);
                if ((cmd[2] & 1) == 1) Avisar("ESC M: fuente B no disponible, se dibuja con la fuente A");
                break;
            case 0x20 or 0x52 or 0x7B or 0x56 or 0x3D or 0x25 or 0x55 or 0x3F or 0x72 or 0x54 or 0x57 or 0x63 or 0x24 or 0x5C or 0x70 or 0x2A or 0x44:
                Configura(cmd);
                Avisar($"comando ignorado {Hex(cmd[..2])} en el byte {Posicion(largo)}");
                break;
            default:
                Configura(cmd);
                Avisar($"comando desconocido {Hex(cmd)} en el byte {Posicion(largo)}");
                break;
        }
        return largo;
    }

    private int Gs(ReadOnlySpan<byte> b)
    {
        byte f = b[1];
        int largo;
        switch (f)
        {
            case 0x56:
                if (b.Length < 3) return 0;
                largo = b[2] is 65 or 66 or 97 or 98 or 103 or 104 ? 4 : 3;
                break;
            case 0x76:
                if (b.Length < 8) return 0;
                if (b[2] != 0x30)
                {
                    largo = 3;
                    break;
                }
                if (!Medida(b, 8 + (long)(b[4] | b[5] << 8) * (b[6] | b[7] << 8), out largo)) return largo;
                break;
            case 0x28:
                if (b.Length < 5) return 0;
                largo = 5 + (b[3] | b[4] << 8);
                break;
            case 0x38:
                if (b.Length < 7) return 0;
                if (!Medida(b, 7 + (long)(uint)(b[3] | b[4] << 8 | b[5] << 16 | b[6] << 24), out largo)) return largo;
                break;
            case 0x6B:
                if (b.Length < 3) return 0;
                largo = b[2] <= 6 ? HastaNul(b, 3, 256) : b.Length < 4 ? 0 : 4 + b[3];
                if (largo == 0) return 0;
                break;
            case 0x2A:
                if (b.Length < 4) return 0;
                largo = 4 + b[2] * b[3] * 8;
                break;
            case 0x68 or 0x77 or 0x48 or 0x66 or 0x21 or 0x42 or 0x49 or 0x61 or 0x72 or 0x2F or 0x62:
                largo = 3;
                break;
            case 0x4C or 0x57 or 0x50 or 0x5C or 0x24:
                largo = 4;
                break;
            default:
                largo = 2;
                break;
        }
        if (b.Length < largo) return 0;
        var cmd = b[..largo];
        switch (f)
        {
            case 0x56: Corte(cmd); break;
            case 0x76:
                Imprime(cmd);
                if (cmd[2] != 0x30)
                {
                    Avisar($"comando desconocido {Hex(cmd)} en el byte {Posicion(largo)}");
                    break;
                }
                Raster(cmd[3], cmd[4] | cmd[5] << 8, cmd[6] | cmd[7] << 8, cmd[8..]);
                break;
            case 0x28: Parentesis(cmd); break;
            case 0x6B: Barras(cmd); break;
            case 0x68:
                Configura(cmd);
                _altoBarras = Math.Max(1, (int)cmd[2]);
                break;
            case 0x77:
                Configura(cmd);
                _anchoModulo = Math.Clamp((int)cmd[2], 1, 6);
                break;
            case 0x48:
                Configura(cmd);
                _hri = cmd[2] % 48;
                break;
            case 0x21:
                Configura(cmd);
                _escalaX = Math.Clamp((cmd[2] >> 4 & 7) + 1, 1, 8);
                _escalaY = Math.Clamp((cmd[2] & 7) + 1, 1, 8);
                break;
            case 0x49:
                Consulta(cmd);
                if (cmd[2] == 67) Responder(_perfil.RespuestaModelo);
                else Avisar($"GS I {cmd[2]}: consulta sin respuesta configurada");
                break;
            case 0x61 or 0x72:
                Consulta(cmd);
                Avisar($"comando ignorado {Hex(cmd[..2])}: estado automático o consulta sin respuesta configurada");
                break;
            case 0x38 or 0x2A or 0x66 or 0x42 or 0x2F or 0x4C or 0x57 or 0x50 or 0x62 or 0x5C or 0x24:
                Configura(cmd);
                Avisar($"comando ignorado {Hex(cmd[..2])} en el byte {Posicion(largo)}");
                break;
            default:
                Configura(cmd);
                Avisar($"comando desconocido {Hex(cmd)} en el byte {Posicion(largo)}");
                break;
        }
        return largo;
    }

    private int Fs(ReadOnlySpan<byte> b)
    {
        byte f = b[1];
        int largo = f switch
        {
            0x70 => 4,
            0x43 or 0x21 or 0x2D or 0x57 => 3,
            0x2E or 0x26 => 2,
            0x71 => LargoFsq(b),
            _ => 2,
        };
        if (largo < 0)
        {
            Medida(b, CargaMaxima + 1, out largo);
            return largo;
        }
        if (largo == 0 || b.Length < largo) return 0;
        var cmd = b[..largo];
        if (f == 0x71)
        {
            Consulta(cmd);
            Avisar($"FS q: definición de imagen NV ignorada ({largo} bytes)");
        }
        else if (f is 0x70 or 0x43 or 0x21 or 0x2D or 0x57 or 0x2E or 0x26)
        {
            Configura(cmd);
            Avisar($"comando ignorado {Hex(cmd[..2])} en el byte {Posicion(largo)}");
        }
        else
        {
            Configura(cmd);
            Avisar($"comando desconocido {Hex(cmd)} en el byte {Posicion(largo)}");
        }
        return largo;
    }

    private int Dle(ReadOnlySpan<byte> b)
    {
        byte f = b[1];
        int largo = f switch
        {
            0x04 or 0x05 => 3,
            0x14 => b.Length < 3 ? 0 : b[2] switch { 1 or 2 => 5, 7 => 4, 8 => 10, _ => 3 },
            _ => 2,
        };
        if (largo == 0 || b.Length < largo) return 0;
        var cmd = b[..largo];
        Consulta(cmd);
        if (f == 0x04)
        {
            var estado = _perfil.Estado;
            byte? r = cmd[2] switch
            {
                1 => estado == EstadoDelPapel.Normal ? (byte)0x12 : (byte)0x1A,
                2 => (byte)(estado switch { EstadoDelPapel.SinPapel => 0x32, EstadoDelPapel.TapaAbierta => 0x16, _ => 0x12 }),
                3 => 0x12,
                4 => estado == EstadoDelPapel.SinPapel ? (byte)0x72 : (byte)0x12,
                _ => null,
            };
            if (r is { } v) Responder([v]);
            else Avisar($"DLE EOT {cmd[2]}: consulta desconocida");
        }
        else
        {
            Avisar($"comando {(f is 0x05 or 0x14 ? "ignorado" : "desconocido")} {Hex(cmd)}");
        }
        return largo;
    }

    private static int HastaNul(ReadOnlySpan<byte> b, int desde, int tope)
    {
        // El NUL se busca sólo dentro del tope: así el largo no depende de cuántos bytes hay ya en el búfer.
        int i = b[desde..Math.Min(b.Length, desde + tope)].IndexOf((byte)0);
        if (i >= 0) return desde + i + 1;
        return b.Length - desde >= tope ? desde + tope : 0;
    }

    /// <summary>Largo de FS q; -1 si la carga supera <see cref="CargaMaxima"/>.</summary>
    private static int LargoFsq(ReadOnlySpan<byte> b)
    {
        if (b.Length < 3) return 0;
        long p = 3;
        for (int i = 0; i < b[2]; i++)
        {
            if (b.Length < p + 4) return 0;
            int q = (int)p;
            p += 4 + (long)(b[q] | b[q + 1] << 8) * (b[q + 2] | b[q + 3] << 8) * 8;
            if (p > CargaMaxima) return -1;
        }
        return (int)p;
    }

    /// <summary>
    /// Valida el largo de un comando con carga. Si supera <see cref="CargaMaxima"/> (bytes corruptos o un comando que no es
    /// lo que parece), no espera esa carga: registra el comando, avisa y consume sólo sus dos primeros bytes.
    /// </summary>
    private bool Medida(ReadOnlySpan<byte> b, long largo, out int resultado)
    {
        if (largo <= CargaMaxima)
        {
            resultado = (int)largo;
            return true;
        }
        Configura(b[..2]);
        Avisar($"comando {Hex(b[..2])} con carga de {largo} bytes en el byte {Posicion(2)}: se descarta por desmesurada");
        resultado = 2;
        return false;
    }

    // ───────────────────────── ticket ─────────────────────────

    private void AbrirTicket()
    {
        _bytes = new MemoryStream();
        _avisos = [];
        _lienzo = new Lienzo(_perfil.AnchoPuntos, _perfil.AltoMaximo, _perfil.AltoMaximo - _perfil.Margen);
        _y = _perfil.Margen;
        _lienzo.Asegurar(_y);
        _contenido = false;
        _altoAgotado = false;
        _avisosOmitidos = 0;
    }

    private void CerrarTicket()
    {
        ImprimirLinea(avanzarSiVacia: false);
        _lienzo.Asegurar(Math.Max(_y, _lienzo.Alto) + _perfil.Margen);
        if (_avisosOmitidos > 0) _avisos.Add($"{_avisosOmitidos} avisos más no se guardaron en el ticket");
        var ticket = new TicketInterpretado(_bytes!.ToArray(), _lienzo.APng(), _lienzo.Ancho, _lienzo.Alto, _avisos);
        _bytes = null;
        Disparar(TicketTerminado, ticket, nameof(TicketTerminado));
    }

    /// <summary>Bytes de un comando que imprime o avanza papel: abre el ticket si hace falta.</summary>
    private void Imprime(ReadOnlySpan<byte> cmd)
    {
        if (_bytes is null) AbrirTicket();
        _bytes!.Write(cmd);
    }

    /// <summary>Bytes de un comando que cambia el estado: forma parte del ticket, abriéndolo si hace falta.</summary>
    private void Configura(ReadOnlySpan<byte> cmd) => Imprime(cmd);

    /// <summary>Consulta o configuración NV: no abre ticket; si hay uno abierto, sus bytes quedan en él.</summary>
    private void Consulta(ReadOnlySpan<byte> cmd) => _bytes?.Write(cmd);

    private long Posicion(int largoDelComando) => (_bytes?.Length ?? largoDelComando) - largoDelComando;

    private void Avisar(string mensaje)
    {
        if (_bytes is not null)
        {
            if (_avisos.Count < AvisosMaximos) _avisos.Add(mensaje);
            else _avisosOmitidos++;
        }
        foreach (var d in Aviso?.GetInvocationList() ?? Array.Empty<Delegate>())
        {
            try
            {
                ((Action<string>)d)(mensaje);
            }
            catch (Exception)
            {
                // Un suscriptor de avisos que falla no puede avisar de sí mismo.
            }
        }
    }

    private void Responder(byte[] bytes) => Disparar(Respuesta, bytes.ToArray(), nameof(Respuesta));

    /// <summary>
    /// Avisa a cada suscriptor por separado. Si uno lanza (por ejemplo, el socket de PAR-PTE-03 se cayó al responder),
    /// se registra como aviso y el comando ya interpretado no se repite en el próximo trozo.
    /// </summary>
    private void Disparar<T>(Action<T>? evento, T valor, string nombre)
    {
        foreach (var d in evento?.GetInvocationList() ?? Array.Empty<Delegate>())
        {
            try
            {
                ((Action<T>)d)(valor);
            }
            catch (Exception e)
            {
                Avisar($"un suscriptor de {nombre} falló: {e.GetType().Name}: {e.Message}");
            }
        }
    }

    private void Reiniciar()
    {
        _linea.Clear();
        _anchoLinea = 0;
        _alineacion = Alineacion.Izquierda;
        _negrita = false;
        _subrayado = 0;
        _escalaX = _escalaY = 1;
        _interlineado = _perfil.Interlineado;
        _tabla = TablasDeCaracteres.Pc437;
        _moduloQr = 3;
        _correccionQr = CodigoQr.Correccion.L;
        _datosQr = [];
        _altoBarras = _perfil.AltoDeBarras;
        _anchoModulo = _perfil.AnchoDeModulo;
        _hri = 0;
    }

    // ───────────────────────── dibujo ─────────────────────────

    private int Origen(int ancho) => _alineacion switch
    {
        Alineacion.Centro => Math.Max(0, (_lienzo.Ancho - ancho) / 2),
        Alineacion.Derecha => Math.Max(0, _lienzo.Ancho - ancho),
        _ => 0,
    };

    private void Avanzar(int puntos)
    {
        _contenido = true;
        int tope = _perfil.AltoMaximo - _perfil.Margen;
        if (_y + puntos > tope)
        {
            if (!_altoAgotado) Avisar($"el ticket llegó al alto máximo de {_perfil.AltoMaximo} puntos: lo que sigue no se dibuja");
            _altoAgotado = true;
            puntos = Math.Max(0, tope - _y);
        }
        _y += puntos;
        _lienzo.Asegurar(_y);
    }

    private void AgregarCaracter(byte b)
    {
        Span<char> ch = stackalloc char[2];
        int n = _tabla.GetChars([b], ch);
        int codigo = n > 0 ? ch[0] : '?';
        if (!Fuente.Normal.Tiene(codigo))
        {
            Avisar($"carácter sin glifo U+{codigo:X4} (byte {b:X2}): se dibuja «?»");
            codigo = '?';
        }
        int ancho = Fuente.Ancho * _escalaX;
        if (_anchoLinea + ancho > _lienzo.Ancho) ImprimirLinea(avanzarSiVacia: false);
        _linea.Add(new Celda(codigo, _negrita, _escalaX, _escalaY, _subrayado));
        _anchoLinea += ancho;
        _contenido = true;
    }

    private void Tabulador()
    {
        int columna = _anchoLinea / Fuente.Ancho;
        int hasta = (columna / 8 + 1) * 8;
        for (int i = columna; i < hasta && _anchoLinea + Fuente.Ancho <= _lienzo.Ancho; i++)
        {
            _linea.Add(new Celda(' ', false, 1, 1, _subrayado));
            _anchoLinea += Fuente.Ancho;
        }
        _contenido = true;
    }

    private void ImprimirLinea(bool avanzarSiVacia)
    {
        if (_linea.Count == 0)
        {
            if (avanzarSiVacia) Avanzar(_interlineado);
            return;
        }
        int alto = _linea.Max(c => Fuente.Alto * c.EscalaY);
        int x = Origen(_anchoLinea);
        foreach (var c in _linea)
        {
            var fuente = c.Negrita ? Fuente.Negrita : Fuente.Normal;
            int arriba = _y + alto - Fuente.Alto * c.EscalaY;
            for (int gy = 0; gy < Fuente.Alto; gy++)
                for (int gx = 0; gx < Fuente.Ancho; gx++)
                    if (fuente.Punto(c.Codigo, gx, gy))
                        for (int sy = 0; sy < c.EscalaY; sy++)
                            for (int sx = 0; sx < c.EscalaX; sx++)
                                _lienzo.Marcar(x + gx * c.EscalaX + sx, arriba + gy * c.EscalaY + sy);
            for (int s = 0; s < c.Subrayado; s++)
                for (int ux = 0; ux < Fuente.Ancho * c.EscalaX; ux++)
                    _lienzo.Marcar(x + ux, _y + alto - 1 - s);
            x += Fuente.Ancho * c.EscalaX;
        }
        _linea.Clear();
        _anchoLinea = 0;
        Avanzar(Math.Max(_interlineado, alto));
    }

    private void Raster(byte modo, int bytesPorFila, int filas, ReadOnlySpan<byte> datos)
    {
        ImprimirLinea(avanzarSiVacia: false);
        int ex = (modo % 48) is 1 or 3 ? 2 : 1, ey = (modo % 48) is 2 or 3 ? 2 : 1;
        int ancho = bytesPorFila * 8 * ex;
        if (ancho > _lienzo.Ancho)
            Avisar($"imagen GS v 0 de {ancho} puntos: se recorta a {_lienzo.Ancho}");
        int x0 = Origen(Math.Min(ancho, _lienzo.Ancho));
        _lienzo.Asegurar(_y + filas * ey);
        for (int f = 0; f < filas; f++)
            for (int c = 0; c < bytesPorFila * 8; c++)
                if (((datos[f * bytesPorFila + (c >> 3)] >> (7 - (c & 7))) & 1) == 1)
                    for (int sy = 0; sy < ey; sy++)
                        for (int sx = 0; sx < ex; sx++)
                            _lienzo.Marcar(x0 + c * ex + sx, _y + f * ey + sy);
        Avanzar(filas * ey);
    }

    private void Parentesis(ReadOnlySpan<byte> cmd)
    {
        byte letra = cmd[2];
        var p = cmd[5..];
        if (letra == 0x6B && p.Length >= 2 && p[0] == 0x31)
        {
            Qr(cmd, p[1], p[2..]);
            return;
        }
        if (letra == 0x4C)
        {
            Consulta(cmd);
            if (p.Length >= 2 && p[0] == 0x30 && p[1] == 0x30 && _perfil.RespuestaNv is { } nv) Responder(nv);
            else if (!(p.Length >= 2 && p[0] == 0x30 && p[1] == 0x30))
                Avisar($"GS ( L fn {(p.Length >= 2 ? p[1] : 0)}: gráficos NV ignorados");
            return;
        }
        Configura(cmd);
        Avisar($"comando ignorado GS ( {(char)letra} en el byte {Posicion(cmd.Length)}");
    }

    private void Qr(ReadOnlySpan<byte> cmd, byte fn, ReadOnlySpan<byte> resto)
    {
        Configura(cmd);
        switch (fn)
        {
            case 0x41:
                break; // modelo: sólo el 2
            case 0x43:
                if (resto.Length >= 1) _moduloQr = Math.Clamp((int)resto[0], 1, 16);
                break;
            case 0x45:
                if (resto.Length >= 1 && resto[0] is >= 48 and <= 51) _correccionQr = (CodigoQr.Correccion)(resto[0] - 48);
                break;
            case 0x50:
                _datosQr = resto.Length >= 1 ? resto[1..].ToArray() : [];
                break;
            case 0x51:
                ImprimirQr();
                break;
            default:
                Avisar($"GS ( k fn {fn}: función de QR ignorada");
                break;
        }
    }

    private void ImprimirQr()
    {
        ImprimirLinea(avanzarSiVacia: false);
        if (_datosQr.Length == 0)
        {
            Avisar("GS ( k fn 81 sin datos guardados: no se imprime el QR");
            return;
        }
        bool[,] m;
        try
        {
            m = CodigoQr.Generar(_datosQr, _correccionQr);
        }
        catch (ArgumentException e)
        {
            Avisar("QR no generado: " + e.Message);
            return;
        }
        int n = m.GetLength(0), ancho = n * _moduloQr;
        if (ancho > _lienzo.Ancho) Avisar($"QR de {ancho} puntos: se recorta a {_lienzo.Ancho}");
        int x0 = Origen(Math.Min(ancho, _lienzo.Ancho));
        _lienzo.Asegurar(_y + ancho);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
                if (m[y, x])
                    for (int sy = 0; sy < _moduloQr; sy++)
                        for (int sx = 0; sx < _moduloQr; sx++)
                            _lienzo.Marcar(x0 + x * _moduloQr + sx, _y + y * _moduloQr + sy);
        Avanzar(ancho);
    }

    private void Barras(ReadOnlySpan<byte> cmd)
    {
        Imprime(cmd);
        int m = cmd[2];
        var datos = m <= 6 ? cmd[3..^1] : cmd[4..];
        if (m is not (2 or 67))
        {
            Avisar($"GS k {m}: sistema de código de barras no soportado");
            return;
        }
        var trece = CodigoEan13.Completar(Encoding.ASCII.GetString(datos));
        if (trece is null)
        {
            Avisar($"GS k {m}: datos EAN-13 inválidos «{Encoding.ASCII.GetString(datos)}»");
            return;
        }
        if (_hri != 0) Avisar("GS H: el texto legible (HRI) no se dibuja");
        ImprimirLinea(avanzarSiVacia: false);
        var modulos = CodigoEan13.Modulos(trece);
        int ancho = modulos.Length * _anchoModulo;
        if (ancho > _lienzo.Ancho) Avisar($"código de barras de {ancho} puntos: se recorta a {_lienzo.Ancho}");
        int x0 = Origen(Math.Min(ancho, _lienzo.Ancho));
        _lienzo.Asegurar(_y + _altoBarras);
        for (int i = 0; i < modulos.Length; i++)
            if (modulos[i])
                for (int sx = 0; sx < _anchoModulo; sx++)
                    for (int y = 0; y < _altoBarras; y++)
                        _lienzo.Marcar(x0 + i * _anchoModulo + sx, _y + y);
        Avanzar(_altoBarras);
    }

    private void Corte(ReadOnlySpan<byte> cmd)
    {
        if (_bytes is null)
        {
            Avisar($"corte {Hex(cmd)} sin ticket abierto: se ignora");
            return;
        }
        _bytes.Write(cmd);
        if (cmd[2] is not (0 or 1 or 48 or 49 or 65 or 66 or 97 or 98 or 103 or 104))
            Avisar($"GS V {cmd[2]}: modo de corte desconocido, se toma como corte");
        ImprimirLinea(avanzarSiVacia: false);
        if (cmd.Length == 4) Avanzar(cmd[3]);
        CerrarTicket();
    }

    private static string Hex(ReadOnlySpan<byte> b) => Convert.ToHexString(b).Chunk(2).Select(c => new string(c)).Aggregate((a, x) => a + " " + x);
}
