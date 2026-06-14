
namespace JwtDemo.Dtos.AuthDtos
{
    public class AssignRolesRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}