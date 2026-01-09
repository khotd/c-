using project.Models.DTO;
using project.Models.Entities;

namespace project.Repositories.Interfaces;

public interface IAnimalRepository
{
    Task<List<Animal>> GetAllAsync();
    Task<Animal?> GetByIdAsync(int id);
    Task<Animal> AddAsync(Animal animal);
    Task<Animal> UpdateAsync(Animal animal);
    Task<bool> DeleteAsync(int id);
    Task<PagedResponse<Animal>> GetPagedAsync(AnimalFilterDto filter);
    Task<List<Animal>> GetByShelterIdAsync(int shelterId);
}
