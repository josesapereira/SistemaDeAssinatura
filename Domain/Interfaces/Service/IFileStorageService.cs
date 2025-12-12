using Microsoft.AspNetCore.Http;

namespace Domain.Interfaces.Service;

public interface IFileStorageService
{
    Task<string> SalvarArquivoAsync(IFormFile arquivo, string pasta = "uploads");
    Task<bool> ExcluirArquivoAsync(string caminhoArquivo);
}

