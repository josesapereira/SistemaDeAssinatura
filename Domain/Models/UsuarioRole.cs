using Domain.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

public class UsuarioRole : IdentityUserRole<Guid>
{
    public Usuario Usuario { get; set; } = null!;
    [AutoInclude]
    public Role Role { get; set; } = null!;
}



