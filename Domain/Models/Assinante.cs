using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class Assinante : BaseId
{
    [Required]
    public long AssinanteId { get; set; }

    [ForeignKey(nameof(AssinanteId))]
    public Usuario UsuarioAssinante { get; set; } = null!;

    [Required]
    public Guid DocumentoId { get; set; }

    [ForeignKey(nameof(DocumentoId))]
    public Documento Documento { get; set; } = null!;
}

