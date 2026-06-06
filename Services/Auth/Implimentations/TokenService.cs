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




namespace JwtDemo.Services.Auth.Interfaces
{
    public  class TokenService : ITokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        public TokenService(IOptions<JwtOptions> jwtOptions, AppDbContext context, UserManager<User> userManager)
        {
            _jwtOptions = jwtOptions.Value;
            _userManager = userManager;
            _context = context;
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
                new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
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

    
            var refreshToken = new RefreshToken
            {
                JwtId = JwtId,
                UserId = user.Id,
                Token = refreshTokenValue,
                DateAdded = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

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
            if(!validToken.IsValid)
            {
                throw new SecurityException("Invalid access token");
            }

            var JwtId = validToken.SecurityToken.Id;
            var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.JwtId == JwtId);
            if(storedRefreshToken is null)
            {
                throw new SecurityException("Refresh token does not exist");
            }

            if(storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked || storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new SecurityException("Invalid refresh token");
            }


            var user = await _context.Users.FindAsync(storedRefreshToken.UserId);
            if(user is null)
            {
                throw new SecurityException("User no longer exists");
            }

            //Rotate stored token
            storedRefreshToken.IsUsed = true;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();

            return await GenerateTokenPairAsync(user);
        }
    }
}