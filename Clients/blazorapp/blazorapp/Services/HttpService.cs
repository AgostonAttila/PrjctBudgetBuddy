using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using System.Text;
using blazorapp.Models.DTO;
using blazorapp.Models;
using System.Net.Http;

namespace blazorapp.Services
{
	public interface IHttpService
	{
		Task<T> Get<T>(string uri);
		Task<T> Post<T>(string uri, object value);
	}

	public class HttpService : IHttpService
	{
		private HttpClient _httpClient;
		private NavigationManager _navigationManager;
		private ILocalStorageService _localStorageService;
		private IConfiguration _configuration;
	

		public HttpService(
			HttpClient httpClient,
			NavigationManager navigationManager,
			ILocalStorageService localStorageService,
			IConfiguration configuration
		)
		{
			_httpClient = httpClient;
			_navigationManager = navigationManager;
			_localStorageService = localStorageService;
			_configuration = configuration;		
		}

		public async Task<T> Get<T>(string uri)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			return await sendRequest<T>(request);
		}

		public async Task<T> Post<T>(string uri, object value)
		{			
			var request = new HttpRequestMessage(HttpMethod.Post, uri);
			request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
			return await sendRequest<T>(request);
		}

		private async Task<T> sendRequest<T>(HttpRequestMessage request)
		{		
			//HTTP Interceptor
			//var token = await _localStorageService.GetItemAsync<string>("authToken");
			//var isApiUrl = !request.RequestUri.IsAbsoluteUri;
		
			//if (!String.IsNullOrWhiteSpace(token) != null && 
			//	isApiUrl
			//	&& !request.RequestUri.OriginalString.Contains("login")
			//	&& !request.RequestUri.OriginalString.Contains("register"))
			//	request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			using var response = await _httpClient.SendAsync(request);
					
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				_navigationManager.NavigateTo("login");
				return default;
			}
			
			if (!response.IsSuccessStatusCode)
			{
				//var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
				//throw new Exception(error["message"]);
				throw new Exception(response.StatusCode.ToString());
			}

			if (response.Content.Headers.ContentLength == 0)
				return default;

			var contentJSON = await response.Content.ReadAsStringAsync();
			var res = JsonSerializer.Deserialize<T>(contentJSON, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });				

			return  res;
		}
	}
}


//var content = JsonSerializer.Serialize(loginDTO);
//var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

//var authResult = await _httpClient.PostAsync("login", bodyContent);
//var authContent = await authResult.Content.ReadAsStringAsync();
//var result = JsonSerializer.Deserialize<Result<TokenDTO>>(authContent, _options);