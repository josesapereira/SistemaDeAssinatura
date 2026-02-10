using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Domain.DTOs;
using Domain.Interfaces.Service;
using Microsoft.Extensions.Configuration;

namespace Service.Implementacoes;

public class DeepFaceService : IDeepFaceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly int _timeoutSeconds;

    public DeepFaceService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Obter URL do serviço DeepFace do appsettings.json
        _baseUrl = _configuration["DeepFace:BaseUrl"] ?? "http://localhost:8000";
        _timeoutSeconds = int.Parse(_configuration["DeepFace:TimeoutSeconds"] ?? "30");
        
        // Configurar timeout do HttpClient
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        
        // Remover barra final se existir
        if (_baseUrl.EndsWith("/"))
        {
            _baseUrl = _baseUrl.TrimEnd('/');
        }
    }

    private string ConvertToBase64(byte[]? image)
    {
        if (image == null || image.Length == 0)
            throw new ArgumentException("Imagem não pode ser nula ou vazia");

        return Convert.ToBase64String(image);
    }

    private async Task<RespostaDTO<T>> ExecuteRequestAsync<T>(
        Func<Task<HttpResponseMessage>> requestFunc,
        string errorMessage)
    {
        var resposta = new RespostaDTO<T>();

        try
        {
            var response = await requestFunc();
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var data = JsonSerializer.Deserialize<T>(content, options);
                    
                    resposta.Sucesso = true;
                    resposta.Mensagem = "Operação realizada com sucesso";
                    resposta.Dados = data;
                }
                catch (JsonException ex)
                {
                    resposta.Sucesso = false;
                    resposta.Mensagem = $"Erro ao deserializar resposta: {ex.Message}";
                    resposta.Erros.Add(ex.Message);
                }
            }
            else
            {
                resposta.Sucesso = false;
                resposta.Mensagem = $"{errorMessage}. Status: {response.StatusCode}";
                
                // Tentar extrair mensagem de erro do JSON
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                    if (errorResponse != null && errorResponse.ContainsKey("detail"))
                    {
                        resposta.Mensagem = errorResponse["detail"]?.ToString() ?? resposta.Mensagem;
                        resposta.Erros.Add(resposta.Mensagem);
                    }
                }
                catch
                {
                    resposta.Erros.Add(content);
                }
            }
        }
        catch (TaskCanceledException)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Timeout: A requisição excedeu o tempo limite de {_timeoutSeconds} segundos";
            resposta.Erros.Add("Timeout na requisição");
        }
        catch (HttpRequestException ex)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Erro de comunicação com o serviço DeepFace: {ex.Message}";
            resposta.Erros.Add(ex.Message);
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = $"Erro inesperado: {ex.Message}";
            resposta.Erros.Add(ex.Message);
        }

        return resposta;
    }

    public async Task<RespostaDTO<DeepFaceVerifyRespostaDTO>> VerificarAsync(DeepFaceVerifyDTO dto)
    {
        if (dto.Imagem1 == null || dto.Imagem2 == null)
        {
            return new RespostaDTO<DeepFaceVerifyRespostaDTO>
            {
                Sucesso = false,
                Mensagem = "É necessário fornecer duas imagens para verificação",
                Erros = { "Imagem1 e Imagem2 são obrigatórias" }
            };
        }

        return await ExecuteRequestAsync<DeepFaceVerifyRespostaDTO>(
            async () =>
            {
                var formData = new MultipartFormDataContent();
                
                var img1Base64 = ConvertToBase64(dto.Imagem1);
                var img2Base64 = ConvertToBase64(dto.Imagem2);
                
                formData.Add(new StringContent(img1Base64), "img1_base64");
                formData.Add(new StringContent(img2Base64), "img2_base64");
                formData.Add(new StringContent(dto.Modelo ?? "VGG-Face"), "model_name");
                formData.Add(new StringContent(dto.MetricaDistancia ?? "cosine"), "distance_metric");

                return await _httpClient.PostAsync($"{_baseUrl}/api/deepface/verify", formData);
            },
            "Erro ao verificar faces"
        );
    }

    public async Task<RespostaDTO<DeepFaceAnalyzeRespostaDTO>> AnalisarAsync(DeepFaceAnalyzeDTO dto)
    {
        if (dto.Imagem == null)
        {
            return new RespostaDTO<DeepFaceAnalyzeRespostaDTO>
            {
                Sucesso = false,
                Mensagem = "É necessário fornecer uma imagem para análise",
                Erros = { "Imagem é obrigatória" }
            };
        }

        return await ExecuteRequestAsync<DeepFaceAnalyzeRespostaDTO>(
            async () =>
            {
                var formData = new MultipartFormDataContent();
                
                var imgBase64 = ConvertToBase64(dto.Imagem);
                formData.Add(new StringContent(imgBase64), "img_base64");
                
                if (dto.Acoes != null && dto.Acoes.Any())
                {
                    var actionsString = string.Join(",", dto.Acoes);
                    formData.Add(new StringContent(actionsString), "actions");
                }

                return await _httpClient.PostAsync($"{_baseUrl}/api/deepface/analyze", formData);
            },
            "Erro ao analisar face"
        );
    }

    public async Task<RespostaDTO<DeepFaceDetectRespostaDTO>> DetectarAsync(DeepFaceDetectDTO dto)
    {
        if (dto.Imagem == null)
        {
            return new RespostaDTO<DeepFaceDetectRespostaDTO>
            {
                Sucesso = false,
                Mensagem = "É necessário fornecer uma imagem para detecção",
                Erros = { "Imagem é obrigatória" }
            };
        }

        return await ExecuteRequestAsync<DeepFaceDetectRespostaDTO>(
            async () =>
            {
                var formData = new MultipartFormDataContent();
                
                var imgBase64 = ConvertToBase64(dto.Imagem);
                formData.Add(new StringContent(imgBase64), "img_base64");

                return await _httpClient.PostAsync($"{_baseUrl}/api/deepface/detect", formData);
            },
            "Erro ao detectar faces"
        );
    }

    public async Task<RespostaDTO<LivenessInstructionRespostaDTO>> GerarInstrucaoProvaVidaAsync(string? ultimaInstrucao = null)
    {
        return await ExecuteRequestAsync<LivenessInstructionRespostaDTO>(
            async () =>
            {
                // FastAPI aceita Form(None), então podemos enviar o campo mesmo que vazio
                // O FastAPI tratará string vazia como None internamente
                var formData = new MultipartFormDataContent();
                
                // Sempre adicionar o campo (mesmo vazio) para garantir compatibilidade
                // O endpoint Python trata string vazia como None: last_instruction if last_instruction and last_instruction.strip() else None
                var lastInstructionValue = ultimaInstrucao ?? string.Empty;
                formData.Add(new StringContent(lastInstructionValue), "last_instruction");

                return await _httpClient.PostAsync($"{_baseUrl}/api/deepface/liveness/generate-instruction", formData);
            },
            "Erro ao gerar instrução de prova de vida"
        );
    }

    public async Task<RespostaDTO<LivenessValidationRespostaDTO>> ValidarProvaVidaAsync(LivenessValidationDTO dto)
    {
        if (dto.Imagem == null)
        {
            return new RespostaDTO<LivenessValidationRespostaDTO>
            {
                Sucesso = false,
                Mensagem = "É necessário fornecer uma imagem para validação de prova de vida",
                Erros = { "Imagem é obrigatória" }
            };
        }

        if (string.IsNullOrWhiteSpace(dto.Instrucao))
        {
            return new RespostaDTO<LivenessValidationRespostaDTO>
            {
                Sucesso = false,
                Mensagem = "É necessário fornecer a instrução esperada",
                Erros = { "Instrução é obrigatória" }
            };
        }

        // Validar instrução
        var instrucoesValidas = new[] { "piscar", "sorrir", "virar_esquerda", "virar_direita" };
        if (!instrucoesValidas.Contains(dto.Instrucao.ToLower()))
        {
            return new RespostaDTO<LivenessValidationRespostaDTO>
            {
                Sucesso = false,
                Mensagem = $"Instrução inválida. Instruções válidas: {string.Join(", ", instrucoesValidas)}",
                Erros = { "Instrução inválida" }
            };
        }

        return await ExecuteRequestAsync<LivenessValidationRespostaDTO>(
            async () =>
            {
                var formData = new MultipartFormDataContent();
                
                var imgBase64 = ConvertToBase64(dto.Imagem);
                formData.Add(new StringContent(imgBase64), "img_base64");
                formData.Add(new StringContent(dto.Instrucao), "instruction");

                return await _httpClient.PostAsync($"{_baseUrl}/api/deepface/liveness/validate", formData);
            },
            "Erro ao validar prova de vida"
        );
    }

    public async Task<RespostaDTO<LivenessValidationMultiplaRespostaDTO>> ValidarProvaVidaMultiplaAsync(LivenessValidationMultiplaDTO dto)
    {
        if (dto.Fotos == null || !dto.Fotos.Any())
        {
            return new RespostaDTO<LivenessValidationMultiplaRespostaDTO>
            {
                Sucesso = false,
                Mensagem = "É necessário fornecer pelo menos uma foto para validação de prova de vida",
                Erros = { "Fotos são obrigatórias" }
            };
        }

        // Validar instruções
        var instrucoesValidas = new[] { "piscar", "sorrir", "virar_esquerda", "virar_direita" };
        foreach (var foto in dto.Fotos)
        {
            if (foto.Imagem == null || foto.Imagem.Length == 0)
            {
                return new RespostaDTO<LivenessValidationMultiplaRespostaDTO>
                {
                    Sucesso = false,
                    Mensagem = "Todas as fotos devem conter uma imagem válida",
                    Erros = { "Imagem inválida encontrada" }
                };
            }

            if (string.IsNullOrWhiteSpace(foto.Instrucao) || !instrucoesValidas.Contains(foto.Instrucao.ToLower()))
            {
                return new RespostaDTO<LivenessValidationMultiplaRespostaDTO>
                {
                    Sucesso = false,
                    Mensagem = $"Instrução inválida encontrada. Instruções válidas: {string.Join(", ", instrucoesValidas)}",
                    Erros = { "Instrução inválida" }
                };
            }
        }

        return await ExecuteRequestAsync<LivenessValidationMultiplaRespostaDTO>(
            async () =>
            {
                var formData = new MultipartFormDataContent();
                
                // Adicionar cada foto com sua instrução
                // Usar formato sem colchetes para compatibilidade com FastAPI
                for (int i = 0; i < dto.Fotos.Count; i++)
                {
                    var foto = dto.Fotos[i];
                    var imgBase64 = ConvertToBase64(foto.Imagem);
                    
                    // Usar formato de nome de campo sem colchetes
                    formData.Add(new StringContent(imgBase64), $"fotos_{i}_img_base64");
                    formData.Add(new StringContent(foto.Instrucao), $"fotos_{i}_instruction");
                }
                
                // Adicionar contador total de fotos
                formData.Add(new StringContent(dto.Fotos.Count.ToString()), "total_fotos");

                return await _httpClient.PostAsync($"{_baseUrl}/api/deepface/liveness/validate-multiple", formData);
            },
            "Erro ao validar prova de vida múltipla"
        );
    }
}
