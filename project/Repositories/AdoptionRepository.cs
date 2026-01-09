using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models.Entities;
using project.Repositories.Interfaces;

namespace project.Repositories;

public class AdoptionRepository : IAdoptionRepository
{
    private readonly AppDbContext _db;

    public AdoptionRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Adoption>> GetAllAsync()
    {
        return _db.Adoptions
            .Include(a => a.Animal)
            .Include(a => a.User)
            .ToListAsync();
    }

    public Task<Adoption?> GetByIdAsync(int id)
    {
        return _db.Adoptions
            .Include(a => a.Animal)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Adoption> AddAsync(Adoption adoption)
    {
        adoption.CreatedAt = DateTime.UtcNow;
        _db.Adoptions.Add(adoption);
        await _db.SaveChangesAsync();
        return adoption;
    }

    public async Task<Adoption> UpdateAsync(Adoption adoption)
    {
        adoption.UpdatedAt = DateTime.UtcNow;
        _db.Adoptions.Update(adoption);
        await _db.SaveChangesAsync();
        return adoption;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var adoption = await _db.Adoptions.FindAsync(id);
        if (adoption == null)
            return false;

        _db.Adoptions.Remove(adoption);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<List<Adoption>> GetByAnimalIdAsync(int animalId)
    {
        return _db.Adoptions
            .Where(a => a.AnimalId == animalId)
            .Include(a => a.User)
            .ToListAsync();
    }

    public Task<List<Adoption>> GetByUserIdAsync(int userId)
    {
        return _db.Adoptions
            .Where(a => a.UserId == userId)
            .Include(a => a.Animal)
            .ToListAsync();
    }
}
