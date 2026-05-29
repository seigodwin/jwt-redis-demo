
using JwtDemo.Dtos;
using JwtDemo.Dtos.AuthDtos;
using JwtDemo.Utility;

namespace JwtDemo.Services.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ServiceResponse<AuthenticatedUserDto>> LoginAsync(LoginRequestDto request);
        Task<ServiceResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<ServiceResponse<string>> ConfirmEmailAsync(ConfirmEmailRequestDto request);
        Task<ServiceResponse<string>> AssignRolesAsync(AssignRolesRequestDto request);
    }
}