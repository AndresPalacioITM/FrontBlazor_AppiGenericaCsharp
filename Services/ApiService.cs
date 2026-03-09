using System.Net.Http.Json;
using System.Text.Json;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public ApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<Dictionary<string, object?>>> ListarAsync(string tabla)
        {
            try
            {
                // Hace GET a la API y obtiene la respuesta como JSON
                var respuesta = await _http.GetFromJsonAsync<JsonElement>($"/api/{tabla}", _jsonOptions);

                // Extrae la propiedad "datos" de la respuesta
                if (respuesta.TryGetProperty("datos", out JsonElement datos))
                {
                    return ConvertirDatos(datos);
                }

                return new List<Dictionary<string, object?>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al listar {tabla}: {ex.Message}");
                return new List<Dictionary<string, object?>>();
            }
        }
        public async Task<(bool exito, string mensaje)> CrearAsync(
            string tabla, Dictionary<string, object?> datos)
        {
            try
            {
                var respuesta = await _http.PostAsJsonAsync($"/api/{tabla}", datos);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }
        public async Task<(bool exito, string mensaje)> ActualizarAsync(
            string tabla, string nombreClave, string valorClave,
            Dictionary<string, object?> datos)
        {
            try
            {
                var respuesta = await _http.PutAsJsonAsync(
                    $"/api/{tabla}/{nombreClave}/{valorClave}", datos);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }
        public async Task<(bool exito, string mensaje)> EliminarAsync(
            string tabla, string nombreClave, string valorClave)
        {
            try
            {
                var respuesta = await _http.DeleteAsync(
                    $"/api/{tabla}/{nombreClave}/{valorClave}");
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }
        private List<Dictionary<string, object?>> ConvertirDatos(JsonElement datos)
        {
            var lista = new List<Dictionary<string, object?>>();

            foreach (var fila in datos.EnumerateArray())
            {
                var diccionario = new Dictionary<string, object?>();

                foreach (var propiedad in fila.EnumerateObject())
                {
                    // Convierte cada valor JSON a su tipo .NET correspondiente
                    diccionario[propiedad.Name] = propiedad.Value.ValueKind switch
                    {
                        JsonValueKind.String => propiedad.Value.GetString(),
                        JsonValueKind.Number => propiedad.Value.TryGetInt32(out int i) ? i : propiedad.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => propiedad.Value.GetRawText()
                    };
                }

                lista.Add(diccionario);
            }

            return lista;
        }
    }
}