
using JwtDemo.Dtos.UserDtos;
using JwtDemo.Utility;

namespace JwtDemo.Services.Users.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<List<GetUserResponseDto>>> GetAllAsync(int pageNumber, int pageSize);
        Task<ServiceResponse<GetUserResponseDto>> GetByIdAsync(string id);
        Task<ServiceResponse<string>> UpdateAsync(string id, UpdateUserRequestDto request);
        Task<ServiceResponse<string>> DeleteAsync(string id);
    }
}