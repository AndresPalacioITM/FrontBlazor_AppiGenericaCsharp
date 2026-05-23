using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly AuthService _authService;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, AuthService authService)
        {
            _http = http;
            _authService = authService;
        }
            
        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<List<Dictionary<string, object?>>> ListarAsync(string tabla)
        {
            try
            {
                // Hace GET a la API y obtiene la respuesta como JSON
                await SetAuthorizationHeaderAsync();
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
                await SetAuthorizationHeaderAsync();
                var respuesta = await _http.PostAsJsonAsync($"/api/{tabla}", datos);
                var raw = await respuesta.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw))
                    return (respuesta.IsSuccessStatusCode, "Operación completada.");

                var contenido = JsonSerializer.Deserialize<JsonElement>(raw, _jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";
                if (!respuesta.IsSuccessStatusCode)
                {
                    Console.WriteLine($"ERROR API ({respuesta.StatusCode}): {raw}");
                    return (false, $"Error {respuesta.StatusCode}: {raw}");
                }
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
                await SetAuthorizationHeaderAsync();
                var respuesta = await _http.PutAsJsonAsync(
                    $"/api/{tabla}/{nombreClave}/{valorClave}", datos);
                var raw = await respuesta.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw))
                    return (respuesta.IsSuccessStatusCode, "Operación completada.");

                var contenido = JsonSerializer.Deserialize<JsonElement>(raw, _jsonOptions);

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
                await SetAuthorizationHeaderAsync();
                var respuesta = await _http.DeleteAsync(
                    $"/api/{tabla}/{nombreClave}/{valorClave}");
                var raw = await respuesta.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw))
                    return (respuesta.IsSuccessStatusCode, "Operación completada.");

                var contenido = JsonSerializer.Deserialize<JsonElement>(raw, _jsonOptions);

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