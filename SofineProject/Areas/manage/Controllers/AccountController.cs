using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.Areas.manage.ViewModels.AccountVMs;
using SofineProject.Models;
using System.Data;

namespace SofineProject.Areas.manage.Controllers
{
    [Area("Manage")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginVM loginVM)
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
            return RedirectToAction("Login", "Account", new { areas = "manage" });
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);
            ProfileVM profileVM = new ProfileVM
            {
                Name = appUser.Name,
                SurName = appUser.SurName,
                UserName = appUser.UserName,
                Email = appUser.Email,
            };

            return View(profileVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVM profileVM)
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
    }
}
