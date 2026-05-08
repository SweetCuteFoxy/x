namespace ShopApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderNum { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
    }
}
