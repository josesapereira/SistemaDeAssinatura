using Domain.Interfaces.Repository;
using Domain.Models;
using Infraestrutura.Contexto;

namespace Infraestrutura.Repository;

public class TipoDocumentoRepository : BaseRepository<TipoDocumento>, ITipoDocumentoRepository
{
    public TipoDocumentoRepository(AppDbContext context) : base(context)
    {
    }
}

