// ================================================================
//  ClienteRPC.cs
//  Cliente RPC (Remote Procedure Call) — Comunicación Distribuida
//  Llama de forma remota a la función "calcularCuadrado"
//  expuesta por ServidorRPC.cs sobre HTTP/JSON.
//
//  Programación Paralela y Distribuida — UNIBE
//  Semana 5: Comunicación con Sockets y RPC
//
//  Ejecutar DESPUÉS de ServidorRPC.cs
//  Comando: dotnet run --project ClienteRPC
// ================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// ── Modelos de datos (deben coincidir con el servidor) ────────
//record SolicitudRPC(string NombreFuncion, int ParametroNumero);
//record RespuestaRPC(string NombreFuncion, int ParametroNumero,
//                    long Resultado, bool Exitoso,
//                    string Mensaje, string MarcaTiempo);

public class ClienteRPC
{
    public const string URL_SERVIDOR = "http://localhost:8080/rpc/";
    public const string TIPO_CONTENIDO = "application/json";
    public const string NOMBRE_FUNCION = "calcularCuadrado";

    // HttpClient es reutilizable — no crear uno por solicitud
    public static readonly HttpClient clienteHttp = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    // ── Método principal de invocación RPC ───────────────────
    // Simula exactamente lo que haría un stub RPC:
    // 1. Serializar los parámetros
    // 2. Enviar al servidor
    // 3. Recibir y deserializar la respuesta
    // 4. Devolver el resultado como si fuera local
    public static async Task InvocarFuncionRemota(int numeroEntrada)
    {
        Console.WriteLine($"\n  [Cliente RPC] Invocando {NOMBRE_FUNCION}({numeroEntrada}) en el servidor...");

        try
        {
            // ── 1. Construir la solicitud RPC ─────────────────
            SolicitudRPC solicitud = new SolicitudRPC(NOMBRE_FUNCION, numeroEntrada);
            string jsonSolicitud = JsonSerializer.Serialize(solicitud);

            Console.WriteLine($"  [Cliente RPC] Enviando JSON: {jsonSolicitud}");

            // ── 2. Realizar la llamada HTTP POST ──────────────
            // En RPC real, esto lo hace el STUB automáticamente
            StringContent contenidoHttp = new StringContent(
                jsonSolicitud,
                Encoding.UTF8,
                TIPO_CONTENIDO
            );

            HttpResponseMessage respuestaHttp =
                await clienteHttp.PostAsync(URL_SERVIDOR, contenidoHttp);

            respuestaHttp.EnsureSuccessStatusCode();

            // ── 3. Deserializar la respuesta ──────────────────
            string jsonRespuesta = await respuestaHttp.Content.ReadAsStringAsync();
            RespuestaRPC? respuesta = JsonSerializer.Deserialize<RespuestaRPC>(
                jsonRespuesta,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (respuesta == null)
                throw new InvalidOperationException("Respuesta del servidor vacía o mal formada.");

            // ── 4. Presentar el resultado al usuario ──────────
            if (respuesta.Exitoso)
            {
                Console.WriteLine($"\n  ┌─────────────────────────────────────────────");
                Console.WriteLine($"  │  Función invocada : {respuesta.NombreFuncion}");
                Console.WriteLine($"  │  Parámetro enviado: {respuesta.ParametroNumero}");
                Console.WriteLine($"  │  RESULTADO        : {respuesta.ParametroNumero}² = {respuesta.Resultado}");
                Console.WriteLine($"  │  Mensaje servidor : {respuesta.Mensaje}");
                Console.WriteLine($"  │  Marca de tiempo  : {respuesta.MarcaTiempo}");
                Console.WriteLine($"  └─────────────────────────────────────────────\n");
            }
            else
            {
                Console.WriteLine($"  [Error RPC] {respuesta.Mensaje}");
            }
        }
        catch (HttpRequestException errorHttp)
        {
            Console.WriteLine($"  [Cliente RPC] Error de conexión: {errorHttp.Message}");
            Console.WriteLine("  ¿Está el ServidorRPC en ejecución?");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("  [Cliente RPC] Tiempo de espera agotado (timeout 10s).");
        }
        catch (Exception error)
        {
            Console.WriteLine($"  [Cliente RPC] Error inesperado: {error.Message}");
        }
    }
}