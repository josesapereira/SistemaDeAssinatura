using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

public class Role : IdentityRole<Guid>
{
    public Role(string role) => Name = role;
    
    public Role() { }

    public List<UsuarioRole> UserRoles { get; set; } = new();
}



