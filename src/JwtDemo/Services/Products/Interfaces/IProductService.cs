
using JwtDemo.Dtos.ProductDtos;
using JwtDemo.Utility;

namespace JwtDemo.Services.Products.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResponse<List<CreateProductResponseDto>>> GetAllAsync(int pageNumber, int pageSize);
        Task<ServiceResponse<CreateProductResponseDto>> GetByIdAsync(int id);
        Task<ServiceResponse<CreateProductResponseDto>> CreateAsync(CreateProductRequestDto request);
        Task<ServiceResponse<string>> UpdateAsync(int id, UpdateProductRequestDto request);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<List<CreateProductResponseDto>>> SearchAsync(string query, int pageNumber, int pageSize);
    }
}