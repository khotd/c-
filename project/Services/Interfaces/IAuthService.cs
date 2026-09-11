using project.Models.DTO;
using project.Models.Entities;

namespace project.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<string> GenerateJwtTokenAsync(User user);
}
