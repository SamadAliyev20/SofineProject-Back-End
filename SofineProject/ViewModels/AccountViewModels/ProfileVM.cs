using SofineProject.Models;
using System.ComponentModel.DataAnnotations;

namespace SofineProject.ViewModels.AccountViewModels
{
    public class ProfileVM
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]

        public string? SurName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string? UserName { get; set; }

        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        public IEnumerable<Address>? Addresses { get; set; }
        public IEnumerable<Order>? Orders { get; set; }
        public AppUser? AppUser { get; set; }
        public Address? EditAddress { get; set; }
    }
}
