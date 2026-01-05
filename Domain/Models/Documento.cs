using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Models;

public class Documento : BaseId
{
    [Required]
    public Guid TipoDeDocumentoId { get; set; }

    [ForeignKey(nameof(TipoDeDocumentoId))]
    public TipoDocumento TipoDeDocumento { get; set; } = null!;

    [Required]
    public DateTime DataInclusao { get; set; }

    [Required]
    public long UsuarioInclusaoId { get; set; }

    [ForeignKey(nameof(UsuarioInclusaoId))]
    public Usuario UsuarioInclusao { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string NomeDoArquivo { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string HashDocumentoOriginal { get; set; } = string.Empty;

    [Required]
    public StatusDocumento StatusDocumento { get; set; }

    public List<Assinante> Assinantes { get; set; } = new();

    public List<Assinatura> Assinaturas { get; set; } = new();
}

