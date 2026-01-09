using project.Models.DTO;
using project.Models.Entities;

namespace project.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto> UpdateAsync(int id, UpdateUserDto dto, string? currentUserRole);
    Task<bool> DeleteAsync(int id, string? currentUserRole);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> VerifyPasswordAsync(User user, string password);
}
