
using System.ComponentModel.DataAnnotations;

namespace JwtDemo.Dtos.AuthDtos
{
    public class ForgotPasswordRequestDto
    {
        [EmailAddress]
        public required string Email { get; set; } 
    }
}