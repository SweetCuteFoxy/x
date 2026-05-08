namespace ShopApp.Models
{
    public class Order
    {
        public int OrderNum { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int PickupPointId { get; set; }
        public PickupPoint? PickupPoint { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string PickupCode { get; set; } = "";
        public int StatusId { get; set; }
        public OrderStatus? Status { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}
