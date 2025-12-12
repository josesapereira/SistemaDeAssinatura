using Domain.Models;

namespace Domain.Interfaces.Repository;

public interface IRegistroAbilityRepository
{
    Task<List<RegistroAbility>> GetAllAsync();
    Task<RegistroAbility?> GetByREAsync(string re);
    Task<bool> ExisteREAsync(string re);
}

