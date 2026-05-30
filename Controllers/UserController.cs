using JwtDemo.DbContext;
using JwtDemo.Dtos.UserDtos;
using JwtDemo.Services.Users.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JWTDEMO.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
        {
            var results = await _userService.GetAllAsync(pageNumber, pageSize);
            return results.Success ? Ok(results) : NotFound(results);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _userService.GetByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequestDto dto)
        {
            if(dto is not null && ModelState.IsValid)
            {
                var result = await _userService.UpdateAsync(id, dto);
                return result.Success ? NoContent() : NotFound(result);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteAsync(id);
            return result.Success ? NoContent() : BadRequest(result);
        }
    }
}   