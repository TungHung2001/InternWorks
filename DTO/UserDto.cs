using System.Data;

namespace IdentityTest2.DTO
{
    public class UserDto
    {
        private string refreshToken;

        public string DisplayName { get; set; }
        public string Token { get; set; }
        /*  public string Image { get; set; }*/
        public string Username { get; set; }

        public string RefreshToken { get => refreshToken; set => refreshToken = value; }
    }

}
