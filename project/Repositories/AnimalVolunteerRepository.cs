using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models.Entities;
using project.Repositories.Interfaces;

namespace project.Repositories;

public class AnimalVolunteerRepository : IAnimalVolunteerRepository
{
    private readonly AppDbContext _db;

    public AnimalVolunteerRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<AnimalVolunteer>> GetAllAsync()
    {
        return _db.AnimalVolunteers
            .Include(av => av.Animal)
            .Include(av => av.User)
            .ToListAsync();
    }

    public Task<AnimalVolunteer?> GetByIdAsync(int id)
    {
        return _db.AnimalVolunteers
            .Include(av => av.Animal)
            .Include(av => av.User)
            .FirstOrDefaultAsync(av => av.Id == id);
    }

    public async Task<AnimalVolunteer> AddAsync(AnimalVolunteer animalVolunteer)
    {
        animalVolunteer.CreatedAt = DateTime.UtcNow;
        _db.AnimalVolunteers.Add(animalVolunteer);
        await _db.SaveChangesAsync();
        return animalVolunteer;
    }

    public async Task<AnimalVolunteer> UpdateAsync(AnimalVolunteer animalVolunteer)
    {
        _db.AnimalVolunteers.Update(animalVolunteer);
        await _db.SaveChangesAsync();
        return animalVolunteer;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var animalVolunteer = await _db.AnimalVolunteers.FindAsync(id);
        if (animalVolunteer == null)
            return false;

        _db.AnimalVolunteers.Remove(animalVolunteer);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<List<AnimalVolunteer>> GetByAnimalIdAsync(int animalId)
    {
        return _db.AnimalVolunteers
            .Where(av => av.AnimalId == animalId && av.UnassignedDate == null)
            .Include(av => av.User)
            .ToListAsync();
    }

    public Task<List<AnimalVolunteer>> GetByUserIdAsync(int userId)
    {
        return _db.AnimalVolunteers
            .Where(av => av.UserId == userId && av.UnassignedDate == null)
            .Include(av => av.Animal)
            .ToListAsync();
    }
}
