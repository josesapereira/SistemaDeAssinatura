using System.Linq.Expressions;
using Domain.Models;
using Domain.DTOs;
using Domain.Interfaces.Repository;
using Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Infraestrutura.Repository;

public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public override async Task<List<Usuario>> GetAllAsync(Expression<Func<Usuario, bool>>? filtro = null)
    {
        var query = _dbSet
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (filtro != null)
        {
            query = query.Where(filtro);
        }

        return await query.ToListAsync();
    }

    public async Task<UsuarioDto> ProximaPagina(int pagina, int quantidade, Expression<Func<Usuario, bool>>? filtro = null)
    {
        var query = _dbSet
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (filtro != null)
        {
            query = query.Where(filtro);
        }

        var totalItens = await query.CountAsync();
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)quantidade);

        var usuarios = await query
            .Skip((pagina - 1) * quantidade)
            .Take(quantidade)
            .ToListAsync();

        var usuarioDto = new UsuarioDto
        {
            PaginaAtual = pagina,
            TotalPaginas = totalPaginas,
            TotalItens = totalItens
        };

        return usuarioDto;
    }

    public async Task<Usuario?> GetByREAsync(string re)
    {
        return await _dbSet
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == re);
    }

    public async Task<bool> UsuarioExisteAsync(string re)
    {
        return await _dbSet.AnyAsync(u => u.UserName == re);
    }

    public override async Task<Usuario?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}

