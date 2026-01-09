using project.Models.DTO;
using project.Models.Entities;
using project.Repositories.Interfaces;
using project.Services.Interfaces;

namespace project.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        _logger.LogInformation("Getting all users");
        var users = await _repository.GetAllAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting user with ID {UserId}", id);
        var user = await _repository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        _logger.LogInformation("Creating new user: {Username}", dto.Username);
        
        var existingUser = await _repository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        existingUser = await _repository.GetByUsernameAsync(dto.Username);
        if (existingUser != null)
            throw new InvalidOperationException("User with this username already exists");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(user);
        _logger.LogInformation("User created with ID {UserId}", created.Id);
        return MapToDto(created);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto dto, string? currentUserRole)
    {
        _logger.LogInformation("Updating user {UserId} by {Role}", id, currentUserRole);
        
        if (dto.Role != null && currentUserRole != "Admin")
            throw new UnauthorizedAccessException("Only Admin can change user roles");

        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (dto.Username != null) user.Username = dto.Username;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.Role != null && currentUserRole == "Admin") user.Role = dto.Role;

        var updated = await _repository.UpdateAsync(user);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id, string? currentUserRole)
    {
        _logger.LogInformation("Deleting user {UserId} by {Role}", id, currentUserRole);
        
        if (currentUserRole != "Admin")
            throw new UnauthorizedAccessException("Only Admin can delete users");

        return await _repository.DeleteAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _repository.GetByUsernameAsync(username);
    }

    public Task<bool> VerifyPasswordAsync(User user, string password)
    {
        return Task.FromResult(PasswordHasher.VerifyPassword(password, user.PasswordHash));
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
