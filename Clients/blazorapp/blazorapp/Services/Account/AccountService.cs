using blazorapp.AuthStateProvider;
using blazorapp.Models.DTO;
using blazorapp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text.Json;
using System.Text;


namespace blazorapp.Services.Account
{
	public class AccountService : IAccountService
	{

		private readonly HttpClient _httpClient;
		private readonly AuthenticationStateProvider _authStateProvider;
		private readonly JsonSerializerOptions _options;
		private readonly ILocalStorageService _localStorageService;
		private IHttpService _httpService;
		private NavigationManager _navigationManager;

		private static System.Timers.Timer refreshTokenTimer;


		public AccountService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, ILocalStorageService localStorageService, IHttpService httpService, NavigationManager navigationManager)
		{
			_httpClient = httpClient;
			_localStorageService = localStorageService;
			_navigationManager = navigationManager;
			_authStateProvider = authStateProvider;
			_httpService = httpService;
			_options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		}


		//interceptor only one place
		//global error handling
		//authorize attribute


		public async Task<Result<string>> Register(RegisterDTO registerDTO)
		{
			Dictionary<string, string> registerObj = new Dictionary<string, string>
			{
				{"email", registerDTO.Email},
				{"password", registerDTO.Password}
			};

			try
			{
				var result = await _httpService.Post<string>("register", registerObj);
				
				if (String.IsNullOrWhiteSpace(result))
					return new Result<string> { IsSuccess = true };
			}
			catch (Exception e)
			{
				return new Result<string> { IsSuccess = false, Errors = new List<string> { e.Message } };
			}

			return new Result<string> { IsSuccess = false };
		}

		public async Task<Result<TokenDTO>> Login(LoginDTO loginDTO)
		{
			TokenDTO tokenDTO = null;

			try
			{
				tokenDTO = await _httpService.Post<TokenDTO>("login", loginDTO);

				 if (String.IsNullOrWhiteSpace(tokenDTO?.AccessToken))
					return new Result<TokenDTO> { IsSuccess = false };

			}
			catch (Exception e)
			{
				return new Result<TokenDTO> { IsSuccess = false, Errors = new List<string> { e.Message } };
			}
			await _localStorageService.SetItemAsync("authToken", tokenDTO.AccessToken);
			await _localStorageService.SetItemAsync("refreshToken", tokenDTO.RefreshToken);

			await ((CustomAuthStateProvider)_authStateProvider).GetAuthenticationStateAsync();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", tokenDTO.AccessToken);

			StartRefreshTokenTimer();

			return new Result<TokenDTO> { IsSuccess = true };
		}

		public async Task<Result<string>> Logout()
		{
			//var authResult = await _httpClient.PostAsync("Account/logout", new StringContent("", Encoding.UTF8, "application/json"));
			//var authContent = await authResult.Content.ReadAsStringAsync();
			//var result = JsonSerializer.Deserialize<Result<string>>(authContent, _options);

			//if (!authResult.IsSuccessStatusCode)
			//	return result;

			await _localStorageService.RemoveItemAsync("authToken");
			await _localStorageService.RemoveItemAsync("refreshToken");

			await ((CustomAuthStateProvider)_authStateProvider).GetAuthenticationStateAsync();
			_httpClient.DefaultRequestHeaders.Authorization = null;

			StopRefreshTokenTimer();

			_navigationManager.NavigateTo("/");
			return new Result<string> { IsSuccess = true, Data = "Logout is Success" }; ;
		}

		public async Task<string> Refresh()
		{
			var token = await _localStorageService.GetItemAsync<string>("refreshToken");

			var content = JsonSerializer.Serialize(new RefreshTokenDTO { RefreshToken = token });
			var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
			var refreshResult = await _httpClient.PostAsync("refresh", bodyContent);

			if (!refreshResult.IsSuccessStatusCode)
				throw new ApplicationException("Something went wrong during the refresh token action");

			var refreshContent = await refreshResult.Content.ReadAsStringAsync();

			if (refreshContent != "''")
			{

				Result<string> result = JsonSerializer.Deserialize<Result<string>>(refreshContent, _options);

				await _localStorageService.SetItemAsync("refreshToken", result.Data);
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Data);
			}
			return "";
		}

		public Task<Result<string>> ConfirmEmail(string token, string email)
		{
			throw new NotImplementedException();
		}

		public async Task<Result<string>> ResendConfirmationEmail(string email)
		{
			var content = JsonSerializer.Serialize(email);
			var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

			var result = await _httpClient.PostAsJsonAsync("resendConfirmationEmail", bodyContent);
			Result<string> resultContent = await result.Content.ReadFromJsonAsync<Result<string>>();

			if (!resultContent.IsSuccess)
			{
				return new Result<string> { IsSuccess = false }; ;
			}

			return new Result<string> { IsSuccess = true };
		}

		public Task<Result<string>> ForgotPassword(string email)
		{
			throw new NotImplementedException();
		}

		public Task<Result<string>> ResetPassword(string email)
		{
			throw new NotImplementedException();
		}

		public async Task<IIdentity> GetCurrentUser()
		{
			return (await _authStateProvider.GetAuthenticationStateAsync()).User.Identity;
		}

		public void StartRefreshTokenTimer()
		{

			//const jwtToken = JSON.parse(atob(user.token.split(".")[1]));
			//const expires = new Date(jwtToken.exp * 1000);
			//const timeout = expires.getTime() - Date.now() - 60 * 1000;
			//this.refreshTokenTimeout = setTimeout(this.refreshToken, timeout);

			if (refreshTokenTimer is null)
			{
				refreshTokenTimer = new System.Timers.Timer(1000 * 60 * 4);
				refreshTokenTimer.Elapsed += (sender, args) =>
				{
					Refresh();
				};
				refreshTokenTimer.AutoReset = true;
				refreshTokenTimer.Enabled = true;
			}
		}

		public void StopRefreshTokenTimer()
		{
			if (refreshTokenTimer is not null)
			{
				refreshTokenTimer.Stop();
				refreshTokenTimer.Dispose();
				refreshTokenTimer = null;
			}
		}

		public void Dispose()
		{
			StopRefreshTokenTimer();
		}



	}
}
