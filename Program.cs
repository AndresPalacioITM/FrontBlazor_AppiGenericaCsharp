using FrontBlazor_AppiGenericaCsharp.Components;
using FrontBlazor_AppiGenericaCsharp.Services;
using FrontBlazor_AppiGenericaCsharp.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using FrontBlazor_AppiGenericaCsharp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar HttpClient para conectarse a la API
// La URL base apunta a la API ApiGenericaCsharp que corre en el puerto 5034
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5230")
});

// Registrar el servicio ApiService para manejar las llamadas a la API
builder.Services.AddScoped<FrontBlazor_AppiGenericaCsharp.Services.ApiService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState(); 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "BlazorAuth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });
builder.Services.AddScoped<AuthService>();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseAntiforgery();

app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
