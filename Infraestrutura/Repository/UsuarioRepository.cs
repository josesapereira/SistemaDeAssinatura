using System.Linq.Expressions;
using Domain.Models;
using Domain.DTOs;
using Domain.Interfaces.Repository;
using Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Infraestrutura.Repository;

public class UsuarioRepository : IUsuarioRepository
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<Usuario> _dbSet;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Usuario>();
        //if(context.)
        //context.Database.OpenConnectionAsync().GetAwaiter().GetResult();
    }

    // Método principal com long
    public virtual async Task<Usuario?> GetByIdAsync(long id)
    {
        var usuario = await _dbSet.Include(x => x.Roles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
        if (usuario == null)
            return null;

        return usuario;
    }

    public virtual async Task<ResultadoPaginado<Usuario>> GetAllAsync(Expression<Func<Usuario, bool>>? filtro = null, Expression<Func<Usuario, object>>? orderBy = null,
        bool ascending = true, int? pagina = null, int? quantidade = null)
    {
        IQueryable<Usuario> query = _dbSet.AsQueryable();

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

        var usuarios = await query.Include(x => x.Roles).ThenInclude(x => x.Role).ToListAsync();
        
        if (usuarios.Any())
        {
            var usuarioIds = usuarios.Select(u => u.Id).ToList();
            
            // Carregar todas as roles de uma vez
            var todasRoles = await _context.Set<UsuarioRole>()
                .Where(ur => usuarioIds.Contains(ur.UserId))
                .Include(ur => ur.Role)
                .ToListAsync();

            // Associar roles aos usuários
            foreach (var usuario in usuarios)
            {
                usuario.Roles = todasRoles.Where(ur => ur.UserId == usuario.Id).ToList();
            }
        }

        var resultado = new ResultadoPaginado<Usuario>
        {
            Itens = usuarios,
            TotalItens = await CountAsync(filtro)
        };
        return resultado;
    }

    public virtual async Task<int> CountAsync(Expression<Func<Usuario, bool>>? filtro = null)
    {
        IQueryable<Usuario> query = _dbSet;

        if (filtro != null)
        {
            query = query.Where(filtro);
        }

        return await query.CountAsync();
    }

    public virtual async Task<Usuario> AdicionarAsync(Usuario entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<Usuario> AtualizarAsync(Usuario entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    // Métodos específicos do IUsuarioRepository
    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        var usuario = await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
        if (usuario == null)
            return null;

        // Carregar roles explicitamente da tabela AspNetUserRoles usando UsuarioRole
        var usuarioRoles = await _context.Set<UsuarioRole>()
            .Where(ur => ur.UserId == usuario.Id)
            .Include(ur => ur.Role)
            .ToListAsync();

        // Associar as roles ao usuário
        usuario.Roles = usuarioRoles;

        return usuario;
    }

    public async Task<Usuario?> GetByREAsync(string re)
    {
        var usuario = await _dbSet.FirstOrDefaultAsync(u => u.UserName == re);
        if (usuario == null)
            return null;

        // Carregar roles explicitamente da tabela AspNetUserRoles usando UsuarioRole
        var usuarioRoles = await _context.Set<UsuarioRole>()
            .Where(ur => ur.UserId == usuario.Id)
            .Include(ur => ur.Role)
            .ToListAsync();

        // Associar as roles ao usuário
        usuario.Roles = usuarioRoles;

        return usuario;
    }

    public async Task<bool> UsuarioExisteAsync(string re)
    {
        return await _dbSet.AnyAsync(u => u.UserName == re);
    }
}

