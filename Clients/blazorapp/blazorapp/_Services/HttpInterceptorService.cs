using System.Net.Http.Headers;
using Toolbelt.Blazor;

namespace blazorapp.Services
{
    public class HttpInterceptorService
    {
        private readonly HttpClientInterceptor _interceptor;
        private readonly RefreshTokenService _refreshTokenService;

        public HttpInterceptorService(HttpClientInterceptor interceptor, RefreshTokenService refreshTokenService)
        {
            _interceptor = interceptor;
            _refreshTokenService = refreshTokenService;
        }

        public void RegisterEvent() => _interceptor.BeforeSendAsync += InterceptBeforeHttpAsync;

        public async Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs e)
        {
            //Console.WriteLine("BeforeSend event of HttpClientInterceptor");
            //Console.WriteLine($"  - {args.Request.Method} {args.Request.RequestUri}");

            var absPath = e.Request.RequestUri.AbsolutePath;

            if (!absPath.Contains("Token") && !absPath.Contains("Account"))
            {
                var token = await _refreshTokenService.TryRefreshToken();

                if (!string.IsNullOrEmpty(token))
                {
                    e.Request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                }
            }
        }


        //public async Task InterceptAfterHttpAsync(object sender, HttpClientInterceptorEventArgs e)
        //{
        //Console.WriteLine("AfterSend event of HttpClientInterceptor");
        //Console.WriteLine($"  - {args.Request.Method} {args.Request.RequestUri}");
        //Console.WriteLine($"  - HTTP Status {args.Response?.StatusCode}");

        //var capturedContent = await args.GetCapturedContentAsync();

        //Console.WriteLine($"  - Content Headers");
        //foreach (var headerText in capturedContent.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))
        //{
        //	Console.WriteLine($"    - {headerText}");
        //}

        //var httpContentString = await capturedContent.ReadAsStringAsync();
        //Console.WriteLine($"  - HTTP Content \"{httpContentString}\"");
        //}


        public void DisposeEvent() => _interceptor.BeforeSendAsync -= InterceptBeforeHttpAsync;
    }
}
