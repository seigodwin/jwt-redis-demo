
using JwtDemo.Dtos;
using JwtDemo.Dtos.AuthDtos;
using JwtDemo.Models;

namespace JwtDemo.Services.Auth.Interfaces
{
    public interface ITokenService
    {
        Task<AuthenticatedUserDto> GenerateTokenPairAsync(User user);
        Task<AuthenticatedUserDto> RefreshAsync(RefreshTokenRequestDto dto);
    }
}