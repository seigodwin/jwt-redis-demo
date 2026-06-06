
using JwtDemo.DbContext;
using JwtDemo.Dtos.ProductDtos;
using JwtDemo.Services.Products.Interfaces;
using JwtDemo.Utility;
using JwtDemo.Models;
using Microsoft.EntityFrameworkCore;
using JwtDemo.Services.Caching.Interfaces;

namespace JwtDemo.Services.Products.Implimentaions
{
    public class ProductService : IProductService
    {
            private readonly AppDbContext _context;
            private readonly IRedisCacheService _cacheService;
        public ProductService(AppDbContext context, IRedisCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }
        public async Task<ServiceResponse<CreateProductResponseDto>> CreateAsync(CreateProductRequestDto request)
        {
            var response = new ServiceResponse<CreateProductResponseDto>();
            if(request is null)
            {
                response.Success = false;
                response.Message = "Invalid product data.";
                return response;
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };

            try
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                await _cacheService.SetAsync($"product:{product.Id}", product, TimeSpan.FromMinutes(10));
                
                response.Data = new CreateProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price
                };
                response.Message = "Product created successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating product: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                response.Success = false;
                response.Message = "Product not found.";
                return response;
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                await _cacheService.RemoveAsync($"product:{id}");
                response.Message = "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting product: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<CreateProductResponseDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var response = new ServiceResponse<List<CreateProductResponseDto>>();
            var cacheKey = $"products:page:{pageNumber}:size:{pageSize}";
            var cachedProducts = await _cacheService.GetAsync<List<CreateProductResponseDto>>(cacheKey);
            
            if (cachedProducts is not null)
            {
                response.Data = cachedProducts;
                response.Message = "Products retrieved from cache successfully.";
                return response;
            }
            try
            {
                if (_context.Products.Any())
                {
                     var products = _context.Products
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new CreateProductResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price
                    })
                    .ToList();

                    response.Data = products;
                    response.Message = "Products retrieved from databse successfully.";
                    await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5));
                }
                else
                {
                    response.Message = "No products found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving products: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<CreateProductResponseDto>> GetByIdAsync(int id)
        {
            var response = new ServiceResponse<CreateProductResponseDto>();

            var cacheKey = $"product:{id}";
            var cachedProduct = await _cacheService.GetAsync<CreateProductResponseDto>(cacheKey);

            if (cachedProduct is not null)
            {
                response.Data = cachedProduct;
                response.Message = "Product retrieved from cache successfully.";
                return response;
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                response.Success = false;
                response.Message = "Product not found.";
                return response;
            }

            response.Data = new CreateProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };
            await _cacheService.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(10));
            response.Message = "Product retrieved successfully.";

            return response;
        }

        public async Task<ServiceResponse<List<CreateProductResponseDto>>> SearchAsync(string query, int pageNumber, int pageSize)
        {
            var response = new ServiceResponse<List<CreateProductResponseDto>>();

            var cacheKey = $"products:search:{query}:page:{pageNumber}:size:{pageSize}";
            var cachedProducts = await _cacheService.GetAsync<List<CreateProductResponseDto>>(cacheKey);
            if (cachedProducts is not null)
            {
                response.Data = cachedProducts;
                response.Message = "Search results retrieved from cache successfully.";
                return response;
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                    var q = query.ToLower();

                    var products = await _context.Products
                        .Where(p => p.Name.ToLower().Contains(q)
                        || (p.Description ?? "").ToLower().Contains(q))
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (products.Any())
                    {
                        response.Data = products.Select(p => new CreateProductResponseDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Price = p.Price
                        }).ToList();

                        response.Message = "Products retrieved successfully.";
                        await _cacheService.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(3));
                    }

                    else
                    {
                        response.Success = false;
                        response.Message = "No products found";
                    }
            }
            else
            { 
                    response.Success = false;
                    response.Message = "Search term is empty.";
            }
            
            return response;
        }

        public async Task<ServiceResponse<string>> UpdateAsync(int id, UpdateProductRequestDto request)
        {
            var response = new ServiceResponse<string>();
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                response.Success = false;
                response.Message = "Product not found.";
                return response;
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;

            try
            {
                await _context.SaveChangesAsync();
                await _cacheService.RemoveAsync($"product:{id}");
                response.Message = "Product updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating product: {ex.Message}";
            }

            return response;
        }
    }
}