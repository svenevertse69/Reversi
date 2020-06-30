using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReversiApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiApp.Services
{
    public class EmailSignInManager : SignInManager<Speler>
    {
        public EmailSignInManager(UserManager<Speler> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<Speler> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<Speler>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<Speler> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public async Task<SignInResult> EmailPasswordSignInAsync(string email, string password,
            bool isPersistent, bool lockoutOnFailure)
        {
            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }
    }
}
