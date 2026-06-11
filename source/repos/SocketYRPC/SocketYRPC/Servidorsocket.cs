// ================================================================
//  ServidorSocket.cs
//  Servidor TCP con Sockets — Comunicación Distribuida
//  Programación Paralela y Distribuida — UNIBE
//  Semana 5: Comunicación con Sockets y RPC
//
//  Ejecutar PRIMERO este archivo, luego ClienteSocket.cs
//  Comando: dotnet run --project ServidorSocket
// ================================================================

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ServidorSocket
{
    // ── Configuración del servidor ────────────────────────────
    public const string DIRECCION_IP = "127.0.0.1"; // localhost
    public const int PUERTO_ESCUCHA = 9000;         // puerto de escucha
    public const int TAMANO_BUFFER = 1024;         // bytes del buffer
    public const int MAX_CONEXIONES = 5;            // cola máxima de clientes

    // ── Método para atender a un cliente en su propio hilo ───
    public static void AtenderCliente(Socket socketCliente, int numeroConexion)
    {
        string direccionCliente = socketCliente.RemoteEndPoint?.ToString() ?? "desconocido";
        Console.WriteLine($"  [Servidor] Conexión #{numeroConexion} aceptada desde {direccionCliente}");

        byte[] bufferRecepcion = new byte[TAMANO_BUFFER];

        try
        {
            // ── 5. Recibir el mensaje del cliente ─────────────
            int bytesRecibidos = socketCliente.Receive(bufferRecepcion);
            string mensajeRecibido = Encoding.UTF8.GetString(
                bufferRecepcion, 0, bytesRecibidos);

            Console.WriteLine($"  [Servidor] Mensaje recibido de #{numeroConexion}: \"{mensajeRecibido}\"");

            // ── 6. Preparar y enviar la respuesta ─────────────
            string mensajeRespuesta =
                $"[SERVIDOR UNIBE] Mensaje recibido correctamente. " +
                $"Contenido: '{mensajeRecibido}' | " +
                $"Conexión: #{numeroConexion} | " +
                $"Hora: {DateTime.Now:HH:mm:ss}";

            byte[] bufferRespuesta = Encoding.UTF8.GetBytes(mensajeRespuesta);
            socketCliente.Send(bufferRespuesta);

            Console.WriteLine($"  [Servidor] Respuesta enviada a cliente #{numeroConexion}");
        }
        catch (SocketException errorSocket)
        {
            Console.WriteLine($"  [Servidor] Error con cliente #{numeroConexion}: {errorSocket.Message}");
        }
        finally
        {
            // ── 7. Cerrar la conexión con este cliente ────────
            socketCliente.Shutdown(SocketShutdown.Both);
            socketCliente.Close();
            Console.WriteLine($"  [Servidor] Conexión #{numeroConexion} cerrada.\n");
        }
    }
}