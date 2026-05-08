using Microsoft.EntityFrameworkCore;

namespace ShopApp.Models
{
    public class ShopContext : DbContext
    {
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
        public DbSet<PickupPoint> PickupPoints => Set<PickupPoint>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=shop_db;Username=postgres;Password=postgres");
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            // roles
            b.Entity<Role>(e =>
            {
                e.ToTable("roles");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Code).HasColumnName("code");
                e.Property(x => x.Name).HasColumnName("name");
            });

            // users
            b.Entity<User>(e =>
            {
                e.ToTable("users");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Login).HasColumnName("login");
                e.Property(x => x.PasswordText).HasColumnName("password_text");
                e.Property(x => x.FullName).HasColumnName("full_name");
                e.Property(x => x.RoleId).HasColumnName("role_id");
                e.HasOne(x => x.Role).WithMany(r => r.Users).HasForeignKey(x => x.RoleId);
            });

            // categories
            b.Entity<Category>(e =>
            {
                e.ToTable("categories");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("name");
            });

            // manufacturers
            b.Entity<Manufacturer>(e =>
            {
                e.ToTable("manufacturers");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("name");
            });

            // suppliers
            b.Entity<Supplier>(e =>
            {
                e.ToTable("suppliers");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("name");
            });

            // order_statuses
            b.Entity<OrderStatus>(e =>
            {
                e.ToTable("order_statuses");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Code).HasColumnName("code");
                e.Property(x => x.Name).HasColumnName("name");
            });

            // pickup_points
            b.Entity<PickupPoint>(e =>
            {
                e.ToTable("pickup_points");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Address).HasColumnName("address");
            });

            // products
            b.Entity<Product>(e =>
            {
                e.ToTable("products");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Article).HasColumnName("article");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.Unit).HasColumnName("unit");
                e.Property(x => x.Price).HasColumnName("price");
                e.Property(x => x.SupplierId).HasColumnName("supplier_id");
                e.Property(x => x.ManufacturerId).HasColumnName("manufacturer_id");
                e.Property(x => x.CategoryId).HasColumnName("category_id");
                e.Property(x => x.DiscountPct).HasColumnName("discount_pct");
                e.Property(x => x.StockQty).HasColumnName("stock_qty");
                e.Property(x => x.Description).HasColumnName("description");
                e.Property(x => x.Photo).HasColumnName("photo");
                e.Ignore(x => x.FinalPrice);
                e.HasOne(x => x.Supplier).WithMany(s => s.Products).HasForeignKey(x => x.SupplierId);
                e.HasOne(x => x.Manufacturer).WithMany(m => m.Products).HasForeignKey(x => x.ManufacturerId);
                e.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId);
            });

            // orders
            b.Entity<Order>(e =>
            {
                e.ToTable("orders");
                e.HasKey(x => x.OrderNum);
                e.Property(x => x.OrderNum).HasColumnName("order_num");
                e.Property(x => x.OrderDate).HasColumnName("order_date");
                e.Property(x => x.DeliveryDate).HasColumnName("delivery_date");
                e.Property(x => x.PickupPointId).HasColumnName("pickup_point_id");
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.PickupCode).HasColumnName("pickup_code");
                e.Property(x => x.StatusId).HasColumnName("status_id");
                e.HasOne(x => x.PickupPoint).WithMany(p => p.Orders).HasForeignKey(x => x.PickupPointId);
                e.HasOne(x => x.User).WithMany(u => u.Orders).HasForeignKey(x => x.UserId);
                e.HasOne(x => x.Status).WithMany(s => s.Orders).HasForeignKey(x => x.StatusId);
            });

            // order_items
            b.Entity<OrderItem>(e =>
            {
                e.ToTable("order_items");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.OrderNum).HasColumnName("order_num");
                e.Property(x => x.ProductId).HasColumnName("product_id");
                e.Property(x => x.Quantity).HasColumnName("quantity");
                e.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderNum);
                e.HasOne(x => x.Product).WithMany(p => p.OrderItems).HasForeignKey(x => x.ProductId);
            });
        }
    }
}
