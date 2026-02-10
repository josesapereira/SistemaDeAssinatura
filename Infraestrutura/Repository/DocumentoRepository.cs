using Domain.DTOs;
using Domain.Enums;
using Domain.Interfaces.Repository;
using Domain.Models;
using Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infraestrutura.Repository;

public class DocumentoRepository : BaseRepository<Documento>, IDocumentoRepository
{
    public DocumentoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Documento?> GetByIdComRelacionamentosAsync(Guid id)
    {
        return await _context.Set<Documento>()
            .Include(d => d.TipoDeDocumento)
            .Include(d => d.UsuarioInclusao)
            .Include(d => d.Assinantes)
                .ThenInclude(a => a.UsuarioAssinante)
            .Include(d => d.Assinaturas)
                .ThenInclude(a => a.Assinante)
                    .ThenInclude(a => a.UsuarioAssinante)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<ResultadoPaginado<Documento>> GetAllComRelacionamentosAsync(
        Expression<Func<Documento, bool>>? filtro = null,
        Expression<Func<Documento, object>>? orderBy = null,
        bool ascending = true,
        int? pagina = null,
        int? quantidade = null)
    {
        IQueryable<Documento> query = _context.Set<Documento>()
            .Include(d => d.TipoDeDocumento)
            .Include(d => d.UsuarioInclusao)
            .Include(d => d.Assinantes)
                .ThenInclude(a => a.UsuarioAssinante)
            .Include(d => d.Assinaturas)
                .ThenInclude(a => a.Assinante)
                    .ThenInclude(a => a.UsuarioAssinante);

        if (filtro != null)
        {
            query = query.Where(filtro);
        }

        if (orderBy != null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        var totalItens = await query.CountAsync();

        if (pagina.HasValue && quantidade.HasValue && pagina.Value >= 0 && quantidade.Value >= 0)
        {
            query = query.Skip((pagina.Value) * quantidade.Value).Take(quantidade.Value);
        }

        var resultado = new ResultadoPaginado<Documento>
        {
            Itens = await query.ToListAsync(),
            TotalItens = totalItens
        };
        return resultado;
    }

    public async Task<List<Documento>> ObterDocumentosPendentesPorUsuarioAsync(long usuarioId)
    {
        return await _context.Set<Documento>()
            .Where(d => d.Assinantes.Any(a => a.AssinanteId == usuarioId) &&
                       (d.StatusDocumento == StatusDocumento.Aguardando_assinatura ||
                        d.StatusDocumento == StatusDocumento.Aguardando_assinatura_da_revogacao))
            .Include(d => d.TipoDeDocumento)
            .Include(d => d.UsuarioInclusao)
            .Include(d => d.Assinantes)
                .ThenInclude(a => a.UsuarioAssinante)
            .Include(d => d.Assinaturas)
                .ThenInclude(a => a.Assinante)
            .OrderByDescending(d => d.DataInclusao)
            .ToListAsync();
    }

    public async Task AdicionarAssinanteAsync(Assinante assinante)
    {
        await _context.Set<Assinante>().AddAsync(assinante);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAssinantesAsync(Guid documentoId)
    {
        var assinantes = await _context.Set<Assinante>()
            .Where(a => a.DocumentoId == documentoId)
            .ToListAsync();
        
        _context.Set<Assinante>().RemoveRange(assinantes);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Assinatura>> ObterAssinaturasPorDocumentoAsync(Guid documentoId, TipoAssinatura? tipoAssinatura = null)
    {
        var query = _context.Set<Assinatura>()
            .Where(a => a.DocumentoId == documentoId);

        if (tipoAssinatura.HasValue)
        {
            query = query.Where(a => a.TipoAssinatura == tipoAssinatura.Value);
        }

        return await query.ToListAsync();
    }

    public async Task AtualizarAssinaturaAsync(Assinatura assinatura)
    {
        _context.Set<Assinatura>().Update(assinatura);
        await _context.SaveChangesAsync();
    }
}

