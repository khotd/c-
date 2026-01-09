using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models.Entities;
using project.Repositories;
using Xunit;

namespace project.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
        
        SeedData();
    }

    private void SeedData()
    {
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenExists()
    {
        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsUser_WhenExists()
    {
        var result = await _repository.GetByUsernameAsync("testuser");

        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenExists()
    {
        var result = await _repository.GetByEmailAsync("test@example.com");

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task AddAsync_CreatesUser()
    {
        var user = new User
        {
            Username = "newuser",
            Email = "new@example.com",
            PasswordHash = "hashedpassword",
            Role = "User"
        };

        var result = await _repository.AddAsync(user);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("newuser", result.Username);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesUser()
    {
        var user = await _repository.GetByIdAsync(1);
        user!.Username = "updateduser";

        var result = await _repository.UpdateAsync(user);

        Assert.Equal("updateduser", result.Username);
        var updated = await _repository.GetByIdAsync(1);
        Assert.Equal("updateduser", updated!.Username);
    }

    [Fact]
    public async Task DeleteAsync_DeletesUser_WhenExists()
    {
        var result = await _repository.DeleteAsync(1);

        Assert.True(result);
        var deleted = await _repository.GetByIdAsync(1);
        Assert.Null(deleted);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
