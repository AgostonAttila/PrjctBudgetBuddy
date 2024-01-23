using mauiapp.Services.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;

namespace mauiapp.Services
{
    public class RefreshTokenService
    {
        private readonly AuthenticationStateProvider _authProvider;
        private readonly IAccountService _accountService;
        private readonly IConfiguration _config;

        public RefreshTokenService(AuthenticationStateProvider authProvider, IAccountService accountService, IConfiguration config)
        {
            _authProvider = authProvider;
            _accountService = accountService;
            _config = config;
        }

        public async Task<string> TryRefreshToken()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            var exp = user.FindFirst(c => c.Type.Equals("exp")).Value;
            var expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));

            var timeUTC = DateTime.UtcNow;

            var diff = expTime - timeUTC;

            //double expMinutes = Convert.ToDouble(_config.GetSection("expiryInMinutes").Value);

            if (diff.TotalMinutes <= 5)
                return await _accountService.Refresh();

            return string.Empty;
        }
    }
}
