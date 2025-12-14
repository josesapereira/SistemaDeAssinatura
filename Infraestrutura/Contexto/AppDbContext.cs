using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Infraestrutura.Contexto;

public class AppDbContext : IdentityDbContext<Usuario, Role, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RegistroAbility> RegistroAbility { get; set; }
    public DbSet<TipoDocumento> TipoDocumento { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configurar relacionamento Usuario ↔ UsuarioRole ↔ Role
        builder.Entity<UsuarioRole>(entity =>
        {
            entity.HasOne(ur => ur.Usuario)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade).IsRequired();

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade).IsRequired();
            entity.Navigation(ur => ur.Role).AutoInclude();
        });
        // Configurar RegistroAbility
        builder.Entity<RegistroAbility>(entity =>
        {
            entity.HasIndex(r => r.RE)
                .IsUnique();
        });
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            foreach (var navigation in entityType.GetNavigations())
            {
                // Pega a propriedade real do CLR
                var propertyInfo = clrType.GetProperty(navigation.Name);
                if (propertyInfo == null)
                    continue;

                // Verifica se tem o [AutoInclude]
                if (Attribute.IsDefined(propertyInfo, typeof(Domain.Extensions.AutoIncludeAttribute)))
                {
                    builder.Entity(clrType)
                                .Navigation(navigation.Name)
                                .AutoInclude();
                }
            }
        }
    }
}

