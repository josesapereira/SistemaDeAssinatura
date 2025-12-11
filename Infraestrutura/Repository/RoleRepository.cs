using Domain.Models;
using Domain.Interfaces.Repository;
using Infraestrutura.Contexto;
using Infraestrutura.Repository;

namespace Infraestrutura.Repository;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await base.GetAllAsync(null);
    }
}

