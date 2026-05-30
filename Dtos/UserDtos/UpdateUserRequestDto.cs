
namespace JwtDemo.Dtos.UserDtos
{
    public class UpdateUserRequestDto
    {
        
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string  Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
    }
}