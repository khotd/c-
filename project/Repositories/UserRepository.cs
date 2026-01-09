using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models.Entities;
using project.Repositories.Interfaces;

namespace project.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<User>> GetAllAsync()
    {
        return _db.Users.ToListAsync();
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> AddAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
}
