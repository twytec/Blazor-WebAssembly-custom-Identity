using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace BlazorWasmIdentity.Auth
{
    public class AuthService
    {
        public string Token { get; set; }
        public const string TokenKey = "AuthToken";
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public event EventHandler UserIsLogin;

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetTokenAsync()
        {
            //Das JS File is under wwwroot/js/storage.js
            Token = await _jsRuntime.InvokeAsync<string>("getFromStorage", TokenKey);
            return Token;
        }

        public async Task<string> LoginAsync(SharedModels.LoginViewModel loginViewModel)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(loginViewModel);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");


            var res = await _httpClient.PostAsync($"{SharedModels.Auth.JwtIssuer}/api/auth/login", content);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                json = await res.Content.ReadAsStringAsync();
                loginViewModel = System.Text.Json.JsonSerializer.Deserialize<SharedModels.LoginViewModel>(json, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true});

                Token = loginViewModel.Token;
                await _jsRuntime.InvokeAsync<bool>("saveToStorage", TokenKey, Token);
                UserIsLogin?.Invoke(null, null);
                return string.Empty;
            }

            return "Email or password incorrect";
        }
    }
}
