using project.Models.Entities;

namespace project.Repositories.Interfaces;

public interface IAnimalVolunteerRepository
{
    Task<List<AnimalVolunteer>> GetAllAsync();
    Task<AnimalVolunteer?> GetByIdAsync(int id);
    Task<AnimalVolunteer> AddAsync(AnimalVolunteer animalVolunteer);
    Task<AnimalVolunteer> UpdateAsync(AnimalVolunteer animalVolunteer);
    Task<bool> DeleteAsync(int id);
    Task<List<AnimalVolunteer>> GetByAnimalIdAsync(int animalId);
    Task<List<AnimalVolunteer>> GetByUserIdAsync(int userId);
}
