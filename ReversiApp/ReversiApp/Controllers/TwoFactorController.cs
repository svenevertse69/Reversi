using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReversiApp.Models;
using ReversiApp.Models.Manage;
using ReversiApp.Services;

namespace ReversiApp.Controllers
{
    [Authorize]
    public class TwoFactorController : Controller
    {
        private readonly UserManager<Speler> _userManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly ILogger _logger;
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public TwoFactorController(
       UserManager<Speler> userManager,
       UrlEncoder urlEncoder,
       ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _urlEncoder = urlEncoder;
            _logger = loggerFactory.CreateLogger<ManageController>();
        }


        //
        // GET: /TwoFactor/Index
        public IActionResult Index()
        {
            return View();
        }

        //
        // GET: /TwoFactor/Register
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var model = new EnableAuthenticator();
            await LoadSharedKeyAndQrCodeUriAsync(user, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Set(EnableAuthenticator model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Register), "TwoFactor");
            }

            // Strip spaces and hypens
            var verificationCode = model.Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                return RedirectToAction(nameof(Register), "TwoFactor");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            model.StatusMessage = "Your authenticator app has been verified.";

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                model.RecoveryCodes = recoveryCodes.ToArray();
                //TODO recovery code pagina
                return View("DisplayRecoveryCodes", new DisplayRecoveryCodes { Codes = recoveryCodes });
            }
            else
            {
                return RedirectToAction(nameof(Index), "TwoFactor");
            }
        }

        //
        // POST: /TwoFactor/ResetAuthenticatorKey
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticatorKey()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                _logger.LogInformation(1, "User reset authenticator key.");
            }
            return RedirectToAction(nameof(Index), "TwoFactor");
        }

        //
        // POST: /TwoFactor/Disable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                _logger.LogInformation(1, "User disabled 2fa.");
            }
            return RedirectToAction(nameof(Index), "TwoFactor");
        }

        //
        // POST: /TwoFactor/GenerateRecoveryCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecoveryCode()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5);
                _logger.LogInformation(1, "User generated new recovery code.");
                return View("DisplayRecoveryCodes", new DisplayRecoveryCodes { Codes = codes });
            }
            return View("Error");
        }

        [HttpGet]
        public IActionResult DisplayRecoveryCodes()
        {
            return View();
        }
        #region Helpers

        private Task<Speler> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(Speler user, EnableAuthenticator model)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = FormatKey(unformattedKey);

            var email = await _userManager.GetEmailAsync(user);
            model.AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("SvenReversi.hbo-ict.org"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
        #endregion
    }
}