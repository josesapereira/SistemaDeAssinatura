using System.Net;
using System.Net.Mail;
using Domain.Interfaces.Service;
using Microsoft.Extensions.Configuration;

namespace Service.Implementacoes;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarCodigo2FAAsync(string email, string codigo)
    {
        var smtpHost = _configuration["SMTP:Host"];
        var smtpPort = int.Parse(_configuration["SMTP:Port"] ?? "587");
        var smtpUser = _configuration["SMTP:User"];
        var smtpPassword = _configuration["SMTP:Password"];
        var smtpFrom = _configuration["SMTP:From"];

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpFrom))
        {
            // Em desenvolvimento, apenas logar o código
            Console.WriteLine($"Código 2FA para {email}: {codigo}");
            return;
        }

        var mensagem = $"Seu código de autenticação de dois fatores é: {codigo}";

        // Validar que a mensagem não contém a palavra "Ability"
        if (email.Contains("Ability", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("A mensagem de email não pode conter a palavra 'Ability'");
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPassword),
            EnableSsl = true
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpFrom),
            Subject = "Código de Autenticação de Dois Fatores",
            Body = mensagem,
            IsBodyHtml = false
        };

        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}



