using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class TipoDocumento : BaseId
{
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descricao { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}

