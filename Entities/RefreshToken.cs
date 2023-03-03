namespace IdentityTest2.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public AppUser AppUser { get; set; }
        public string Token { get; set; }   
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(7);

        public bool IsExpired => DateTime.UtcNow >= ExpirationDate;

        public DateTime Revoked { get; set; }

        public bool IsActive => Revoked == null && !IsExpired;

    }
}
