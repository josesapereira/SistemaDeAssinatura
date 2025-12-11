using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Domain.DTOs;
using System.Collections.Generic;

namespace SistemaGestaoDeAssinatura.Components.Pages.Autencicacao
{
    public partial class Login
    {
        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        //[Inject]
        //protected HttpClient HttpClient { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [CascadingParameter]
        public Task<AuthenticationState> authenticationState { get; set; }

        protected string redirectUrl = "";
        protected string error = "";
        protected LoginDTO loginDTO = new();
        protected bool carregando = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await Iniciar();
                }
                catch (Exception ex)
                {
                   
                }
            }
        }

        private async Task Iniciar()
        {
            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(new Uri(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).ToString()).Query);
                var integracao = query.Get("integracao");

                if (integracao != null && integracao != "")
                {
                    try
                    {
                        var navigateUri = $"Account/Login2?Integracao={integracao}";
                        NavigationManager.NavigateTo(navigateUri, true);
                        return;
                    }
                    catch (NavigationException ex)
                    {
                        try
                        {
                            var caminho = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                            if (!Directory.Exists(caminho))
                            {
                                Directory.CreateDirectory(caminho);
                            }
                            var arquivo = Path.Combine(caminho, $"{DateTime.Now.ToString("ddMMyyyy")}.txt");
                            bool arquivoExiste = File.Exists(arquivo);
                            using (StreamWriter sw = new StreamWriter(arquivo, arquivoExiste))
                            {
                                sw.WriteLine("ID: " + Guid.NewGuid().ToString());
                                sw.WriteLine("Data e Hora: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                                sw.WriteLine("Classe: " + "Logim");
                                sw.WriteLine("Usuario: " + "");
                                if (ex.InnerException != null)
                                {
                                    sw.WriteLine("Mensagem: " + ex.InnerException.Message);
                                    sw.WriteLine("StackTrace: " + ex.InnerException.StackTrace);
                                }
                                else
                                {
                                    sw.WriteLine("Mensagem: " + ex.Message);
                                    sw.WriteLine("StackTrace: " + ex.StackTrace);
                                }

                                sw.WriteLine("");
                                sw.Flush();
                            }
                        }
                        catch (Exception) { }
                    }
                }
                var usuario = await authenticationState;

                //if (usuario.User.Identity.IsAuthenticated)
                //{
                //    var response = await HttpClient.GetAsync(NavigationManager.BaseUri + "Account/Logout");
                //}

                error = (query.Get("error") ?? "").Replace("invalido", "inválido").Replace("Usuario", "Usuário") ?? "";
                redirectUrl = query.Get("redirectUrl") ?? "";
            }
            catch (Exception)
            {
                throw;
            }

            StateHasChanged();
        }

        //private async Task OnSubmit()
        //{
        //    carregando = true;
        //    error = string.Empty;

        //    try
        //    {
        //        var url = string.IsNullOrEmpty(redirectUrl) 
        //            ? "api/auth/login" 
        //            : $"api/auth/login?redirectUrl={redirectUrl}";
                    
        //        var response = await HttpClient.PostAsJsonAsync(url, loginDTO);
                
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var resultado = await response.Content.ReadFromJsonAsync<RespostaDTO<object>>();

        //            if (resultado != null && resultado.Sucesso)
        //            {
        //                // Verificar se há um RedirectTo nos dados
        //                string redirectTo = "/home";
        //                if (resultado.Dados != null)
        //                {
        //                    var dados = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(resultado.Dados.ToString() ?? "{}");
        //                    if (dados != null && dados.ContainsKey("RedirectTo"))
        //                    {
        //                        redirectTo = dados["RedirectTo"]?.ToString() ?? "/home";
        //                    }
        //                }
                        
        //                // Recarregar a página para aplicar autenticação
        //                NavigationManager.NavigateTo(redirectTo, forceLoad: true);
        //            }
        //            else
        //            {
        //                error = resultado?.Mensagem ?? "Usuário ou senha inválidos";
        //            }
        //        }
        //        else
        //        {
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            error = $"Erro ao fazer login: {response.StatusCode}";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        error = $"Erro ao fazer login: {ex.Message}";
        //    }
        //    finally
        //    {
        //        carregando = false;
        //        StateHasChanged();
        //    }
        //}
    }
}
