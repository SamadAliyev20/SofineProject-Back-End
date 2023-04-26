using System.ComponentModel.DataAnnotations;

namespace SofineProject.Areas.manage.ViewModels.UserVMs
{
	public class UserChangeRoleVM
	{
		[Required]
		public string UserId { get; set; }
		[Required]
		public string RoleId { get; set; }
	}
}
