using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class Role : IdentityRole<long>
{
    public Role(string role) => Name = role;
    
    public Role() { }

    public List<UsuarioRole> UserRoles { get; set; } = new();
    [NotMapped]
    public string IdString => Id.ToString();
}



