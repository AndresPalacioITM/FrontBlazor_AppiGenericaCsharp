using Microsoft.JSInterop;

namespace FrontBlazor_AppiGenericaCsharp.Services;

public static class LocalStorageService
{
    private static IJSRuntime? _jsRuntime;

    public static void Initialize(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public static async Task SaveTokenAsync(string token)
    {
        if (_jsRuntime is not null)
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
    }

    public static async Task<string?> GetTokenAsync()
    {
        if (_jsRuntime is not null)
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        return null;
    }

    public static async Task RemoveTokenAsync()
    {
        if (_jsRuntime is not null)
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
    }

    public static async Task SaveUserAsync(string user)
    {
        if (_jsRuntime is not null)
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", user);
    }

    public static async Task<string?> GetUserAsync()
    {
        if (_jsRuntime is not null)
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "currentUser");
        return null;
    }

    public static async Task RemoveUserAsync()
    {
        if (_jsRuntime is not null)
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "currentUser");
    }
}