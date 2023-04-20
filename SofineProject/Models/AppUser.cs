using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SofineProject.Models
{
    public class AppUser :IdentityUser
    {
        [StringLength(100)]
        public string? Name { get; set; }
        [StringLength(100)]
        public string? SurName { get; set; }
    }
}
