using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JwtDemo.Dtos
{
    public class RegisterRequestDto
    {
        [MaxLength(50)]
        public required string FirstName { get; set; }
        [MaxLength(50)]
        public required string LastName { get; set; } 
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;
        [EmailAddress]
        public required string Email { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}