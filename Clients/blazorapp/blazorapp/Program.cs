using blazorapp;
using blazorapp.Services.Account;
using blazorapp.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using blazorapp.AuthStateProvider;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.NetworkInformation;
using blazorapp.StateContainers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClientInterceptor();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:5296/")
    //BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
}
.EnableIntercept(sp));


//Services
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IHttpService, HttpService>();

//builder.Services.AddOidcAuthentication(options =>
//{ 
//    builder.Configuration.Bind("Local", options.ProviderOptions);
//});

//State containers
builder.Services.AddSingleton<AppState>();

//Authorization - Authentication
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<HttpInterceptorService>();

await builder.Build().RunAsync();

static void SubscribeHttpClientInterceptorEvents2(WebAssemblyHost host)
{
    // Subscribe IHttpClientInterceptor's events.
    HttpInterceptorService httpInterceptor = host.Services.GetService<HttpInterceptorService>();
    httpInterceptor.RegisterEvent();

}

