using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models.Entities;
using project.Repositories.Interfaces;

namespace project.Repositories;

public class ShelterRepository : IShelterRepository
{
    private readonly AppDbContext _db;

    public ShelterRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Shelter>> GetAllAsync()
    {
        return _db.Shelters.ToListAsync();
    }

    public Task<Shelter?> GetByIdAsync(int id)
    {
        return _db.Shelters.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Shelter> AddAsync(Shelter shelter)
    {
        shelter.CreatedAt = DateTime.UtcNow;
        _db.Shelters.Add(shelter);
        await _db.SaveChangesAsync();
        return shelter;
    }

    public async Task<Shelter> UpdateAsync(Shelter shelter)
    {
        shelter.UpdatedAt = DateTime.UtcNow;
        _db.Shelters.Update(shelter);
        await _db.SaveChangesAsync();
        return shelter;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var shelter = await _db.Shelters.FindAsync(id);
        if (shelter == null)
            return false;

        _db.Shelters.Remove(shelter);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetAnimalCountAsync(int shelterId)
    {
        return await _db.Animals.CountAsync(a => a.ShelterId == shelterId);
    }
}
