namespace AuthService.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public DateTime ExpiryDate { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsRevoked { get; set; }

        public bool IsActive => !IsRevoked && ExpiryDate > DateTime.UtcNow;
    }
}
