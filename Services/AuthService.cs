using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using FrontBlazor_AppiGenericaCsharp.Providers;
using FrontBlazor_AppiGenericaCsharp.Models;
using Microsoft.Extensions.Options;

namespace FrontBlazor_AppiGenericaCsharp.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ApiSettings _apiSettings;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _apiSettings = apiSettings.Value;
    }

    // Llamar al endpoint de login de tu API
    public async Task<bool> Login(string email, string password)
    {
        // 1. Construir el objeto que espera el endpoint /token
        var requestBody = new
        {
            tabla = _apiSettings.TablaUsuarios,
            campoUsuario = _apiSettings.CampoUsuario,
            campoContrasena = _apiSettings.CampoContrasena,
            usuario = email,
            contrasena = password
        };

        // 2. Llamar al endpoint correcto: POST /token
        var response = await _httpClient.PostAsJsonAsync("api/Autenticacion/token", requestBody);

        if (!response.IsSuccessStatusCode)
            return false;

        // 3. Leer la respuesta y extraer el token
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        string token = result?.token;

        if (string.IsNullOrEmpty(token))
            return false;

        // 4. Guardar token y notificar
        await _localStorage.SetItemAsync("authToken", token);
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(token);

        return true;
    }

    // Cerrar sesión
    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }

    // Obtener el token guardado (para usarlo en ApiService)
    public async Task<string> GetToken()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }

    // Clase que mapea la respuesta de tu API
    private class LoginResponse
    {
        public int estado { get; set; } 
        public string mensaje { get; set; }
        public string usuario { get; set; }
        public string token { get; set; } 
        public DateTime expiracion { get; set; }
    }
}