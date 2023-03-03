using System.ComponentModel.DataAnnotations;

namespace IdentityTest2.DTO
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
      
        public string Password { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string Username { get; set; }

        /*[Required]
        public string Role { get; set; }*/
    }
}
