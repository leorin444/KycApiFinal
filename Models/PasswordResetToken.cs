namespace KycApi.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsUsed { get; set; } = false;

        // Link to user
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
