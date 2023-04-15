using System.ComponentModel.DataAnnotations;

namespace SofineProject.Models
{
    public class Slider : BaseEntity
    {
        [StringLength(255)]
        public string SubTitle { get; set; }
        [StringLength(255)]
        public string MainTitle { get; set; }
        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(255)]
        public string Image { get; set; }
    }
}
