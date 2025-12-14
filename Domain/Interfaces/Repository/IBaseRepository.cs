using Domain.DTOs;
using System.Linq.Expressions;

namespace Domain.Interfaces.Repository;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<ResultadoPaginado<T>> GetAllAsync(Expression<Func<T, bool>>? filtro = null, Expression<Func<T, object>>? orderBy = null, bool ascending = true, int? pagina = null, int? quantidade = null);
    Task<int> CountAsync(Expression<Func<T, bool>>? filtro = null);
    Task<T> AdicionarAsync(T entity);
    Task<T> AtualizarAsync(T entity);
    Task<bool> ExcluirAsync(Guid id);
}

