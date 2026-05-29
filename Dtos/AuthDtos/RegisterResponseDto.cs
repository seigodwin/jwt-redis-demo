using System.ComponentModel.DataAnnotations;

namespace JwtDemo.Dtos
{
    public class RegisterResponseDto
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(50)]
        public  string UserName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string ConfirmEmailToken { get; set; } = string.Empty;
        
        public List<string> Roles { get; set; } = new List<string>();
    }
}