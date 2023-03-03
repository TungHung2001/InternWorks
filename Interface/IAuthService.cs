using IdentityTest2.DTO;

namespace IdentityTest2.Interface
{
    public interface IAuthService
    {
        public Task<UserDto> Login(string email, string password);
        public Task<UserDto> Register(UserDto user);
    }
}
