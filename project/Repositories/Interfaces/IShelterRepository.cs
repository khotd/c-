using project.Models.Entities;

namespace project.Repositories.Interfaces;

public interface IShelterRepository
{
    Task<List<Shelter>> GetAllAsync();
    Task<Shelter?> GetByIdAsync(int id);
    Task<Shelter> AddAsync(Shelter shelter);
    Task<Shelter> UpdateAsync(Shelter shelter);
    Task<bool> DeleteAsync(int id);
    Task<int> GetAnimalCountAsync(int shelterId);
}
