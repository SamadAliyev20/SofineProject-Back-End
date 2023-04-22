using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels.AccountViewModels;
using System.Data;
using System.Net;

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
			TempData["ToasterMessage"] = "Login successfully!";
			return RedirectToAction("Index", "Home");
		}
		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			TempData["ToasterMessage"] = "LogOut successfully!";
			return RedirectToAction("Index", "Home");
		}

        [HttpGet]
        public async Task<ActionResult> Profile(Address? address)
        {
            AppUser appUser = await _userManager.Users
                .Include(u => u.Orders.Where(a => a.IsDeleted == false))
                .ThenInclude(u => u.OrderItems.Where(o => o.IsDeleted == false))
                .ThenInclude(oi => oi.Product)
                .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.NormalizedUserName == User.Identity.Name.ToUpperInvariant());

            ProfileVM profileVM = new ProfileVM
            {
                Addresses = appUser.Addresses,
                Orders = appUser.Orders,
                Name = appUser.Name,
                SurName = appUser.SurName,
                UserName = appUser.UserName,
                Email = appUser.Email,
                EditAddress = address
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
			TempData["ToasterMessage"] = "Account Details updated successfully!";
			await _signInManager.SignOutAsync();
			return RedirectToAction("Login", "account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> AddAddress(Address address)
        {
            AppUser appUser = await _userManager.Users
               .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.NormalizedUserName == User.Identity.Name.ToUpperInvariant());

            ProfileVM profileVM = new ProfileVM
            {
                Addresses = appUser.Addresses
            };

            if (!ModelState.IsValid)

            {
                return View(nameof(Profile), profileVM);
            }

            if (address.IsMain && appUser.Addresses != null && appUser.Addresses.Count() > 0 && appUser.Addresses.Any(a => a.IsMain))
            {
                appUser.Addresses.FirstOrDefault(a => a.IsMain).IsMain = false;
            }
            address.AppUserId = appUser.Id;
            address.CreatedBy = $"{appUser.Name} {appUser.SurName}";
            address.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
			TempData["ToasterMessage"] = "Address added successfully!";
			TempData["Tab"] = "address";
            return RedirectToAction(nameof(Profile));
        }
        [HttpGet]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> EditAddress(int? id)
        {
            if (id == null) return BadRequest();

            AppUser appUser = await _userManager.Users
               .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.NormalizedUserName == User.Identity.Name.ToUpperInvariant());

            Address oldAddress = appUser.Addresses.FirstOrDefault(a => a.Id == id && a.IsDeleted == false);

            if (oldAddress == null)
            {
                return NotFound();
            }

            Address address = new()
            {
                City = oldAddress.City,
                State = oldAddress.State,
                AddressLine = oldAddress.AddressLine,
                PostalCode = oldAddress.PostalCode,
                Country = oldAddress.Country,
                IsMain = oldAddress.IsMain,
                Id = oldAddress.Id
            };
            TempData["Tab"] = "address";
            return RedirectToAction(nameof(Profile), address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> EditAddress(Address address)
        {
            if (address == null || address.Id == null) return NotFound();

            AppUser appUser = await _userManager.Users
               .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.NormalizedUserName == User.Identity.Name.ToUpperInvariant());

            if (appUser == null) { return NotFound(); }
            if (!ModelState.IsValid)

            {
                return View(nameof(Profile), address);
            }
            var dbAddress = appUser.Addresses.FirstOrDefault(a => a.Id == address.Id && a.IsDeleted == false);

            if (dbAddress == null)
            {
                return NotFound();
            }
            bool isMain = address.IsMain;
            if (isMain)
            {
                var otherAdresses = appUser.Addresses.Where(a => a.IsDeleted == false && a.Id != address.Id && a.IsMain);
                foreach (var address1 in otherAdresses)
                {
                    address1.IsMain = false;
                }

            }
            dbAddress.AddressLine = address.AddressLine;
            dbAddress.City = address.City;
            dbAddress.Country = address.Country;
            dbAddress.State = address.State;
            dbAddress.PostalCode = address.PostalCode;
            dbAddress.IsMain = address.IsMain;

            await _userManager.UpdateAsync(appUser);
            await _context.SaveChangesAsync();
			TempData["Tab"] = "address";
			TempData["ToasterMessage"] = "Address updated successfully!";

			return RedirectToAction(nameof(Profile));
        }
    }
}
