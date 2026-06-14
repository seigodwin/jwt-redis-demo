
using System.ComponentModel.DataAnnotations;

namespace JwtDemo.Dtos
{
    public class LoginRequestDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}