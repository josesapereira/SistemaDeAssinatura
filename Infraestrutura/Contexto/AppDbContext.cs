using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Infraestrutura.Contexto;

public class AppDbContext : IdentityDbContext<Usuario, Role, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RegistroAbility> RegistroAbilities { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configurar relacionamento Usuario ↔ UsuarioRole ↔ Role
        builder.Entity<UsuarioRole>(entity =>
        {
            entity.HasOne(ur => ur.Usuario)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar RegistroAbility
        builder.Entity<RegistroAbility>(entity =>
        {
            entity.HasIndex(r => r.RE)
                .IsUnique();
        });
    }
}

