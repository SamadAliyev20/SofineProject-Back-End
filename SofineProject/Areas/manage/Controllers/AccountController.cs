using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using SofineProject.Areas.manage.ViewModels.AccountVMs;
using SofineProject.Models;
using SofineProject.ViewModels;
using SofineProject.ViewModels.AccountViewModels;
using System.Data;

namespace SofineProject.Areas.manage.Controllers
{
    [Area("Manage")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly SmtpSetting _smtpSetting;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager, IOptions<SmtpSetting> smtpSetting)
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _smtpSetting = smtpSetting.Value;
        }

		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginAdminVM loginVM)
		{
			if (!ModelState.IsValid) { return View(loginVM); }
			AppUser appUser = await _userManager.Users
				.FirstOrDefaultAsync(u => u.NormalizedEmail == loginVM.Email.Trim().ToUpperInvariant());
			if (appUser == null)
			{
				ModelState.AddModelError("", "Email Ve ya Sifre Yanlisdir");
				return View(loginVM);
			}
			Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(appUser,
				loginVM.Password, loginVM.RemindMe, true);
			if (signInResult.IsLockedOut)
			{
				ModelState.AddModelError("", "Hesabiniz Bloklanib");
				return View(loginVM);
			}
			if (!signInResult.Succeeded)
			{
				ModelState.AddModelError("", "Email Ve ya Sifre Yanlisdir");
				return View(loginVM);
			}
			TempData["ToasterMessage4"] = "Login successfully!";
			return RedirectToAction("index", "dashboard", new { areas = "manage" });
		}
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["ToasterMessage4"] = "Logout successfully!";
            return RedirectToAction("Login", "Account", new { areas = "manage" });
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);
            ProfileAdminVM profileAdminVM = new ProfileAdminVM
            {
                Name = appUser.Name,
                SurName = appUser.SurName,
                UserName = appUser.UserName,
                Email = appUser.Email,
            };

            return View(profileAdminVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileAdminVM profileVM)
        {
            if (!ModelState.IsValid) return View(profileVM);

            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            appUser.Name = profileVM.Name;
            appUser.SurName = profileVM.SurName;

            if (appUser.NormalizedUserName != profileVM.UserName.Trim().ToUpperInvariant())
            {
                appUser.UserName = profileVM.UserName;
            }
            if (appUser.NormalizedEmail != profileVM.Email.Trim().ToUpperInvariant())
            {
                appUser.Email = profileVM.Email;
            }


            IdentityResult identityResult = await _userManager.UpdateAsync(appUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                    return View(profileVM);
                }
            }
            await _signInManager.SignInAsync(appUser, true);

            if (!string.IsNullOrWhiteSpace(profileVM.OldPassword))
            {
                if (!await _userManager.CheckPasswordAsync(appUser, profileVM.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Köhnə şifrə Yanlışdır");
                    return View(profileVM);
                }
                if (profileVM.OldPassword == profileVM.Password)
                {
                    ModelState.AddModelError("Password", "Köhnə şifrə ilə yeni şifrə eyni ola bilməz!");
                    return View(profileVM);
                }

                string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

                identityResult = await _userManager.ResetPasswordAsync(appUser, token, profileVM.Password);

                if (!identityResult.Succeeded)
                {
                    foreach (IdentityError identityError in identityResult.Errors)
                    {
                        ModelState.AddModelError("", identityError.Description);
                        return View(profileVM);
                    }
                }
            }
            TempData["ToasterMessage4"] = "Profil uğurla dəyişdirildi!";
            return RedirectToAction("index", "dashboard", new { areas = "manage" });
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordAdmin forgetPasswordAdmin)
        {
            if (!ModelState.IsValid) return View(forgetPasswordAdmin);

            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == forgetPasswordAdmin.Email.Trim().ToUpperInvariant());

            if (appUser == null)
            {
                return RedirectToAction("ForgotPassword", "account", new {Areas="manage"});
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

            string url = Url.Action("ResetPassword", "Account", new { token, email = forgetPasswordAdmin.Email, Areas = "manage" },
                HttpContext.Request.Scheme, HttpContext.Request.Host.ToString());

            string fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templaes", "PassReset.html");
            string templateContent = await System.IO.File.ReadAllTextAsync(fullpath);
            templateContent = templateContent.Replace("{{url}}", url);

            MimeMessage mimeMessage = new();
            mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
            mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
            mimeMessage.Subject = "Reset Password";
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = templateContent
            };
            using (SmtpClient smtpClient = new())
            {
                await smtpClient.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
                await smtpClient.SendAsync(mimeMessage);
                await smtpClient.DisconnectAsync(true);
                smtpClient.Dispose();
            }

            TempData["ToasterMessage2"] = "Your password reset request has been sent to your email. Please check your email!";
            return RedirectToAction("Login", "account", new { Areas = "manage" });



        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordAdminVM { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordAdminVM resetPasswordAdminVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPasswordAdminVM);
            }
            AppUser appUser = await _userManager.FindByEmailAsync(resetPasswordAdminVM.Email);

            if (appUser == null) { return NotFound(); }

            IdentityResult identityResult = await _userManager.ResetPasswordAsync(appUser, resetPasswordAdminVM.Token, resetPasswordAdminVM.Password);

            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
            }
            TempData["ToasterMessage4"] = "Your password has been successfully changed!";
            return RedirectToAction("Login", "account", new { Areas = "manage" });
        }

        //#region create SuperAdmin
        //[HttpGet]

        //public async Task<IActionResult> CreateUser()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        Name = "Super",
        //        SurName = "Admin",
        //        UserName = "SuperAdmin",
        //        Email = "superadmin@gmail.com"

        //    };

        //    await _userManager.CreateAsync(appUser, "SuperAdmin133");
        //    await _userManager.AddToRoleAsync(appUser, "SuperAdmin");

        //    return Content("Success");
        //}

        //#endregion\






    }
}
