
using JwtDemo.Models;

namespace JwtDemo.Services.Auth.Interfaces
{
    public interface ITokenGenerator
    {
        Task<string> GenerateTokenAsync(User user);
    }
}