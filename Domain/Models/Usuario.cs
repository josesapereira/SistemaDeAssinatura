using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

public class Usuario : IdentityUser<Guid>
{
    public string? NomeDaFotoRegistrada { get; set; }
    public bool PrimeiroAcesso { get; set; } = true;
    public bool DoisFatoresAtivo { get; set; } = false;
    public bool Ativo { get; set; } = true;
    public string? ArquivoUpload { get; set; }
    public List<UsuarioRole> Roles { get; set; } = new();
}



