//ServidorSocket.cs
using System.Net;
using System.Net.Sockets;

Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║  Servidor TCP con Sockets — UNIBE                   ║");
Console.WriteLine("║  Programación Paralela y Distribuida                ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");

// ── 1. Crear el socket del servidor ───────────────────
// AddressFamily.InterNetwork = IPv4
// SocketType.Stream          = TCP orientado a conexión
// ProtocolType.Tcp           = protocolo TCP
Socket socketServidor = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp
);

try
{
    // ── 2. Vincular el socket a la dirección y puerto ─
    IPEndPoint puntoFinal = new IPEndPoint( IPAddress.Parse(ServidorSocket.DIRECCION_IP), ServidorSocket.PUERTO_ESCUCHA);
    socketServidor.Bind(puntoFinal);

    // ── 3. Poner el socket en modo escucha ────────────
    socketServidor.Listen(ServidorSocket.MAX_CONEXIONES);
    Console.WriteLine($"\n  [Servidor] Escuchando en {ServidorSocket.DIRECCION_IP}:{ServidorSocket.PUERTO_ESCUCHA}");
    Console.WriteLine("  [Servidor] Esperando conexiones... (Ctrl+C para detener)\n");

    // ── 4. Bucle principal: acepta múltiples clientes ─
    int numeroConexion = 0;
    while (true)
    {
        // Accept() bloquea hasta que llega una conexión
        Socket socketCliente = socketServidor.Accept();
        numeroConexion++;

        // Lanzar un hilo por cliente para atender concurrentemente
        int capturedNum = numeroConexion;
        Thread hiloCliente = new Thread(() => ServidorSocket.AtenderCliente(socketCliente, capturedNum));
        hiloCliente.IsBackground = true;
        hiloCliente.Start();
    }
}
catch (SocketException errorSocket)
{
    Console.WriteLine($"  [Servidor] Error de socket: {errorSocket.Message}");
}
catch (Exception error)
{
    Console.WriteLine($"  [Servidor] Error inesperado: {error.Message}");
}
finally
{
    socketServidor.Close();
    Console.WriteLine("  [Servidor] Socket cerrado.");
}

//ClienteRPC.cs
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║  Cliente RPC sobre HTTP/JSON — UNIBE                ║");
Console.WriteLine("║  Programación Paralela y Distribuida                ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine($"\n  Conectando al servidor RPC en: {ClienteRPC.URL_SERVIDOR}");
Console.WriteLine($"  Función remota a invocar: {ClienteRPC.NOMBRE_FUNCION}\n");

bool continuarSesion = true;

while (continuarSesion)
{
    Console.Write("  Ingrese un número entero (o 'salir' para terminar): ");
    string entradaUsuario = Console.ReadLine() ?? string.Empty;

    if (entradaUsuario.ToLower() == "salir")
    {
        continuarSesion = false;
        break;
    }

    // ── Validar que la entrada sea un número ──────────
    if (!int.TryParse(entradaUsuario, out int numeroIngresado))
    {
        Console.WriteLine("  [Error] Por favor ingrese un número entero válido.\n");
        continue;
    }

    // ── Invocar la función remota ──────────────────────
    await ClienteRPC.InvocarFuncionRemota(numeroIngresado);
}

Console.WriteLine("\n  [Cliente RPC] Sesión finalizada.");




//ServidorRPC.cs
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║  Servidor RPC sobre HTTP/JSON — UNIBE               ║");
Console.WriteLine("║  Programación Paralela y Distribuida                ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");

// ── HttpListener: servidor HTTP integrado en .NET ──────
HttpListener oyenteHttp = new HttpListener();
oyenteHttp.Prefixes.Add(ServidorRPC.URL_BASE);

try
{
    oyenteHttp.Start();
    Console.WriteLine($"\n  [Servidor RPC] Escuchando en {ServidorRPC.URL_BASE}");
    Console.WriteLine($"  [Servidor RPC] Función disponible: calcularCuadrado");
    Console.WriteLine("  [Servidor RPC] Esperando llamadas remotas... (Ctrl+C para detener)\n");

    // Bucle principal de atención de solicitudes
    while (true)
    {
        // GetContext() bloquea hasta recibir una solicitud HTTP
        HttpListenerContext contextoHttp = oyenteHttp.GetContext();

        // Atender cada solicitud en un hilo separado
        Thread hiloSolicitud = new Thread(() =>
            ServidorRPC.ProcesarSolicitudRPC(contextoHttp));
        hiloSolicitud.IsBackground = true;
        hiloSolicitud.Start();
    }
}
catch (HttpListenerException errorHttp)
{
    Console.WriteLine($"  [Servidor RPC] Error HTTP: {errorHttp.Message}");
    Console.WriteLine("  Tip: Ejecutar como administrador si el puerto 8080 está bloqueado.");
}
catch (Exception error)
{
    Console.WriteLine($"  [Servidor RPC] Error: {error.Message}");
}
finally
{
    oyenteHttp.Stop();
    Console.WriteLine($"\n  [Servidor RPC] Detenido. Llamadas atendidas: {ServidorRPC.totalLlamadasAtendidas}");
}

//ClienteSocket.cs
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║  Cliente TCP con Sockets — UNIBE                    ║");
Console.WriteLine("║  Programación Paralela y Distribuida                ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");

// Permitir enviar múltiples mensajes en la misma sesión
bool continuarEnviando = true;
int numeroEnvio = 0;

while (continuarEnviando)
{
    numeroEnvio++;
    Console.Write($"\n  Ingrese mensaje a enviar (o 'salir' para terminar): ");
    string mensajeUsuario = Console.ReadLine() ?? string.Empty;

    if (mensajeUsuario.ToLower() == "salir")
    {
        continuarEnviando = false;
        break;
    }

    ClienteSocket.EnviarMensaje(mensajeUsuario, numeroEnvio);
}

Console.WriteLine("\n  [Cliente] Sesión finalizada.");