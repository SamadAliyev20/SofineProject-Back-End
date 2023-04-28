using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels;
using SofineProject.ViewModels.AccountViewModels;
using SofineProject.ViewModels.BasketViewModels;
using System.Data;
using System.Net;

namespace SofineProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;
		private readonly SmtpSetting _smtpSetting;
		public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext context, IOptions<SmtpSetting> smtpSetting)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
			_smtpSetting = smtpSetting.Value;
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
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            string url = Url.Action("EmailConfirm", "Account", new { id = appUser.Id, token = token },
                HttpContext.Request.Scheme, HttpContext.Request.Host.ToString());

            string fullpath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Shared", "EmailConfirmPartial.cshtml");
            string templateContent = await System.IO.File.ReadAllTextAsync(fullpath);
            templateContent = templateContent.Replace("{{url}}", url);

            MimeMessage mimeMessage = new();
            mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
            mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
            mimeMessage.Subject = "Email Confirmation";
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
            TempData["ToasterMessage2"] = "Your account has been successfully created, please confirm your account.Check your email.";
            return RedirectToAction("Login","Account");
        }

        [HttpGet]
        public async Task<IActionResult> EmailConfirm(string id, string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }
            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) { return NotFound(); }

            IdentityResult identityResult = await _userManager.ConfirmEmailAsync(appUser, token);

            if (!identityResult.Succeeded)
            {

                return BadRequest();

            }
            TempData["ToasterMessage2"] = "Your account has been successfully Confirmed.";
            await _signInManager.SignInAsync(appUser, false);
            return RedirectToAction("index", "home");

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
            string basket = HttpContext.Request.Cookies["basket"];
            if (string.IsNullOrWhiteSpace(basket))
            {
                if (appUser.Baskets != null && appUser.Baskets.Count() > 0)
                {
                    List<BasketVM> basketVMs = new List<BasketVM>();

                    foreach (Basket basket1 in appUser.Baskets)
                    {
                        BasketVM basketVM = new BasketVM
                        {
                            Id = (int)basket1.ProductId,
                            Count = basket1.Count,
                        };
                        basketVMs.Add(basketVM);
                    }
                    basket = JsonConvert.SerializeObject(basketVMs);
                    HttpContext.Response.Cookies.Append("basket", basket);
                }
            }
            else
            {
                HttpContext.Response.Cookies.Append("basket", "");
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

			if (!ModelState.IsValid)
            {
				TempData["Tab"] = "account";
				return View(profileVM);
            }
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

		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPassword forgotPassword)
		{
			if (!ModelState.IsValid) return View(forgotPassword);

			AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == forgotPassword.Email.Trim().ToUpperInvariant());

			if (appUser == null)
			{
				return RedirectToAction("ForgotPassword", "account");
			}

			string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

			string url = Url.Action("ResetPassword", "Account", new { token, email = forgotPassword.Email },
				HttpContext.Request.Scheme, HttpContext.Request.Host.ToString());

			string fullpath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Shared", "PassReset.cshtml");
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
            return RedirectToAction("index", "Home");



		}

		[HttpGet]
		public IActionResult ResetPassword(string token, string email)
		{
			var model = new ResetPasswordVM { Token = token, Email = email };
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
		{
			if (!ModelState.IsValid)
			{
				return View(resetPasswordVM);
			}
			AppUser appUser = await _userManager.FindByEmailAsync(resetPasswordVM.Email);

			if (appUser == null) { return NotFound(); }

			IdentityResult identityResult = await _userManager.ResetPasswordAsync(appUser, resetPasswordVM.Token, resetPasswordVM.Password);

			if (!identityResult.Succeeded)
			{
				foreach (var error in identityResult.Errors)
				{
					ModelState.TryAddModelError(error.Code, error.Description);
				}
				return View();
			}
            TempData["ToasterMessage"] = "Your password has been successfully changed!";
            return RedirectToAction("login", "account");
		}
	}
}
