namespace JwtDemo.Dtos
{
    public class AuthenticatedUserDto
    {
        public required string UserName { get; set; }
        public required string Token { get; set; }
    }
}