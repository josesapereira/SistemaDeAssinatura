using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

public class UsuarioRole : IdentityUserRole<Guid>
{
    public Usuario Usuario { get; set; } = null!;
    public Role Role { get; set; } = null!;
}



