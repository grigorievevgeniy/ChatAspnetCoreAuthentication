using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ChatAspnetCoreAuthentication.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            //TokenOptions token;

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);




                Microsoft.AspNetCore.Identity.SignInResult inResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

                //JsonResult jsonResult = inResult.GetType();

                var result2 = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                //var resp = await fetch("/account/token", { method: "POST", body: formData });


                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // Код на TypeScript

            //public async loginFormSubmitted(evt: Event) {
            //    try
            //    {
            //        evt.preventDefault();

            //        // Send the username and password to the server
            //        const formData = new FormData(loginForm);
            //        const resp = await fetch("/account/token", { method: "POST", body: formData });

            //        if (resp.status !== 200)
            //        {
            //            this.error = `HTTP ${ resp.status}
            //            error from server`;
            //            return;
            //        }

            //        const json = await resp.json();

            //        if (json["error"])
            //        {
            //            this.error = `Login error: ${ json["error"]}`;
            //            return;
            //        }
            //        else
            //        {
            //            this.loginToken = json["token"];
            //        }

            //        // Update rendering while we connect
            //        this.render();

            //        // Connect, using the token we got.
            //        this.connection = new signalR.HubConnectionBuilder()
            //            .withUrl("/hubs/chat", { accessTokenFactory: () => this.loginToken })
            //    .build();

            //        this.connection.on("ReceiveSystemMessage", (message) => this.receiveMessage(message, "green"));
            //        this.connection.on("ReceiveDirectMessage", (message) => this.receiveMessage(message, "blue"));
            //        this.connection.on("ReceiveChatMessage", (message) => this.receiveMessage(message));
            //        await this.connection.start();
            //        this.connectionStarted = true;

            //    }
            //    catch (e)
            //    {
            //        this.error = `Error connecting: ${ e}`;
            //    }
            //    finally
            //    {
            //        // Update rendering with any final state.
            //        this.render();
            //    }
            //}


            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
