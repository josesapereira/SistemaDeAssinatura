using Domain.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace SistemaGestaoDeAssinatura.Components.Pages.Autencicacao;

public partial class AlterarSenha : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    private AlterarSenhaDTO alterarSenhaDTO = new();
    private string mensagemErro = string.Empty;
    private string mensagemSucesso = string.Empty;
    private bool carregando = false;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated ?? true)
        {
            Navigation.NavigateTo("/", forceLoad: true);
        }
    }

    private async Task OnSubmit()
    {
        carregando = true;
        mensagemErro = string.Empty;
        mensagemSucesso = string.Empty;

        try
        {
            var response = await Http.PostAsJsonAsync("api/Account/AlterarSenha", alterarSenhaDTO);

            // Verificar se a resposta foi bem-sucedida
            if (response.IsSuccessStatusCode)
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                
                var resultado = await response.Content.ReadFromJsonAsync<RespostaDTO<object>>(options);

                if (resultado != null && resultado.Sucesso)
                {
                    mensagemSucesso = resultado.Mensagem;

                    // Aguardar um pouco antes de redirecionar
                    await Task.Delay(1500);

                    // Recarregar a página para aplicar autenticação
                    Navigation.NavigateTo("/ativar-2fa", forceLoad: true);
                }
                else
                {
                    mensagemErro = resultado?.Mensagem ?? "Erro ao alterar senha";
                    if (resultado?.Erros != null && resultado.Erros.Any())
                    {
                        mensagemErro += ": " + string.Join(", ", resultado.Erros);
                    }
                }
            }
            else
            {
                // Tentar ler a resposta como JSON primeiro
                var contentString = await response.Content.ReadAsStringAsync();

                // Verificar se o conteúdo é JSON válido (começa com { ou [)
                if (!string.IsNullOrWhiteSpace(contentString) && 
                    (contentString.TrimStart().StartsWith("{") || contentString.TrimStart().StartsWith("[")))
                {
                    try
                    {
                        var options = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                        };
                        
                        var resultado = System.Text.Json.JsonSerializer.Deserialize<RespostaDTO<object>>(contentString, options);
                        mensagemErro = resultado?.Mensagem ?? $"Erro: {response.StatusCode}";
                        if (resultado?.Erros != null && resultado.Erros.Any())
                        {
                            mensagemErro += ": " + string.Join(", ", resultado.Erros);
                        }
                    }
                    catch (System.Text.Json.JsonException jsonEx)
                    {
                        // Se falhar na deserialização, mostrar o conteúdo e o erro
                        mensagemErro = $"Erro ao processar resposta do servidor. Status: {response.StatusCode}";
                        if (contentString.Length > 200)
                        {
                            mensagemErro += $"\nResposta: {contentString.Substring(0, 200)}...";
                        }
                        else
                        {
                            mensagemErro += $"\nResposta: {contentString}";
                        }
                    }
                }
                else
                {
                    // Se não for JSON, mostrar mensagem genérica
                    mensagemErro = $"Erro ao alterar senha. Status: {response.StatusCode}";
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        mensagemErro = "Sessão expirada. Por favor, faça login novamente.";
                        Navigation.NavigateTo("/", forceLoad: true);
                    }
                    else if (!string.IsNullOrWhiteSpace(contentString) && contentString.Length < 500)
                    {
                        mensagemErro += $"\n{contentString}";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            mensagemErro = $"Erro: {ex.Message}";
        }
        finally
        {
            carregando = false;
        }

        StateHasChanged();
    }
}

