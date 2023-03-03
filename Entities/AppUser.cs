using Microsoft.AspNetCore.Identity;

namespace IdentityTest2.Entities
{
    public class AppUser : IdentityUser
    {
            public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
