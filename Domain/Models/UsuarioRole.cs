using Domain.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

public class UsuarioRole : IdentityUserRole<long>
{
    public Usuario Usuario { get; set; }
    //[AutoInclude]
    public Role Role { get; set; }
}



