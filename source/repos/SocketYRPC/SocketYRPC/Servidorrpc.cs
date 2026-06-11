// ================================================================
//  ServidorRPC.cs
//  Servidor RPC (Remote Procedure Call) — Comunicación Distribuida
//  Implementado sobre HTTP con JsonSerializer para simular RPC
//  de forma estándar y ejecutable sin bibliotecas externas.
//
//  El servidor expone una función remota "calcularCuadrado"
//  que recibe un número entero y devuelve su cuadrado.
//
//  Programación Paralela y Distribuida — UNIBE
//  Semana 5: Comunicación con Sockets y RPC
//
//  Ejecutar PRIMERO este archivo, luego ClienteRPC.cs
//  Comando: dotnet run --project ServidorRPC
// ================================================================

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;

// ── Modelos de datos RPC ──────────────────────────────────────
// Representan la solicitud y respuesta que viajan por la red
record SolicitudRPC(string NombreFuncion, int ParametroNumero);
record RespuestaRPC(string NombreFuncion, int ParametroNumero,
                    long Resultado, bool Exitoso,
                    string Mensaje, string MarcaTiempo);

public class ServidorRPC
{
    public const string URL_BASE = "http://localhost:8080/";
    public const string RUTA_RPC = "rpc/";
    public const string TIPO_CONTENIDO = "application/json";

    // ── Registro de llamadas remotas recibidas ────────────────
    public static int totalLlamadasAtendidas = 0;

    // ── Procesa una solicitud RPC entrante ────────────────────
    public static void ProcesarSolicitudRPC(HttpListenerContext contextoHttp)
    {
        HttpListenerRequest solicitudHttp = contextoHttp.Request;
        HttpListenerResponse respuestaHttp = contextoHttp.Response;

        int numeroLlamada = Interlocked.Increment(ref totalLlamadasAtendidas);
        string ipCliente = solicitudHttp.RemoteEndPoint?.ToString() ?? "desconocido";

        Console.WriteLine($"  [Servidor RPC] Llamada #{numeroLlamada} recibida desde {ipCliente}");

        try
        {
            // ── Leer el cuerpo JSON de la solicitud ───────────
            string cuerpoJson;
            using (StreamReader lector = new StreamReader(
                solicitudHttp.InputStream, solicitudHttp.ContentEncoding))
            {
                cuerpoJson = lector.ReadToEnd();
            }

            Console.WriteLine($"  [Servidor RPC] JSON recibido: {cuerpoJson}");

            // ── Deserializar la solicitud RPC ─────────────────
            SolicitudRPC? solicitudRPC = JsonSerializer.Deserialize<SolicitudRPC>(
                cuerpoJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (solicitudRPC == null)
                throw new InvalidOperationException("Solicitud RPC mal formada.");

            // ── Despachar la función remota solicitada ─────────
            RespuestaRPC respuestaRPC = DespacharFuncion(solicitudRPC, numeroLlamada);

            // ── Serializar y enviar la respuesta ──────────────
            string jsonRespuesta = JsonSerializer.Serialize(respuestaRPC,
                new JsonSerializerOptions { WriteIndented = true });

            byte[] bufferRespuesta = Encoding.UTF8.GetBytes(jsonRespuesta);
            respuestaHttp.ContentType = TIPO_CONTENIDO;
            respuestaHttp.ContentLength64 = bufferRespuesta.Length;
            respuestaHttp.StatusCode = (int)HttpStatusCode.OK;

            respuestaHttp.OutputStream.Write(bufferRespuesta, 0, bufferRespuesta.Length);

            Console.WriteLine($"  [Servidor RPC] Resultado enviado: " +
                              $"{solicitudRPC.ParametroNumero}² = {respuestaRPC.Resultado}\n");
        }
        catch (Exception error)
        {
            Console.WriteLine($"  [Servidor RPC] Error procesando llamada #{numeroLlamada}: {error.Message}");
            respuestaHttp.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        finally
        {
            respuestaHttp.OutputStream.Close();
        }
    }

    // ── Registro de funciones remotas disponibles ─────────────
    static RespuestaRPC DespacharFuncion(SolicitudRPC solicitud, int numeroLlamada)
    {
        return solicitud.NombreFuncion.ToLower() switch
        {
            "calcularcuadrado" => EjecutarCalcularCuadrado(solicitud),
            _ => new RespuestaRPC(
                solicitud.NombreFuncion, solicitud.ParametroNumero,
                0, false,
                $"Función '{solicitud.NombreFuncion}' no registrada en este servidor.",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
        };
    }

    // ── FUNCIÓN REMOTA: calcularCuadrado ──────────────────────
    // Recibe un número entero y devuelve su cuadrado
    // Esta es la función que el cliente invoca de forma remota
    static RespuestaRPC EjecutarCalcularCuadrado(SolicitudRPC solicitud)
    {
        int numeroEntrada = solicitud.ParametroNumero;
        long resultadoCuadrado = (long)numeroEntrada * numeroEntrada;

        Console.WriteLine($"  [Servidor RPC] Ejecutando calcularCuadrado({numeroEntrada})");
        Console.WriteLine($"  [Servidor RPC] Resultado: {numeroEntrada}² = {resultadoCuadrado}");

        return new RespuestaRPC(
            NombreFuncion: "calcularCuadrado",
            ParametroNumero: numeroEntrada,
            Resultado: resultadoCuadrado,
            Exitoso: true,
            Mensaje: $"Cuadrado de {numeroEntrada} calculado exitosamente.",
            MarcaTiempo: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        );
    }
}