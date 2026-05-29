using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using JwtDemo.Models;
using Microsoft.IdentityModel.Tokens;
using JwtDemo.Options;


namespace JwtDemo.Services.Auth.Interfaces
{
    public  class TokenGenerator : ITokenGenerator
    {
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<User> _userManager;
        public TokenGenerator(IOptions<JwtOptions> jwtOptions, UserManager<User> userManager)
        {
            _jwtOptions = jwtOptions.Value;
            _userManager = userManager;
        }
        public async Task<string> GenerateTokenAsync(User user)
        {
            if(user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var tokenHandler = new JsonWebTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!)
            };

            var roles = await _userManager.GetRolesAsync(user);

            if(user is not null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.CreateToken(tokenDescriptor);
        }

    }
}