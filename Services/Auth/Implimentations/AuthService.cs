
using JwtDemo.DbContext;
using JwtDemo.Dtos;
using JwtDemo.Dtos.AuthDtos;
using JwtDemo.Models;
using JwtDemo.Services.Auth.Interfaces;
using JwtDemo.Services.Caching.Interfaces;
using JwtDemo.Utility;
using Microsoft.AspNetCore.Identity;

namespace JwtDemo.Services.Auth.Implimentations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRedisCacheService _redisCacheService;
        public AuthService(AppDbContext context, ITokenService tokenService, 
        UserManager<User> userManager, IRedisCacheService redisCacheService, RoleManager<IdentityRole> roleManager)
        {
            _redisCacheService = redisCacheService;
            _context = context;
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ServiceResponse<string>> AssignRolesAsync(AssignRolesRequestDto request)
        {
            var response = new ServiceResponse<string>();

            if(request is null)
            {
                response.Success = false;
                response.Message = "Assign roles data is null";
                return response;
            }
            var user = await _userManager.FindByIdAsync(request.UserId);

            if(user is null)
            {
                response.Success = false;
                response.Message = "User does not exist";
                return response;
            }

            if(! await _userManager.IsEmailConfirmedAsync(user))
            {
                response.Success = false;
                response.Message = "Please verify user's email before assigning roles";
                return response;
            }

            if(request.Roles is not null && request.Roles.Count > 0)
            {
                var cleanRoles = request.Roles
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .ToList();

                if (!cleanRoles.Any())
                {
                    response.Success = false;
                    response.Message = "No valid roles provided.";
                    return response;
                }

                try
                {
                    //Check for each role if it exists, if not create it

                    foreach(var role in cleanRoles)
                    {
                        if(!await _roleManager.RoleExistsAsync(role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(role));
                            await _context.SaveChangesAsync();
                        }

                    }

                    var rolesToAdd = cleanRoles.Except(await _userManager.GetRolesAsync(user)).ToList();
                    
                    var roleResults = await _userManager.AddToRolesAsync(user, rolesToAdd);

                    if (!roleResults.Succeeded)
                    {
                        response.Success = false;
                        response.Message =
                            "Failed to assign roles: " +
                            string.Join(", ", roleResults.Errors.Select(e => e.Description));
                        return response;
                    }

                    response.Message = "Roles assigned successfully";
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    response.Message =
                        "Role assignment failed: " + ex.Message;

                    return response;
                }
            }
            else
            {
                response.Success = false;
                response.Message = "No roles provided to assign.";
               
            }
             return response;
        }

        public async Task<ServiceResponse<string>> ConfirmEmailAsync(ConfirmEmailRequestDto request)
        {
            var response = new ServiceResponse<string>();
            if(request is null)            {
                response.Success = false;
                response.Message = "Confirm email data is null";
                return response;
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if(user is null)
            {
                response.Success = false;
                response.Message = "User does not exist";
                return response;
            }

            var emailConfirmed = await _userManager.ConfirmEmailAsync(user, request.Token);

            if (!emailConfirmed.Succeeded)
            {
                response.Success = false;
                response.Message = "Invalid token or expired token";
                return response;
            }

            response.Message = "Email confirmed successfully";
            return response;
        }

        public async Task<ServiceResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var response = new ServiceResponse<string>();
            if(request is null)
            {
                response.Success = false;
                response.Message = "Forgot password data is null";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if(user is null)
            {
                response.Success = false;
                response.Message = "If an account with that email exists, a password reset token has been sent to the email address.";
                return response;
            }

            var key = $"forgot-password:{request.Email}";

            var blocked = await _redisCacheService.IsRateLimited(key,3,TimeSpan.FromMinutes(15));
            if (blocked)
            {
                response.Success = false;
                response.Message = "Too many attempts. Try again in 15 minutes";
                return response;
            }

            if(! await _userManager.IsEmailConfirmedAsync(user))
            {
                response.Success = false;
                response.Message = "Please verify your email before resetting password";
                return response;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            response.Data = token;
            return response;
        }

        public async Task<ServiceResponse<AuthenticatedUserDto>> LoginAsync(LoginRequestDto request)
        {
            var response = new ServiceResponse<AuthenticatedUserDto>();
            if(request is null)
            {
                response.Success = false;
                response.Message = "Login data is null";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if(user is null)
            {
                response.Success = false;
                response.Message = "Invalid email";
                return response;
            }

            var key = $"login:{request.Email}";
                 
            var blocked = await _redisCacheService.IsRateLimited(key,5,TimeSpan.FromMinutes(1));
            if (blocked)
            {
                response.Success = false;
                response.Message = "Too many failed attempts. Try again in a minutes";
                return response;
            }

            if(!await _userManager.IsEmailConfirmedAsync(user))
            {
                response.Success = false;
                response.Message = "Please verify your email before logging in";
                return response;
            }

            if(! await _userManager.CheckPasswordAsync(user, request.Password))
            {
                response.Success = false;
                response.Message = "Wrong password";
                return response;
            }

            var tokens = await _tokenService.GenerateTokenPairAsync(user);

            if(tokens is null)
            {
                response.Success = false;
                response.Message = "Failed to login";
                return response;
            }

            response.Message = "Login successful";

            response.Data = new AuthenticatedUserDto
            {
                UserName = tokens.UserName,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                AccessTokenExpiry = tokens.AccessTokenExpiry
            };
            
            return response;
        }

        public async Task<ServiceResponse<AuthenticatedUserDto>> RefreshAsync(RefreshTokenRequestDto request)
        {
            var response = new ServiceResponse<AuthenticatedUserDto>();
            if(request is null  || string.IsNullOrEmpty(request.RefreshToken))
            {
                response.Success = false;
                response.Message = "Provide valid data to continue";
                return response;
            }

            var results = await _tokenService.RefreshAsync(request);

            if(results is null)
            {
                response.Success = false;
                response.Message = "Failed to refresh token";
                return response;
            }
            
            response.Data = results;
            response.Message = "Token refresh successfull";
            
            return response;
        }

        public async Task<ServiceResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            var response = new ServiceResponse<RegisterResponseDto>();

            if(request is null)
            {
                response.Success = false;
                response.Message = "Registeration data is null";
                return response;
            }

            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if(userExists is not null)
            {
                response.Success = false;
                response.Message = "User already exists";
                return response;
            }

            if(request.Password != request.ConfirmPassword)
            {
                response.Success = false;
                response.Message = "Password and Confirm Password do not match";
                return response;
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,    
                Email = request.Email,
                UserName = request.UserName ?? request.Email
            };

            try
            {
                var results = await _userManager.CreateAsync(user, request.Password);
                await _context.SaveChangesAsync();

                if (results.Succeeded)
                {
                    response.Message = "User created successfully";
                    response.Data = new RegisterResponseDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        UserName = user.UserName,
                        ConfirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user) ?? "Failed to generate email confirmation token",
                    };    

                    if (request.Roles is not null && request.Roles.Count > 0)
                    
                    {   
                        var cleanRoles = request.Roles
                            .Where(r => !string.IsNullOrWhiteSpace(r))
                            .ToList();

                        if (!cleanRoles.Any())
                        {
                            response.Success = true;
                            response.Message = "User created but no valid roles were provided.";
                            return response;
                        }

                        try
                        {
                            var roleResults = await _userManager.AddToRolesAsync(user, cleanRoles);

                            if (!roleResults.Succeeded)
                            {
                                response.Success = false;
                                response.Message =
                                    "User created but failed to assign roles: " +
                                    string.Join(", ", roleResults.Errors.Select(e => e.Description));
                                return response;
                            }

                            response.Message = "User created and roles assigned successfully";
                            response.Data.Roles = await _userManager.GetRolesAsync(user) as List<string> ?? new List<string>();
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            response.Message =
                                "User created but role assignment failed: " + ex.Message;

                            return response;
                        }
                    }

    
                }
                 else
                {
                    response.Success = false;
                    response.Message = string.Join(", ", results.Errors.Select(e => e.Description));
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred while registering the user: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var response = new ServiceResponse<string>();
            if(request is null)
            {
                response.Success = false;
                response.Message = "Reset password data is null";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if(user is null)
            {
                response.Success = false;
                response.Message = "User does not exist";
                return response;
            }

            var key = $"reset-password:{request.Email}";

            var blocked = await _redisCacheService.IsRateLimited(key,3,TimeSpan.FromMinutes(15));
            if (blocked)
            {
                response.Success = false;
                response.Message = "Too many failed attempts. Try again in 15 minutes";
                return response;
            }

            if(request.NewPassword != request.ConfirmPassword)
            {
                response.Success = false;
                response.Message = "Password and Confirm Password do not match";
                return response;
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if(!result.Succeeded)
            {
                response.Success = false;
                response.Message = "Invalid or expired token";
                return response;
            }

            response.Message = "Password reset successful";
            return response;
        } 
    }
}