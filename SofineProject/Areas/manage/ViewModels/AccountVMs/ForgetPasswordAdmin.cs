using System.ComponentModel.DataAnnotations;

namespace SofineProject.Areas.manage.ViewModels.AccountVMs
{
    public class ForgetPasswordAdmin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
