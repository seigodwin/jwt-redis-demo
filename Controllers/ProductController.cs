

using JwtDemo.Dtos.ProductDtos;
using JwtDemo.Services.Products.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtDemo.Controllers
{
    [ApiController]
    [Route("api/v1/product")]
    [Authorize(Roles = "Admin,Supervisor")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromRoute]int pageNumber = 1,[FromRoute] int pageSize = 10)
        {
            var response = await _productService.GetAllAsync(pageNumber, pageSize);
            
            return response.Success ? Ok(response) : NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var response = await _productService.GetByIdAsync(id);
            return response.Success ? Ok(response) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create ([FromBody] CreateProductRequestDto request)
        {
            if(request is not null && ModelState.IsValid)
            {
                var response = await _productService.CreateAsync(request);
                return response.Success ? CreatedAtAction(nameof(Get), new { id = response?.Data?.Id }, response) 
                : BadRequest(response);
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProductRequestDto request)
        {
            if (request is not null && ModelState.IsValid)
            {
                var response = await _productService.UpdateAsync(id, request);
                return response.Success ? NoContent() : BadRequest(response);
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var response = await _productService.DeleteAsync(id);
            return response.Success ? NoContent() : BadRequest(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search ([FromQuery] string query, int pageNumber = 1, int pageSize = 10)
        {
            var response = await _productService.SearchAsync(query, pageNumber, pageSize);
            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}