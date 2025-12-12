using Domain.Interfaces.Repository;
using Domain.Models;
using Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Infraestrutura.Repository;

public class RegistroAbilityRepository : BaseRepository<RegistroAbility>, IRegistroAbilityRepository
{
    public RegistroAbilityRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<RegistroAbility>> GetAllAsync()
    {
        return await _dbSet.OrderBy(r => r.RE).ToListAsync();
    }

    public async Task<RegistroAbility?> GetByREAsync(string re)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.RE == re);
    }

    public async Task<bool> ExisteREAsync(string re)
    {
        return await _dbSet.AnyAsync(r => r.RE == re);
    }
}
