using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Infraestrutura.Contexto;

public class AppDbContext : IdentityDbContext<Usuario, Role, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configurar ID do Usuario para gerar GUID automaticamente no banco de dados
        builder.Entity<Usuario>(entity =>
        {
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");
        });

        // Configurar ID da Role para gerar GUID automaticamente no banco de dados
        builder.Entity<Role>(entity =>
        {
            entity.Property(r => r.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");
        });

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
    }
}

