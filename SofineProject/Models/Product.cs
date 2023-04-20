using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SofineProject.Models
{
    public class Product :BaseEntity
    {
        [StringLength(255)]
        public string? Title { get; set; }
        [Column(TypeName = "money")]
        public double Price { get; set; }
        [Column(TypeName = "money")]
        public double DiscountedPrice { get; set; }
        public int Count { get; set; }
        [StringLength(1000)]
        public string? ShortDescription { get; set; }
        [StringLength(4000)]
        public string? LongDescription { get; set; }

        [StringLength(255)]
        public string? MainImage { get; set; }
        [StringLength(255)]
        public string? HoverImage { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int ProductTypeId { get; set; }

        public ProductType ProductType { get; set; }          
        public IEnumerable<ProductImage> ProductImages { get; set; }
        [NotMapped]
        public IEnumerable<IFormFile>? Files { get; set; }

        [NotMapped]
        public IFormFile? MainFile { get; set; }

        [NotMapped]
        public IFormFile? HoverFile { get; set; }

		public IEnumerable<Review>? Reviews { get; set; }

	}
}
