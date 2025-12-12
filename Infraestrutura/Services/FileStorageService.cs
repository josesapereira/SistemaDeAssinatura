using Domain.Interfaces.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Infraestrutura.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IHostEnvironment? _environment;
    private const string BaseUploadPath = "uploads/usuarios";

    public FileStorageService(IHostEnvironment? environment = null)
    {
        _environment = environment;
    }

    public async Task<string> SalvarArquivoAsync(IFormFile arquivo, string pasta = "uploads")
    {
        if (arquivo == null || arquivo.Length == 0)
            throw new ArgumentException("Arquivo inválido");

        // Gera um nome único para o arquivo
        var extension = Path.GetExtension(arquivo.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        
        // Se tiver ambiente, salva o arquivo
        // Nota: IHostEnvironment não tem WebRootPath, então vamos usar ContentRootPath
        if (_environment != null && !string.IsNullOrEmpty(_environment.ContentRootPath))
        {
            var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadPath = Path.Combine(wwwrootPath, BaseUploadPath);
            
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }
        }
        
        // Retorna o caminho relativo que será usado pela aplicação
        return $"/{BaseUploadPath}/{fileName}";
    }

    public async Task<bool> ExcluirArquivoAsync(string caminhoArquivo)
    {
        if (string.IsNullOrEmpty(caminhoArquivo))
            return false;

        try
        {
            if (_environment != null && !string.IsNullOrEmpty(_environment.ContentRootPath))
            {
                var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                var filePath = caminhoArquivo.StartsWith("/") 
                    ? Path.Combine(wwwrootPath, caminhoArquivo.TrimStart('/'))
                    : caminhoArquivo;

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return await Task.FromResult(true);
                }
            }
        }
        catch
        {
            // Log error if needed
        }

        return false;
    }
}
