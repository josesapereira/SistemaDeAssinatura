using Domain.DTOs;
using Domain.Interfaces.Repository;
using Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infraestrutura.Repository;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<ResultadoPaginado<T>> GetAllAsync(Expression<Func<T, bool>>? filtro = null, Expression<Func<T, object>>? orderBy = null,
        bool ascending = true, int? pagina = null, int? quantidade = null)
    {
        IQueryable<T> query = _dbSet;

        if (filtro != null)
        {
            query = query.Where(filtro);
        }

        if (orderBy != null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        if (pagina.HasValue && quantidade.HasValue && pagina.Value >= 0 && quantidade.Value >= 0)
        {
            query = query.Skip((pagina.Value) * quantidade.Value).Take(quantidade.Value);
        }
        var resultado = new ResultadoPaginado<T>
        {
            Itens = await query.ToListAsync(),
            TotalItens = await CountAsync(filtro)
        };
        return resultado;
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filtro = null)
    {
        IQueryable<T> query = _dbSet;

        if (filtro != null)
        {
            query = query.Where(filtro);
        }

        return await query.CountAsync();
    }

    public virtual async Task<T> AdicionarAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> AtualizarAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> ExcluirAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}

