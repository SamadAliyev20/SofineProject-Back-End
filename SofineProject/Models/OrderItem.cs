namespace SofineProject.Models
{
    public class OrderItem : BaseEntity
    {
        public int? Count { get; set; }
        public double? Price { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public int? OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
