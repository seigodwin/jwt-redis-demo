namespace JwtDemo.Dtos
{
    public class AuthenticatedUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken {get; set;} = string.Empty;
        public DateTime AccessTokenExpiry { get; set;}
    }
}