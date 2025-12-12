using Domain.DTOs;
using Domain.Models;
using Infraestrutura.Contexto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infraestrutura;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        UserManager<Usuario> userManager,
        RoleManager<Role> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Garantir que o banco está criado
            await _context.Database.MigrateAsync();

            // Criar roles se não existirem
            await SeedRolesAsync();

            // Criar usuário admin padrão se não existir
            await SeedDefaultAdminAsync();
            var usuario = await _context.Users.FirstAsync(x => x.UserName == "9167");           
            
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var result = await _userManager.ResetPasswordAsync(usuario, token, "123456");
            usuario.DoisFatoresAtivo = false;
            usuario.PrimeiroAcesso = true;
            usuario.TwoFactorEnabled = false;
            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            //await _usuarioRepository.AdicionarAsync(usuario);
            _logger.LogInformation("Seed do banco de dados concluído com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar seed do banco de dados");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Administrador", "Usuario", "Assinante" };

        foreach (var roleName in roles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new Role(roleName));
                _logger.LogInformation("Role '{RoleName}' criada", roleName);
            }
        }
    }

    private async Task SeedDefaultAdminAsync()
    {
        const string defaultAdminEmail = "9167";
        const string defaultAdminPassword = "123456";
        const string defaultAdminName = "José de Sá Pereira Junior";
        const string defaultAdminCpf = "00000000000";

        // Verificar se já existe um usuário admin
        var existingAdmin = await _userManager.FindByEmailAsync(defaultAdminEmail);
        if (existingAdmin != null)
        {
            _logger.LogInformation("Usuário admin padrão já existe");
            return;
        }

        // Criar usuário admin
        var adminUser = new Usuario
        {
            UserName = defaultAdminEmail,
            Email = defaultAdminEmail,
            EmailConfirmed = true,
            Ativo = true
        };

        var result = await _userManager.CreateAsync(adminUser, defaultAdminPassword);

        if (result.Succeeded)
        {
            // Adicionar role Administrador
            await _userManager.AddToRoleAsync(adminUser, "Administrador");
            _logger.LogInformation("Usuário admin padrão criado com sucesso. Email: {Email}, Senha: {Password}",
                defaultAdminEmail, defaultAdminPassword);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Erro ao criar usuário admin padrão: {Errors}", errors);
            throw new Exception($"Erro ao criar usuário admin padrão: {errors}");
        }
    }
}

