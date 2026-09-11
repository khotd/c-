using project.Models.Entities;

namespace project.Repositories.Interfaces;

public interface IAdoptionRepository
{
    Task<List<Adoption>> GetAllAsync();
    Task<Adoption?> GetByIdAsync(int id);
    Task<Adoption> AddAsync(Adoption adoption);
    Task<Adoption> UpdateAsync(Adoption adoption);
    Task<bool> DeleteAsync(int id);
    Task<List<Adoption>> GetByAnimalIdAsync(int animalId);
    Task<List<Adoption>> GetByUserIdAsync(int userId);
}
