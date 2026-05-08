namespace ShopApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string PasswordText { get; set; } = "";
        public string FullName { get; set; } = "";
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public List<Order> Orders { get; set; } = new();
    }
}
