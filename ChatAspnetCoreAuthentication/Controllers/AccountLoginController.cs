﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ChatAspnetCoreAuthentication.Controllers
{
    //[Route("account/token")]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private static readonly SigningCredentials SigningCreds = new SigningCredentials(Startup.SecurityKey, SecurityAlgorithms.HmacSha256);

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToPage("/Index");
        }

        [HttpPost]
        public async Task<IActionResult> Token(string email, string password)
        {
            try
            {
                // Check the password but don't "sign in" (which would set a cookie)
                var user = await _signInManager.UserManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Json(new
                    {
                        error = "Login failed"
                    });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
                if (result.Succeeded)
                //if (email == "1" && password == "1")
                {
                    var principal = await _signInManager.CreateUserPrincipalAsync(user);
                    var token = new JwtSecurityToken(
                        "SignalRAuthenticationSample",
                        "SignalRAuthenticationSample",
                        principal.Claims,
                        expires: DateTime.UtcNow.AddDays(30),
                        signingCredentials: SigningCreds);



                    return Json(new
                    {
                        token = _tokenHandler.WriteToken(token),
                    });


                    //return Json(new
                    //{
                    //    token = _tokenHandler.WriteToken(token),
                    //    StartMessage = "Добро пожаловать!!! " +
                    //    "\r\nВ окне слева Вы видите список доступных Вам комнат." +
                    //    "\r\n\r\nВы можете зайти в одну из них при помощи команды: " +
                    //    "\r\n//room connect {Название комнаты}." +
                    //    "\r\n\r\nЛибо создать свою новую комнату при помощи команды: " +
                    //    "\r\n//room create {Название комнаты}." +
                    //    "\r\n\r\n\r\n\r\n" +
                    //    "Для проверки и автоматической генерации комнат и ролей, наберите:" +
                    //    "\r\n//si",
                    //    ListRooms = "Room1\r\nRoom2"
                    //});
                }
                else
                {
                    return Json(new
                    {
                        error = result.IsLockedOut ? "User is locked out" : "Login failed"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.ToString()
                });
            }
        }
        //[HttpPost]
        //public async Task<IActionResult> Register(string email, string password)
        //{
        //    try
        //    {
        //        // временно
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            error = ex.ToString()
        //        });
        //    }
        //}
    }
}
