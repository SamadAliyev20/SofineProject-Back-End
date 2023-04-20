using System.ComponentModel.DataAnnotations;

namespace SofineProject.ViewModels.AccountViewModels
{
	public class LoginVM
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		public bool RemindMe { get; set; }
	}
}
