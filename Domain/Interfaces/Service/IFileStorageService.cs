using Microsoft.AspNetCore.Http;

namespace Domain.Interfaces.Service;

public interface IFileStorageService
{
    Task<string> SalvarArquivoAsync(byte[] arquivo, string nomeArquivo);
    Task<bool> ExcluirArquivoAsync(string caminhoArquivo);
}

