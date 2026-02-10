using Domain.DTOs;
using Domain.Enums;
using Domain.Models;
using System.Linq.Expressions;

namespace Domain.Interfaces.Repository;

public interface IDocumentoRepository : IBaseRepository<Documento>
{
    Task<List<Documento>> ObterDocumentosPendentesPorUsuarioAsync(long usuarioId);
    Task<Documento?> GetByIdComRelacionamentosAsync(Guid id);
    Task<ResultadoPaginado<Documento>> GetAllComRelacionamentosAsync(
        Expression<Func<Documento, bool>>? filtro = null,
        Expression<Func<Documento, object>>? orderBy = null,
        bool ascending = true,
        int? pagina = null,
        int? quantidade = null);
    Task AdicionarAssinanteAsync(Assinante assinante);
    Task RemoverAssinantesAsync(Guid documentoId);
    Task<List<Assinatura>> ObterAssinaturasPorDocumentoAsync(Guid documentoId, TipoAssinatura? tipoAssinatura = null);
    Task AtualizarAssinaturaAsync(Assinatura assinatura);
}

