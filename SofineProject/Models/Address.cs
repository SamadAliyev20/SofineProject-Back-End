using System.ComponentModel.DataAnnotations;

namespace SofineProject.Models
{
    public class Address : BaseEntity
    {
        [StringLength(100)]
        public string? Country { get; set; }
        [StringLength(100)]
        public string? State { get; set; }
        [StringLength(100)]
        public string? City { get; set; }
        [StringLength(100)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? AddressLine { get; set; }

        public bool IsMain { get; set; }
        public string? AppUserId { get; set; }

        public AppUser? AppUser { get; set; }
    }
}
