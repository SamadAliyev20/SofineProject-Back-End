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
	}
}
