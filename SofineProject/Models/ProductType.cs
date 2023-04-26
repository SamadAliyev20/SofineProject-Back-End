namespace SofineProject.Models
{
    public class ProductType :BaseEntity
    {
        public string Name { get; set; }

        public IEnumerable<Product>? Products { get; set;}
    }
}
