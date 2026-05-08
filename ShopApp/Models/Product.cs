namespace ShopApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Article { get; set; } = "";
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public int ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int DiscountPct { get; set; }
        public int StockQty { get; set; }
        public string? Description { get; set; }
        public string? Photo { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();

        // Цена со скидкой (для UI)
        public decimal FinalPrice => Math.Round(Price * (100 - DiscountPct) / 100m, 2);
    }
}
