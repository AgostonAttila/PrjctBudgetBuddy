using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;

using blazorapp.Helpers;
using blazorapp.Services;
using Microsoft.AspNetCore.Components;
using blazorapp.Models;


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

            if (!String.IsNullOrWhiteSpace(token))
            {

                try
                {
                    //Must be build in some checkers issuer, audience etc
                    identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType");
                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                    //_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken.Replace("\"", ""));

                }
                catch (Exception e)
                {
                    await _localStorageService.RemoveItemAsync("authToken");
                    identity = new ClaimsIdentity();
                }
            }
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;

        }       

    }   
}
