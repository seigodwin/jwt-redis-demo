
using JwtDemo.DbContext;
using JwtDemo.Dtos.UserDtos;
using JwtDemo.Models;
using JwtDemo.Services.Users.Interfaces;
using JwtDemo.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace JwtDemo.Services.Users.Implimentations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        public UserService(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<ServiceResponse<string>> DeleteAsync(string id)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users.FindAsync(id);

            if(user is null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            try
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                response.Message = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting user: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<GetUserResponseDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var response = new  ServiceResponse<List<GetUserResponseDto>>();
            var users = await _context.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

              if(users.Any())
              {
                response.Data = new List<GetUserResponseDto>();

                foreach (var user in users)
                {
                    response.Data.Add(new GetUserResponseDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName!,
                        Email = user.Email!,
                        Roles = (await _userManager.GetRolesAsync(user)).ToList()
                    });
                }

                response?.Message = "Users retrieved successfully";
              }
              else
              {
                  response.Success = false;
                  response.Message = "No users found.";
              }
              return response!;
        }

        public async Task<ServiceResponse<GetUserResponseDto>> GetByIdAsync(string id)
        {
            var response = new ServiceResponse<GetUserResponseDto>();
            var user = await _context.Users.FindAsync(id);

            if(user is null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            response.Data = new GetUserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            };

            response.Message = "User retrieved successfully.";
            return response;
        }

        public async Task<ServiceResponse<string>> UpdateAsync(string id, UpdateUserRequestDto request)
        {
            var response = new ServiceResponse<string>();
            if(request is null)
            {
                response.Success = false;
                response.Message = "Invalid user data.";
                return response;
            }

            var user = await _context.Users.FindAsync(id);
            if(user is null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            // Update user properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UserName = request.Username;
            user.Email = request.Email;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                response.Message = "User updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating user: {ex.Message}";
            }

            return response;    
        }
    }

}