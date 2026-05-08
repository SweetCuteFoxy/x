namespace ShopApp.Models
{
    public class PickupPoint
    {
        public int Id { get; set; }
        public string Address { get; set; } = "";
        public List<Order> Orders { get; set; } = new();
    }
}
