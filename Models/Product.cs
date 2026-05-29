using System.ComponentModel.DataAnnotations;

namespace JwtDemo.Models
{
    public class Product
    {
        [Key]
        public int Id {get;set ;}
        [MaxLength(100)]
        public required string Name {get;set;}
        public string? Description {get;set;}
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price {get;set;}
    }
}