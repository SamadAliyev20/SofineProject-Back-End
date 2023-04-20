using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels.AccountViewModels;

namespace SofineProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;  
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            AppUser appUser = new AppUser

            {
                Name = registerVM.Name,
                Email = registerVM.Email,
                SurName = registerVM.SurName,
                UserName = registerVM.UserName
            };

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(registerVM);


            }

            await _userManager.AddToRoleAsync(appUser, "Member");
            return RedirectToAction("Login","Account");
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

			return RedirectToAction("Index", "Home");
		}
		[HttpGet]
		public async Task<ActionResult> Logout()
		{
			await _signInManager.SignOutAsync();

			return RedirectToAction("Index", "Home");
		}
	}
}
