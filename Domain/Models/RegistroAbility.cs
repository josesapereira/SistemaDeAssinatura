using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class RegistroAbility : BaseId
{
    [StringLength(100)]
    public string RE { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    [StringLength(100)]
    public string RECoordenador { get; set; } = string.Empty;
    [StringLength(100)]
    public string Coordenador { get; set; } = string.Empty;
    [StringLength(100)]
    public string RESupervisor { get; set; } = string.Empty;
    [StringLength(100)]
    public string Supervisor { get; set; } = string.Empty;
    [StringLength(100)]
    public string REGerente { get; set; } = string.Empty;
    [StringLength(100)]
    public string Gerente { get; set; } = string.Empty;
    public DateTime DatadeInclusao { get; set; }
    [StringLength(20)]
    public string CentroDeCusto { get; set; }
    [StringLength(100)]
    public string Setor { get; set; }
    [StringLength(100)]
    public string Departamento { get; set; }
    public int CargoId { get; set; }
    [StringLength(100)]
    public string Cargo { get; set; }
    public DateTime? DatadeDemissao { get; set; }
    [StringLength(100)]
    public string ReTelefonica { get; set; }
    [NotMapped]
    public string RENome
    {
        get
        {
            return $"{RE} - {Nome}";
        }
    }
}

