using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;

using blazorapp.Helpers;
using blazorapp.Services;



namespace blazorapp.AuthStateProvider
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly HttpClient _http;
        private readonly AuthenticationState _anonymous;

        public CustomAuthStateProvider(ILocalStorageService localStorageService, HttpClient http)
        {
            _localStorageService = localStorageService;
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorageService.GetItemAsync<string>("authToken");
            //if (string.IsNullOrWhiteSpace(token))
            //	return _anonymous;

            var identity = new ClaimsIdentity();
            _http.DefaultRequestHeaders.Authorization = null;
            try
            {
                identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType");
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                //_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken.Replace("\"", ""));

            }
            catch (Exception)
            {
                await _localStorageService.RemoveItemAsync("authToken");
                identity = new ClaimsIdentity();
            }

            var state = new AuthenticationState(new ClaimsPrincipal(identity));
            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;


        }

        //public void NotifyUserAuthentication(string token)
        //{
        //	var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType"));
        //	var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        //	NotifyAuthenticationStateChanged(authState);
        //}

        //public void NotifyUserLogout()
        //{
        //	var authState = Task.FromResult(_anonymous);
        //	NotifyAuthenticationStateChanged(authState);
        //}



    }
}
