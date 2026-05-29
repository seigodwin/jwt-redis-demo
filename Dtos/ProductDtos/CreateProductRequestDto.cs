
using System.ComponentModel.DataAnnotations;

namespace JwtDemo.Dtos.ProductDtos
{
    public class CreateProductRequestDto
    {
        [MaxLength(100)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
    }
}