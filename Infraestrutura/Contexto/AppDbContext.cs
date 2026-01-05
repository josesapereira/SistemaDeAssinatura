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
    public DbSet<Documento> Documento { get; set; }
    public DbSet<Assinante> Assinante { get; set; }
    public DbSet<Assinatura> Assinatura { get; set; }

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

        // Configurar Documento
        builder.Entity<Documento>(entity =>
        {
            entity.HasOne(d => d.TipoDeDocumento)
                .WithMany()
                .HasForeignKey(d => d.TipoDeDocumentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UsuarioInclusao)
                .WithMany()
                .HasForeignKey(d => d.UsuarioInclusaoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(d => d.Assinantes)
                .WithOne(a => a.Documento)
                .HasForeignKey(a => a.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Assinaturas)
                .WithOne(a => a.Documento)
                .HasForeignKey(a => a.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar Assinante
        builder.Entity<Assinante>(entity =>
        {
            entity.HasOne(a => a.UsuarioAssinante)
                .WithMany()
                .HasForeignKey(a => a.AssinanteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.AssinanteId, a.DocumentoId })
                .IsUnique();
        });

        // Configurar Assinatura
        builder.Entity<Assinatura>(entity =>
        {
            entity.HasOne(a => a.Assinante)
                .WithMany()
                .HasForeignKey(a => a.AssinanteId)
                .OnDelete(DeleteBehavior.Restrict);
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

