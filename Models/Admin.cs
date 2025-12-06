namespace KycApi.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = ""; // stored hash
        public string Role { get; set; } = "Admin";
    }
}
