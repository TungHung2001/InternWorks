using IdentityTest2.Entities;

namespace IdentityTest2.Interface
{
    public interface IRefreshToken
    {
        Task<string> GenerateToken(string username);
    }
}
