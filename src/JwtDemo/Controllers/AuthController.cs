

using JwtDemo.Dtos;
using JwtDemo.Dtos.AuthDtos;
using JwtDemo.Services.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace JwtDemo.Controllers
{
    [ApiController]
    [Route("api/v1/auth/user")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if(request is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authService.RegisterAsync(request);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
                if(request is null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

            var result = await _authService.LoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if(request is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }   
            var result = await _authService.ForgotPasswordAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [EnableRateLimiting("resetPolicy")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if(request is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto request)
        {
            if(request is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ConfirmEmailAsync(request);
            return result.Success ? NoContent() : BadRequest(result);
        }

        [HttpPost("assign-roles")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesRequestDto request)
        {
            if(request is null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.AssignRolesAsync(request);
            return result.Success ? NoContent() : BadRequest(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh (RefreshTokenRequestDto dto)
        {
            if(dto is not null && ModelState.IsValid)
            {
               var response = await _authService.RefreshAsync(dto);
               return response.Success ? Ok(response) : BadRequest(response);
            }

            return BadRequest(ModelState);
        }

    }
}