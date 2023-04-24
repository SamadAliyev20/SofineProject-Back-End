using System.ComponentModel.DataAnnotations;

namespace SofineProject.ViewModels.AccountViewModels
{
	public class ForgotPassword
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
