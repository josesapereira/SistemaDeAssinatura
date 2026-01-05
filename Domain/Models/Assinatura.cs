using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Models;

public class Assinatura : BaseId
{
    [Required]
    public DateTime DataDaAssinatura { get; set; }

    [Required]
    public TipoAssinatura TipoAssinatura { get; set; }

    [Required]
    public Guid AssinanteId { get; set; }

    [ForeignKey(nameof(AssinanteId))]
    public Assinante Assinante { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string HashDocumentoAssinado { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string HashNomeArquivoReconhecimentoFacial { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string NomeArquivoReconhecimentoFacial { get; set; } = string.Empty;

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [Required]
    [StringLength(100)]
    public string SistemaDeAssinatura { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string VersaoSistema { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SistemaOperacional { get; set; } = string.Empty;

    [StringLength(50)]
    public string IPDaAssinatura { get; set; } = string.Empty;

    [Required]
    public Guid DocumentoId { get; set; }

    [ForeignKey(nameof(DocumentoId))]
    public Documento Documento { get; set; } = null!;
}

