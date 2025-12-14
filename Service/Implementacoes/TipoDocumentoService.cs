using AutoMapper;
using Domain.DTOs;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using System.Linq.Expressions;

namespace Service.Implementacoes;

public class TipoDocumentoService : ITipoDocumentoService
{
    private readonly ITipoDocumentoRepository _repository;
    private readonly IMapper _mapper;

    public TipoDocumentoService(ITipoDocumentoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RespostaDTO<object>> SalvarAsync(TipoDocumentoDTO dto)
    {
        var resposta = new RespostaDTO<object>();

        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Nome é obrigatório";
            resposta.Erros.Add("Nome é obrigatório");
            return resposta;
        }

        if (dto.Nome.Length > 100)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Nome deve ter no máximo 100 caracteres";
            resposta.Erros.Add("Nome deve ter no máximo 100 caracteres");
            return resposta;
        }

        if (dto.Descricao.Length > 500)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Descrição deve ter no máximo 500 caracteres";
            resposta.Erros.Add("Descrição deve ter no máximo 500 caracteres");
            return resposta;
        }

        try
        {
            if (dto.Id != Guid.Empty)
            {
                // Atualizar
                var existente = await _repository.GetByIdAsync(dto.Id);
                if (existente == null)
                {
                    resposta.Sucesso = false;
                    resposta.Mensagem = "Tipo de documento não encontrado";
                    return resposta;
                }

                existente.Nome = dto.Nome;
                existente.Descricao = dto.Descricao;
                existente.Ativo = dto.Ativo;

                await _repository.AtualizarAsync(existente);
                resposta.Sucesso = true;
                resposta.Mensagem = "Tipo de documento atualizado com sucesso";
            }
            else
            {
                // Criar
                var novo = new TipoDocumento
                {
                    Nome = dto.Nome,
                    Descricao = dto.Descricao,
                    Ativo = dto.Ativo
                };

                await _repository.AdicionarAsync(novo);
                resposta.Sucesso = true;
                resposta.Mensagem = "Tipo de documento criado com sucesso";
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Erro ao salvar tipo de documento: {ex.Message}";
            resposta.Erros.Add(ex.Message);
        }

        return resposta;
    }

    public async Task<RespostaDTO<ResultadoPaginado<TipoDocumentoDTO>>> ListarAsync(
        Expression<Func<TipoDocumento, bool>>? filtro = null,
        Expression<Func<TipoDocumento, object>>? orderBy = null,
        bool ascending = true,
        int? pagina = null,
        int? quantidade = null)
    {
        var resposta = new RespostaDTO<ResultadoPaginado<TipoDocumentoDTO>>();

        try
        {
            var resultado = await _repository.GetAllAsync(filtro, orderBy, ascending, pagina, quantidade);

            var listaDTO = resultado.Itens.Select(t => new TipoDocumentoDTO
            {
                Id = t.Id,
                Nome = t.Nome,
                Descricao = t.Descricao,
                Ativo = t.Ativo
            }).ToList();

            resposta.Sucesso = true;
            resposta.Mensagem = "Lista carregada com sucesso";
            resposta.Dados = new ResultadoPaginado<TipoDocumentoDTO>
            {
                Itens = listaDTO,
                TotalItens = resultado.TotalItens
            };
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Erro ao listar tipos de documento: {ex.Message}";
            resposta.Erros.Add(ex.Message);
        }

        return resposta;
    }

    public async Task<RespostaDTO<TipoDocumentoDTO>> ObterPorIdAsync(Guid id)
    {
        var resposta = new RespostaDTO<TipoDocumentoDTO>();

        try
        {
            var tipoDocumento = await _repository.GetByIdAsync(id);
            if (tipoDocumento == null)
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Tipo de documento não encontrado";
                return resposta;
            }

            resposta.Sucesso = true;
            resposta.Mensagem = "Tipo de documento encontrado";
            resposta.Dados = new TipoDocumentoDTO
            {
                Id = tipoDocumento.Id,
                Nome = tipoDocumento.Nome,
                Descricao = tipoDocumento.Descricao,
                Ativo = tipoDocumento.Ativo
            };
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Erro ao obter tipo de documento: {ex.Message}";
            resposta.Erros.Add(ex.Message);
        }

        return resposta;
    }
}

