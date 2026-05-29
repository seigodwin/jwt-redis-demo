
using System.ComponentModel.DataAnnotations;

namespace JwtDemo.Dtos.ProductDtos
{
    public class CreateProductResponseDto
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public required decimal Price { get; set; }
    }
}