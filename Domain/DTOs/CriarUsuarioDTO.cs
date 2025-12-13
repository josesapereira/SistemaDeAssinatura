using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;

namespace Domain.DTOs;

public class CriarUsuarioDTO
{
    public Guid Id { get; set; } = Guid.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public string NomeDoArquivo { get; set; } = "";
    public byte[] ArquivoUpload { get; set; }
    public Guid? RoleId { get; set; }
    public string RoleIdString
    {
        get
        {
            return RoleId.ToString();
        }
        set
        {
            RoleId = Guid.Parse(value);
        }
    }
}

