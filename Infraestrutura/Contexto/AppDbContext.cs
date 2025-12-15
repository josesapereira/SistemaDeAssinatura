using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infraestrutura.Contexto;

public class AppDbContext : IdentityDbContext<Usuario, Role, long, IdentityUserClaim<long>, UsuarioRole, IdentityUserLogin<long>, IdentityRoleClaim<long>, IdentityUserToken<long>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RegistroAbility> RegistroAbility { get; set; }
    public DbSet<TipoDocumento> TipoDocumento { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UsuarioRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            userRole.HasOne(ur => ur.Usuario)
                .WithMany(r => r.Roles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            userRole.Navigation(ur => ur.Role).AutoInclude();
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

