using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;

namespace Domain.DTOs;

public class UsuarioDTO
{
    public long Id { get; set; } = 0;
    public string UserName { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public string NomeDoArquivo { get; set; } = "";
    public byte[] ArquivoUpload { get; set; }
    public long? RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleIdString
    {
        get
        {
            return RoleId?.ToString() ?? string.Empty;
        }
        set
        {
            if (!string.IsNullOrEmpty(value) && long.TryParse(value, out var roleId))
            {
                RoleId = roleId;
            }
        }
    }
}
