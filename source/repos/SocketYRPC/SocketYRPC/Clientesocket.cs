// ================================================================
//  ClienteSocket.cs
//  Cliente TCP con Sockets — Comunicación Distribuida
//  Programación Paralela y Distribuida — UNIBE
//  Semana 5: Comunicación con Sockets y RPC
//
//  Ejecutar DESPUÉS de ServidorSocket.cs
//  Comando: dotnet run --project ClienteSocket
// ================================================================

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ClienteSocket
{
    // ── Configuración de conexión ─────────────────────────────
    public const string DIRECCION_SERVIDOR = "127.0.0.1"; // IP del servidor
    public const int PUERTO_SERVIDOR = 9000;          // puerto del servidor
    public const int TAMANO_BUFFER = 1024;           // bytes del buffer

    public static void EnviarMensaje(string mensajeAEnviar, int numeroEnvio)
    {
        // ── 1. Crear el socket del cliente ────────────────────
        Socket socketCliente = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        try
        {
            // ── 2. Conectar al servidor ───────────────────────
            IPEndPoint puntoFinalServidor = new IPEndPoint(
                IPAddress.Parse(DIRECCION_SERVIDOR),
                PUERTO_SERVIDOR
            );

            Console.WriteLine($"  [Cliente] Conectando a {DIRECCION_SERVIDOR}:{PUERTO_SERVIDOR}...");
            socketCliente.Connect(puntoFinalServidor);
            Console.WriteLine($"  [Cliente] Conexión establecida exitosamente.");

            // ── 3. Enviar el mensaje al servidor ──────────────
            string mensajeFormateado = $"[Envío #{numeroEnvio}] {mensajeAEnviar}";
            byte[] bufferEnvio = Encoding.UTF8.GetBytes(mensajeFormateado);

            int bytesEnviados = socketCliente.Send(bufferEnvio);
            Console.WriteLine($"  [Cliente] Mensaje enviado ({bytesEnviados} bytes): \"{mensajeFormateado}\"");

            // ── 4. Esperar y recibir la confirmación ──────────
            byte[] bufferRespuesta = new byte[TAMANO_BUFFER];
            int bytesRecibidos = socketCliente.Receive(bufferRespuesta);
            string respuestaServidor = Encoding.UTF8.GetString(
                bufferRespuesta, 0, bytesRecibidos);

            Console.WriteLine($"\n  [Cliente] Confirmación del servidor:");
            Console.WriteLine($"  ┌─────────────────────────────────────────────");
            Console.WriteLine($"  │ {respuestaServidor}");
            Console.WriteLine($"  └─────────────────────────────────────────────");
        }
        catch (SocketException errorSocket)
        {
            Console.WriteLine($"  [Cliente] Error de conexión: {errorSocket.Message}");
            Console.WriteLine("  [Cliente] ¿Está el servidor en ejecución?");
        }
        catch (Exception error)
        {
            Console.WriteLine($"  [Cliente] Error inesperado: {error.Message}");
        }
        finally
        {
            // ── 5. Cerrar el socket del cliente ───────────────
            if (socketCliente.Connected)
                socketCliente.Shutdown(SocketShutdown.Both);
            socketCliente.Close();
        }
    }
}