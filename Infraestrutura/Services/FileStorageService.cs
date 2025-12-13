using Domain.Interfaces.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO.Pipelines;

namespace Infraestrutura.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IHostEnvironment? _environment;
    private const string BaseUploadPath = "uploads/usuarios";

    public FileStorageService(IHostEnvironment? environment = null)
    {
        _environment = environment;
    }

    public async Task<string> SalvarArquivoAsync(byte[] arquivo, string fileName)
    {
        if (arquivo == null || arquivo.Length == 0)
            throw new ArgumentException("Arquivo inválido");

        if (_environment == null || string.IsNullOrEmpty(_environment.ContentRootPath))
            return fileName;

        var uploadPath = Path.Combine(_environment.ContentRootPath, BaseUploadPath);

        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, fileName);

        using var image = Image.Load(arquivo);

        // 🔹 Ajusta DPI (apenas para impressão)
        image.Metadata.HorizontalResolution = 1200;
        image.Metadata.VerticalResolution = 1200;

        // 🔹 Redimensiona mantendo proporção (máx 1200px)
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(1200, 1200)
        }));

        // 🔹 Controle de qualidade JPEG
        var encoder = new JpegEncoder
        {
            Quality = 85 // 1–100 (85 é excelente equilíbrio)
        };

        await image.SaveAsync(filePath, encoder);

        return fileName;
    }

    //public async Task<string> SalvarArquivoAsync(byte[] arquivo, string fileName)
    //{
    //    if (arquivo == null || arquivo.Length == 0)
    //        throw new ArgumentException("NomeDoArquivo inválido");

    //    if (_environment != null && !string.IsNullOrEmpty(_environment.ContentRootPath))
    //    {
    //        var uploadPath = Path.Combine(_environment.ContentRootPath, BaseUploadPath);
    //        var Memory = new MemoryStream();
    //        await Memory.WriteAsync(arquivo);
    //        Memory.Position = 0;
    //        if (!Directory.Exists(uploadPath))
    //        {
    //            Directory.CreateDirectory(uploadPath);
    //        }

    //        var filePath = Path.Combine(uploadPath, fileName);

    //        using (var stream = new FileStream(filePath, FileMode.Create))
    //        {
    //            await stream.CopyToAsync(Memory);
    //        }
    //    }

    //    // Retorna o caminho relativo que será usado pela aplicação
    //    return $"{fileName}";
    //}

    public async Task<byte[]?> LerArquivoAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        if (_environment == null || string.IsNullOrEmpty(_environment.ContentRootPath))
            return null;

        var uploadPath = Path.Combine(_environment.ContentRootPath, BaseUploadPath);
        var filePath = Path.Combine(uploadPath, fileName);

        if (!File.Exists(filePath))
            return null;

        return await File.ReadAllBytesAsync(filePath);
    }

}
