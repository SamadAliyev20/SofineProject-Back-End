using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofineProject.Areas.manage.ViewModels.UserVMs;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels;

namespace SofineProject.Areas.manage.Controllers
{
	[Area("manage")]
	public class UserController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly AppDbContext _context;
		private readonly RoleManager<IdentityRole> _roleManager;
		public UserController(UserManager<AppUser> userManager, AppDbContext context, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_context = context;
			_roleManager = roleManager;

		}
		public async Task<IActionResult> Index(int pageIndex = 1)
		{
			List<UserVM> query = await _userManager.Users
				.Where(u => u.UserName != User.Identity.Name)
				.Select(x => new UserVM
				{
					Id = x.Id,
					Email = x.Email,
					Name = x.Name,
					SurName = x.SurName,
					UserName = x.UserName


				})
				.ToListAsync();
			foreach (var item in query)
			{
				string roleId = _context.UserRoles.FirstOrDefault(u => u.UserId == item.Id).RoleId;
				string roleName = _context.Roles.FirstOrDefault(r => r.Id == roleId).Name;
				item.RoleName = roleName;
			}


			return View(PageNatedList<UserVM>.Create(query.AsQueryable(), pageIndex, 5));
		}

        [HttpGet]
        public async Task<IActionResult> ChangeRole(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);
            if (appUser == null) return NotFound();

            string roleId = _context.UserRoles.FirstOrDefault(u => u.UserId == appUser.Id).RoleId;

            UserChangeRoleVM userChangeRoleVM = new()
            {

                UserId = appUser.Id,
                RoleId = roleId
            };
            ViewBag.Role = await _roleManager.Roles.Where(r => r.Name != "SuperAdmin").ToListAsync();
            return View(userChangeRoleVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(UserChangeRoleVM userChangeRoleVM)
        {
            ViewBag.Role = await _roleManager.Roles.Where(r => r.Name != "SuperAdmin").ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(userChangeRoleVM);
            }

            if (string.IsNullOrWhiteSpace(userChangeRoleVM.UserId)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(userChangeRoleVM.UserId);
            if (appUser == null) return NotFound();
            string roleId = _context.UserRoles.FirstOrDefault(u => u.UserId == userChangeRoleVM.UserId).RoleId;
            string roleName = _context.Roles.FirstOrDefault(r => r.Id == roleId).Name;
            string newRoleName = _roleManager.Roles.FirstOrDefault(r => r.Name != "SuperAdmin" && r.Id == userChangeRoleVM.RoleId).Name;
            await _userManager.RemoveFromRoleAsync(appUser, roleName);
            await _userManager.AddToRoleAsync(appUser, newRoleName);
            TempData["ToasterMessage4"] = "Role Uğurla dəyişdirildi!";
            return RedirectToAction(nameof(Index));
        }
    }
}
