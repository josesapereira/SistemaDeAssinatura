using Domain.DTOs;
using Domain.Enums;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Service.Implementacoes;

public class DocumentoService : IDocumentoService
{
    private readonly IDocumentoRepository _repository;
    private readonly ITipoDocumentoRepository _tipoDocumentoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly AppDbContext _context;

    public DocumentoService(
        IDocumentoRepository repository,
        ITipoDocumentoRepository tipoDocumentoRepository,
        IUsuarioRepository usuarioRepository,
        IFileStorageService fileStorageService,
        AppDbContext context)
    {
        _repository = repository;
        _tipoDocumentoRepository = tipoDocumentoRepository;
        _usuarioRepository = usuarioRepository;
        _fileStorageService = fileStorageService;
        _context = context;
    }

    public async Task<RespostaDTO<object>> SalvarAsync(DocumentoDTO dto)
    {
        var resposta = new RespostaDTO<object>();

        // Regra 1: Validar campos obrigatórios
        if (dto.Assinantes == null || !dto.Assinantes.Any())
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "É necessário informar pelo menos um assinante";
            resposta.Erros.Add("É necessário informar pelo menos um assinante");
            return resposta;
        }

        if (string.IsNullOrWhiteSpace(dto.NomeDoArquivo))
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Nome do arquivo é obrigatório";
            resposta.Erros.Add("Nome do arquivo é obrigatório");
            return resposta;
        }

        if (dto.TipoDeDocumentoId == Guid.Empty)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Tipo de documento é obrigatório";
            resposta.Erros.Add("Tipo de documento é obrigatório");
            return resposta;
        }

        if (dto.UsuarioInclusaoId == 0)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário de inclusão é obrigatório";
            resposta.Erros.Add("Usuário de inclusão é obrigatório");
            return resposta;
        }

        // Validar se tipo de documento existe
        var tipoDocumento = await _tipoDocumentoRepository.GetByIdAsync(dto.TipoDeDocumentoId);
        if (tipoDocumento == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Tipo de documento não encontrado";
            resposta.Erros.Add("Tipo de documento não encontrado");
            return resposta;
        }

        // Validar se usuário de inclusão existe
        var usuarioInclusao = await _usuarioRepository.GetByIdAsync(dto.UsuarioInclusaoId);
        if (usuarioInclusao == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário de inclusão não encontrado";
            resposta.Erros.Add("Usuário de inclusão não encontrado");
            return resposta;
        }

        try
        {
            // Salvar arquivo se fornecido
            string nomeArquivoSalvo = dto.NomeDoArquivo;
            if (dto.Arquivo != null && dto.Arquivo.Length > 0)
            {
                // Gerar nome único para o arquivo
                if (string.IsNullOrWhiteSpace(nomeArquivoSalvo) || !nomeArquivoSalvo.Contains("."))
                {
                    nomeArquivoSalvo = Guid.NewGuid().ToString() + ".pdf";
                }
                else if (!nomeArquivoSalvo.StartsWith(Guid.NewGuid().ToString().Substring(0, 8)))
                {
                    nomeArquivoSalvo = Guid.NewGuid().ToString() + Path.GetExtension(nomeArquivoSalvo);
                }

                await _fileStorageService.SalvarArquivoAsync(dto.Arquivo, nomeArquivoSalvo);
            }

            // Gerar hash do arquivo
            string hashDocumento = string.Empty;
            if (dto.Arquivo != null && dto.Arquivo.Length > 0)
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(dto.Arquivo);
                    hashDocumento = Convert.ToBase64String(hashBytes);
                }
            }

            if (dto.Id == Guid.Empty)
            {
                // Criar novo documento
                var documento = new Documento
                {
                    TipoDeDocumentoId = dto.TipoDeDocumentoId,
                    DataInclusao = DateTime.Now,
                    UsuarioInclusaoId = dto.UsuarioInclusaoId,
                    NomeDoArquivo = nomeArquivoSalvo,
                    HashDocumentoOriginal = hashDocumento,
                    StatusDocumento = StatusDocumento.Aguardando_assinatura // Regra 2: Status inicial
                };

                documento = await _repository.AdicionarAsync(documento);

                // Adicionar assinantes
                foreach (var assinanteDTO in dto.Assinantes)
                {
                    var usuarioAssinante = await _usuarioRepository.GetByIdAsync(assinanteDTO.AssinanteId);
                    if (usuarioAssinante == null)
                    {
                        resposta.Sucesso = false;
                        resposta.Mensagem = $"Usuário assinante com ID {assinanteDTO.AssinanteId} não encontrado";
                        resposta.Erros.Add($"Usuário assinante com ID {assinanteDTO.AssinanteId} não encontrado");
                        return resposta;
                    }

                    var assinante = new Assinante
                    {
                        AssinanteId = assinanteDTO.AssinanteId,
                        DocumentoId = documento.Id
                    };

                    await _context.Set<Assinante>().AddAsync(assinante);
                }

                await _context.SaveChangesAsync();

                resposta.Sucesso = true;
                resposta.Mensagem = "Documento criado com sucesso";
            }
            else
            {
                // Atualizar documento existente
                var documento = await _repository.GetByIdAsync(dto.Id);
                if (documento == null)
                {
                    resposta.Sucesso = false;
                    resposta.Mensagem = "Documento não encontrado";
                    resposta.Erros.Add("Documento não encontrado");
                    return resposta;
                }

                // Regra 3: Se status for Documento_revogado, atualizar assinaturas com Validacao para Revogacao
                if (dto.StatusDocumento == StatusDocumento.Documento_revogado)
                {
                    var assinaturas = await _context.Set<Assinatura>()
                        .Where(a => a.DocumentoId == documento.Id && a.TipoAssinatura == TipoAssinatura.Validacao)
                        .ToListAsync();

                    foreach (var assinatura in assinaturas)
                    {
                        assinatura.TipoAssinatura = TipoAssinatura.Revogacao;
                        _context.Set<Assinatura>().Update(assinatura);
                    }
                }

                documento.TipoDeDocumentoId = dto.TipoDeDocumentoId;
                documento.NomeDoArquivo = nomeArquivoSalvo;
                documento.StatusDocumento = dto.StatusDocumento;
                
                // Atualizar hash se arquivo foi alterado
                if (!string.IsNullOrEmpty(hashDocumento))
                {
                    documento.HashDocumentoOriginal = hashDocumento;
                }

                // Atualizar assinantes
                var assinantesExistentes = await _context.Set<Assinante>()
                    .Where(a => a.DocumentoId == documento.Id)
                    .ToListAsync();

                _context.Set<Assinante>().RemoveRange(assinantesExistentes);

                foreach (var assinanteDTO in dto.Assinantes)
                {
                    var usuarioAssinante = await _usuarioRepository.GetByIdAsync(assinanteDTO.AssinanteId);
                    if (usuarioAssinante == null)
                    {
                        resposta.Sucesso = false;
                        resposta.Mensagem = $"Usuário assinante com ID {assinanteDTO.AssinanteId} não encontrado";
                        resposta.Erros.Add($"Usuário assinante com ID {assinanteDTO.AssinanteId} não encontrado");
                        return resposta;
                    }

                    var assinante = new Assinante
                    {
                        AssinanteId = assinanteDTO.AssinanteId,
                        DocumentoId = documento.Id
                    };

                    await _context.Set<Assinante>().AddAsync(assinante);
                }

                await _repository.AtualizarAsync(documento);
                await _context.SaveChangesAsync();

                resposta.Sucesso = true;
                resposta.Mensagem = "Documento atualizado com sucesso";
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Erro ao salvar documento: {ex.Message}";
            resposta.Erros.Add(ex.Message);
        }

        return resposta;
    }

    private (StatusDaAssinatura status, DateTime? dataAssinatura) CalcularStatusAssinante(
        Assinante assinante,
        StatusDocumento statusDocumento,
        List<Assinatura> assinaturas)
    {
        // Se status for Cancelado
        if (statusDocumento == StatusDocumento.Documento_cancelado)
        {
            return (StatusDaAssinatura.Cancelado, null);
        }

        // Se status for Aguardando_assinatura ou Assinado, buscar assinatura de Validação
        if (statusDocumento == StatusDocumento.Aguardando_assinatura || 
            statusDocumento == StatusDocumento.Assinado)
        {
            var assinatura = assinaturas
                .FirstOrDefault(a => a.AssinanteId == assinante.Id && 
                                     a.TipoAssinatura == TipoAssinatura.Validacao);

            if (assinatura == null)
            {
                return (StatusDaAssinatura.Pendente, null);
            }

            return (StatusDaAssinatura.Assinado, assinatura.DataDaAssinatura);
        }

        // Se status for Aguardando_assinatura_da_revogacao ou Documento_revogado, buscar assinatura de Revogação
        if (statusDocumento == StatusDocumento.Aguardando_assinatura_da_revogacao || 
            statusDocumento == StatusDocumento.Documento_revogado)
        {
            var assinatura = assinaturas
                .FirstOrDefault(a => a.AssinanteId == assinante.Id && 
                                     a.TipoAssinatura == TipoAssinatura.Revogacao);

            if (assinatura == null)
            {
                return (StatusDaAssinatura.Pendente, null);
            }

            return (StatusDaAssinatura.Assinado, assinatura.DataDaAssinatura);
        }

        // Default: Pendente
        return (StatusDaAssinatura.Pendente, null);
    }

    public async Task<RespostaDTO<ResultadoPaginado<DocumentoDTO>>> ListarAsync(
        Expression<Func<Documento, bool>>? filtro = null,
        Expression<Func<Documento, object>>? orderBy = null,
        bool ascending = true,
        int? pagina = null,
        int? quantidade = null)
    {
        var resposta = new RespostaDTO<ResultadoPaginado<DocumentoDTO>>();

        try
        {
            var resultado = await _repository.GetAllAsync(filtro, orderBy, ascending, pagina, quantidade);

            var listaDTO = new List<DocumentoDTO>();

            foreach (var documento in resultado.Itens)
            {
                // Carregar relacionamentos
                await _context.Entry(documento)
                    .Reference(d => d.TipoDeDocumento)
                    .LoadAsync();

                await _context.Entry(documento)
                    .Reference(d => d.UsuarioInclusao)
                    .LoadAsync();

                await _context.Entry(documento)
                    .Collection(d => d.Assinantes)
                    .LoadAsync();

                await _context.Entry(documento)
                    .Collection(d => d.Assinaturas)
                    .LoadAsync();

                // Carregar assinantes com usuários
                foreach (var assinante in documento.Assinantes)
                {
                    await _context.Entry(assinante)
                        .Reference(a => a.UsuarioAssinante)
                        .LoadAsync();
                }

                // Carregar assinaturas com assinantes
                foreach (var assinatura in documento.Assinaturas)
                {
                    await _context.Entry(assinatura)
                        .Reference(a => a.Assinante)
                        .LoadAsync();
                }

                var documentoDTO = new DocumentoDTO
                {
                    Id = documento.Id,
                    TipoDeDocumentoId = documento.TipoDeDocumentoId,
                    TipoDeDocumento = documento.TipoDeDocumento?.Nome ?? string.Empty,
                    DataInclusao = documento.DataInclusao,
                    UsuarioInclusaoId = documento.UsuarioInclusaoId,
                    UsuarioInclusao = documento.UsuarioInclusao?.Nome ?? string.Empty,
                    NomeDoArquivo = documento.NomeDoArquivo,
                    StatusDocumento = documento.StatusDocumento,
                    Assinantes = documento.Assinantes.Select(a =>
                    {
                        var (status, dataAssinatura) = CalcularStatusAssinante(a, documento.StatusDocumento, documento.Assinaturas);
                        return new AssinanteDTO
                        {
                            Id = a.Id,
                            AssinanteId = a.AssinanteId,
                            UsuarioAssinante = a.UsuarioAssinante?.Nome ?? string.Empty,
                            DocumentoId = a.DocumentoId,
                            StatusDaAssinatura = status,
                            DataAssinatura = dataAssinatura
                        };
                    }).ToList(),
                    Assinaturas = documento.Assinaturas.Select(a => new AssinaturaDTO
                    {
                        Id = a.Id,
                        DataDaAssinatura = a.DataDaAssinatura,
                        TipoAssinatura = a.TipoAssinatura,
                        AssinanteId = a.AssinanteId,
                        Assinante = a.Assinante?.UsuarioAssinante?.Nome ?? string.Empty,
                        SistemaDeAssinatura = a.SistemaDeAssinatura,
                        VersaoSistema = a.VersaoSistema,
                        SistemaOperacional = a.SistemaOperacional,
                        IPDaAssinatura = a.IPDaAssinatura,
                        DocumentoId = a.DocumentoId
                    }).ToList()
                };

                listaDTO.Add(documentoDTO);
            }

            resposta.Sucesso = true;
            resposta.Mensagem = "Lista carregada com sucesso";
            resposta.Dados = new ResultadoPaginado<DocumentoDTO>
            {
                Itens = listaDTO,
                TotalItens = resultado.TotalItens
            };
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Erro ao listar documentos: {ex.Message}";
            resposta.Erros.Add(ex.Message);
        }

        return resposta;
    }

    public async Task<RespostaDTO<DocumentoDTO>> ObterPorIdAsync(Guid id)
    {
        var resposta = new RespostaDTO<DocumentoDTO>();

        try
        {
            var documento = await _repository.GetByIdAsync(id);
            if (documento == null)
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Documento não encontrado";
                return resposta;
            }

            // Carregar relacionamentos
            await _context.Entry(documento)
                .Reference(d => d.TipoDeDocumento)
                .LoadAsync();

            await _context.Entry(documento)
                .Reference(d => d.UsuarioInclusao)
                .LoadAsync();

            await _context.Entry(documento)
                .Collection(d => d.Assinantes)
                .LoadAsync();

            await _context.Entry(documento)
                .Collection(d => d.Assinaturas)
                .LoadAsync();

            // Carregar assinantes com usuários
            foreach (var assinante in documento.Assinantes)
            {
                await _context.Entry(assinante)
                    .Reference(a => a.UsuarioAssinante)
                    .LoadAsync();
            }

            // Carregar assinaturas com assinantes
            foreach (var assinatura in documento.Assinaturas)
            {
                await _context.Entry(assinatura)
                    .Reference(a => a.Assinante)
                    .LoadAsync();

                if (assinatura.Assinante != null)
                {
                    await _context.Entry(assinatura.Assinante)
                        .Reference(a => a.UsuarioAssinante)
                        .LoadAsync();
                }
            }

            // Carregar arquivo se existir
            byte[]? arquivo = null;
            if (!string.IsNullOrWhiteSpace(documento.NomeDoArquivo))
            {
                arquivo = await _fileStorageService.LerArquivoAsync(documento.NomeDoArquivo);
            }

            resposta.Sucesso = true;
            resposta.Mensagem = "Documento encontrado";
            resposta.Dados = new DocumentoDTO
            {
                Id = documento.Id,
                TipoDeDocumentoId = documento.TipoDeDocumentoId,
                TipoDeDocumento = documento.TipoDeDocumento?.Nome ?? string.Empty,
                DataInclusao = documento.DataInclusao,
                UsuarioInclusaoId = documento.UsuarioInclusaoId,
                UsuarioInclusao = documento.UsuarioInclusao?.Nome ?? string.Empty,
                NomeDoArquivo = documento.NomeDoArquivo,
                StatusDocumento = documento.StatusDocumento,
                Arquivo = arquivo,
                Assinantes = documento.Assinantes.Select(a =>
                {
                    var (status, dataAssinatura) = CalcularStatusAssinante(a, documento.StatusDocumento, documento.Assinaturas);
                    return new AssinanteDTO
                    {
                        Id = a.Id,
                        AssinanteId = a.AssinanteId,
                        UsuarioAssinante = a.UsuarioAssinante?.Nome ?? string.Empty,
                        DocumentoId = a.DocumentoId,
                        StatusDaAssinatura = status,
                        DataAssinatura = dataAssinatura
                    };
                }).ToList(),
                Assinaturas = documento.Assinaturas.Select(a => new AssinaturaDTO
                {
                    Id = a.Id,
                    DataDaAssinatura = a.DataDaAssinatura,
                    TipoAssinatura = a.TipoAssinatura,
                    AssinanteId = a.AssinanteId,
                    Assinante = a.Assinante?.UsuarioAssinante?.Nome ?? string.Empty,
                        SistemaDeAssinatura = a.SistemaDeAssinatura,
                        VersaoSistema = a.VersaoSistema,
                        SistemaOperacional = a.SistemaOperacional,
                        IPDaAssinatura = a.IPDaAssinatura,
                        DocumentoId = a.DocumentoId
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Erro ao obter documento: {ex.Message}";
            resposta.Erros.Add(ex.Message);
        }

        return resposta;
    }
}

