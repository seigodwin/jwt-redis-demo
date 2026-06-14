
using System.ComponentModel.DataAnnotations;

namespace JwtDemo.Dtos.AuthDtos
{
    public class ResetPasswordRequestDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}