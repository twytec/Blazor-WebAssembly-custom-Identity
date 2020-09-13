using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedModels;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (await _userManager.FindByNameAsync(model.Email) == null)
                {
                    //Default admin accont
                    if (model.Email.ToLower() == $"admin@local.de")
                    {
                        var user = new AppUser
                        {
                            UserName = model.Email,
                            Email = model.Email,
                        };
                        var res = await _userManager.CreateAsync(user, model.Password);
                        if (res.Succeeded == false)
                        {
                            //Show in res.Errors
                            return BadRequest("Password");
                        }

                        await _roleManager.CreateAsync(new IdentityRole(Auth.AdminRoleName));
                        await _roleManager.CreateAsync(new IdentityRole(Auth.UserRoleName));

                        await _userManager.AddToRoleAsync(user, Auth.AdminRoleName);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

                if (result.Succeeded)
                {
                    var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);

                    model.Token = await GenerateJwtTokenAsync(appUser);
                    model.Password = "";
                    return Ok(model);
                }
            }
            catch (Exception)
            {

            }

            return BadRequest();
        }

        public async Task<IActionResult> Register(LoginViewModel model)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
            };
            var res = await _userManager.CreateAsync(user, model.Password);
            if (res.Succeeded)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                
                if (result.Succeeded)
                {
                    var token = await GenerateJwtTokenAsync(user);
                    return Ok(token);
                }
            }

            if (res.Errors.Count() > 0)
                return BadRequest(res.Errors.ElementAt(0).Description);

            return BadRequest("Unkown error");
        }

        #region Helper

        private async Task<string> GenerateJwtTokenAsync(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Auth.JwtKey);

            List<Claim> c = new List<Claim>() { 
                new Claim(ClaimTypes.Name, user.Email)
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var item in roles)
            {
                c.Add(new Claim(ClaimTypes.Role, item));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(c),
                Expires = DateTime.UtcNow.AddDays(Auth.JwtExpireDays),
                Audience = Auth.JwtIssuer,
                Issuer = Auth.JwtIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion
    }
}
