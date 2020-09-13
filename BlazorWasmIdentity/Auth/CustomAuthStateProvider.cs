using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWasmIdentity.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthService _authService;

        public CustomAuthStateProvider(AuthService authService)
        {
            _authService = authService;
            _authService.UserIsLogin += _authService_UserIsLogin;
        }

        private void _authService_UserIsLogin(object sender, EventArgs e)
        {
            this.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = await _authService.GetTokenAsync();

            Console.WriteLine($"Token: {token}");
            if (string.IsNullOrEmpty(token) == false)
            {
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidAudience = SharedModels.Auth.JwtIssuer,
                    ValidIssuer = SharedModels.Auth.JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SharedModels.Auth.JwtKey))
                };
                ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
                var auth = new AuthenticationState(principal);

                return auth;
            }

            //Kein Token vorhanden oder konnte nicht entschlüsselt werden
            var anonymous = new ClaimsIdentity();
            return new AuthenticationState(new ClaimsPrincipal(anonymous));
        }
    }
}
