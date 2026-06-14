using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using JwtDemo.Models;
using Microsoft.IdentityModel.Tokens;
using JwtDemo.Options;
using JwtDemo.Dtos;
using JwtDemo.Dtos.AuthDtos;
using JwtDemo.DbContext;
using System.Security.Cryptography;
using System.Security;
using Microsoft.EntityFrameworkCore;
using JwtDemo.Services.Caching.Interfaces;




namespace JwtDemo.Services.Auth.Interfaces
{
    public  class TokenService : ITokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IDistributedRedisCacheService _redisCacheService;
        public TokenService(IOptions<JwtOptions> jwtOptions, 
        IDistributedRedisCacheService redisCacheService, AppDbContext context, UserManager<User> userManager)
        {
            
            _jwtOptions = jwtOptions.Value;
            _userManager = userManager;
            _context = context;
            _redisCacheService = redisCacheService;
        }
        public async Task<AuthenticatedUserDto> GenerateTokenPairAsync(User user)
        {
            var response = new AuthenticatedUserDto();

            if(user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var tokenHandler = new JsonWebTokenHandler();

            var JwtId = Guid.NewGuid().ToString();

            var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, JwtId)
            };

            var roles = await _userManager.GetRolesAsync(user);

            if(roles.Any())
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
            var accessToken = tokenHandler.CreateToken(tokenDescriptor);

            
            //Generate refresh Token
             var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshTokenValue = Convert.ToBase64String(randomBytes);

            var cacheKey = $"refresh:{user.Id}";
            
            await _redisCacheService.SetAsync(cacheKey, refreshTokenValue, TimeSpan.FromDays(7));

            return new AuthenticatedUserDto
            {
                UserName = user.UserName ?? string.Empty,
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiry = tokenDescriptor.Expires.GetValueOrDefault()
            };
        }

        public async Task<AuthenticatedUserDto> RefreshAsync(RefreshTokenRequestDto dto)
        {
            if(dto is null || string.IsNullOrEmpty(dto.AccessToken) || string.IsNullOrEmpty(dto.RefreshToken))
            {
                throw new SecurityException("Provide valid data to continue");
            }

            //validate incoming access token
            var tokenHandler = new JsonWebTokenHandler();
            var validTokenParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime =  false,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
                ClockSkew = TimeSpan.Zero
            };

            var validToken = await tokenHandler.ValidateTokenAsync(dto.AccessToken, validTokenParameters);

            if(!validToken.IsValid || validToken.SecurityToken is not JsonWebToken jwtToken)
            {
                throw new SecurityException("Invalid access token");
            }

            var userId =  jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityException("User not found");
            }

            //fetch and verify stored refresh token
            var key = $"refresh:{userId}";

            var storedRefreshToken = await _redisCacheService.GetAsync<string>(key);

            if(storedRefreshToken is null || string.IsNullOrEmpty(storedRefreshToken))
            {
                throw new SecurityException("Refresh token does not exist");
            }

            if(storedRefreshToken != dto.RefreshToken)
            {
                throw new SecurityException("Refresh token mismatch");
            }


            var user = await _context.Users.FindAsync(userId);
            if(user is null)
            {
                throw new SecurityException("User no longer exists");
            }

            //Rotate stored token
            await _redisCacheService.RemoveAsync(key);

            return await GenerateTokenPairAsync(user);
        }
    }
}