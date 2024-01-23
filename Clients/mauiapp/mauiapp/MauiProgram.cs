using mauiapp.Services;
using mauiapp.Services.Account;
using mauiapp.StateContainers;
using mauiapp.StateProviders;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace mauiapp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();


            builder.Services.AddHttpClientInterceptor();
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5296/")
                //BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            }
            .EnableIntercept(sp));

            //Services
            builder.Services.AddScoped<IAccountService, AccountService>();

            builder.Services.AddScoped<RefreshTokenService>();
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
            builder.Services.AddScoped<IHttpService, HttpService>();

            //State containers
            builder.Services.AddSingleton<AppState>();

            //Authorization - Authentication
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<HttpInterceptorService>();


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();

            static void SubscribeHttpClientInterceptorEvents2(MauiApp host)
            {
                // Subscribe IHttpClientInterceptor's events.
                HttpInterceptorService httpInterceptor = host.Services.GetService<HttpInterceptorService>();
                httpInterceptor.RegisterEvent();

            }
        }
    }
}
