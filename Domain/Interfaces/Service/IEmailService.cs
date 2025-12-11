namespace Domain.Interfaces.Service;

public interface IEmailService
{
    Task EnviarCodigo2FAAsync(string email, string codigo);
}



