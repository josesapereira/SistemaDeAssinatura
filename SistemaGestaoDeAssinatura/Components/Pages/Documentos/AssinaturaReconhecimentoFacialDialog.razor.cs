using Domain.DTOs;
using Domain.Enums;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Infraestrutura.Contexto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Security.Cryptography;
using System.Text;

namespace SistemaGestaoDeAssinatura.Components.Pages.Documentos;

public partial class AssinaturaReconhecimentoFacialDialog : ComponentBase
{
    [Inject]
    public IDeepFaceService DeepFaceService { get; set; } = null!;

    [Inject]
    public IDocumentoRepository DocumentoRepository { get; set; } = null!;

    [Inject]
    public IFileStorageService FileStorageService { get; set; } = null!;

    [Inject]
    public IUsuarioRepository UsuarioRepository { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    public AppDbContext DbContext { get; set; } = null!;

    [Parameter]
    public Guid DocumentoId { get; set; }

    [Parameter]
    public long UsuarioId { get; set; }

    private enum EtapaAssinatura
    {
        Instrucao,
        Captura,
        Processando,
        Sucesso,
        Erro
    }

    private EtapaAssinatura etapa = EtapaAssinatura.Instrucao;
    private string instrucaoAtual = string.Empty;
    private string acaoAtual = string.Empty;
    private bool processando = false;
    private string mensagemProcessamento = string.Empty;
    private string mensagemErro = string.Empty;
    private byte[]? fotoCapturada = null;
    private List<LivenessFotoInstrucaoDTO> fotosCapturadas = new();
    private List<LivenessInstructionRespostaDTO> instrucoesGeradas = new();
    private int fotoAtualIndex = 0;
    private const int NUMERO_FOTOS_REQUERIDAS = 3; // Número de fotos para prova de vida

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Iniciar câmera imediatamente quando o diálogo abrir
            await JSRuntime.InvokeVoidAsync("iniciarCamera");
            
            // Depois iniciar o processo de geração de instruções
            await IniciarProcesso();
        }
    }

    private async Task IniciarProcesso()
    {
        try
        {
            etapa = EtapaAssinatura.Instrucao;
            fotosCapturadas.Clear();
            instrucoesGeradas.Clear();
            fotoAtualIndex = 0;
            StateHasChanged();

            // Gerar múltiplas instruções aleatórias de prova de vida
            string? ultimaInstrucao = null;
            for (int i = 0; i < NUMERO_FOTOS_REQUERIDAS; i++)
            {
                var resultadoInstrucao = await DeepFaceService.GerarInstrucaoProvaVidaAsync(ultimaInstrucao);
                
                if (!resultadoInstrucao.Sucesso || resultadoInstrucao.Dados == null)
                {
                    etapa = EtapaAssinatura.Erro;
                    mensagemErro = resultadoInstrucao.Mensagem;
                    StateHasChanged();
                    return;
                }

                instrucoesGeradas.Add(resultadoInstrucao.Dados);
                ultimaInstrucao = resultadoInstrucao.Dados.Acao;
            }

            // Configurar primeira instrução
            if (instrucoesGeradas.Count > 0)
            {
                instrucaoAtual = instrucoesGeradas[0].Instrucao;
                acaoAtual = instrucoesGeradas[0].Acao;
            }

            // Mudar para etapa de captura (câmera já está aberta)
            etapa = EtapaAssinatura.Captura;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            etapa = EtapaAssinatura.Erro;
            mensagemErro = $"Erro ao iniciar processo: {ex.Message}";
            StateHasChanged();
        }
    }

    private async Task CapturarFoto()
    {
        try
        {
            processando = true;
            StateHasChanged();

            // Capturar foto via JavaScript
            var fotoBase64 = await JSRuntime.InvokeAsync<string>("capturarFoto");
            
            if (string.IsNullOrEmpty(fotoBase64))
            {
                throw new Exception("Não foi possível capturar a foto");
            }

            // Converter base64 para byte[]
            var base64Data = fotoBase64.Split(',')[1];
            var fotoBytes = Convert.FromBase64String(base64Data);

            // Adicionar foto à lista com sua instrução
            fotosCapturadas.Add(new LivenessFotoInstrucaoDTO
            {
                Imagem = fotoBytes,
                Instrucao = acaoAtual
            });

            fotoAtualIndex++;

            // Verificar se já capturou todas as fotos necessárias
            if (fotoAtualIndex >= NUMERO_FOTOS_REQUERIDAS)
            {
                // Todas as fotos capturadas, validar todas de uma vez
                await ValidarProvaVidaMultipla();
            }
            else
            {
                // Atualizar instrução para próxima foto
                if (fotoAtualIndex < instrucoesGeradas.Count)
                {
                    instrucaoAtual = instrucoesGeradas[fotoAtualIndex].Instrucao;
                    acaoAtual = instrucoesGeradas[fotoAtualIndex].Acao;
                }
                
                processando = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            processando = false;
            etapa = EtapaAssinatura.Erro;
            mensagemErro = $"Erro ao capturar foto: {ex.Message}";
            StateHasChanged();
        }
    }

    private async Task ValidarProvaVidaMultipla()
    {
        try
        {
            mensagemProcessamento = "Validando prova de vida com múltiplas fotos...";
            etapa = EtapaAssinatura.Processando;
            StateHasChanged();

            var validacaoMultiplaDTO = new LivenessValidationMultiplaDTO
            {
                Fotos = fotosCapturadas
            };

            var resultadoValidacao = await DeepFaceService.ValidarProvaVidaMultiplaAsync(validacaoMultiplaDTO);

            if (!resultadoValidacao.Sucesso || resultadoValidacao.Dados == null)
            {
                throw new Exception(resultadoValidacao.Mensagem);
            }

            if (!resultadoValidacao.Dados.Aprovado)
            {
                var detalhes = $"Prova de vida não aprovada. {resultadoValidacao.Dados.FotosAprovadas} de {resultadoValidacao.Dados.TotalFotos} fotos foram aprovadas.";
                throw new Exception(detalhes);
            }

            // Usar a primeira foto capturada para verificação de identidade
            fotoCapturada = fotosCapturadas[0].Imagem;

            // Prova de vida aprovada, continuar com verificação de identidade
            await VerificarIdentidade();
        }
        catch (Exception ex)
        {
            processando = false;
            etapa = EtapaAssinatura.Erro;
            mensagemErro = ex.Message;
            StateHasChanged();
        }
    }

    private async Task VerificarIdentidade()
    {
        try
        {
            mensagemProcessamento = "Verificando identidade...";
            StateHasChanged();

            // Buscar foto cadastrada do usuário (se existir)
            var usuario = await UsuarioRepository.GetByIdAsync(UsuarioId);
            if (usuario == null)
            {
                throw new Exception("Usuário não encontrado");
            }

            // Se não houver foto cadastrada, pular verificação de identidade
            // (ou implementar cadastro de foto primeiro)
            if (string.IsNullOrEmpty(usuario.NomeDoArquivo))
            {
                // Pular verificação de identidade e prosseguir com assinatura
                await ProcessarAssinatura();
                return;
            }

            // Buscar foto cadastrada
            var fotoCadastrada = await FileStorageService.LerArquivoAsync(usuario.NomeDoArquivo);
            if (fotoCadastrada == null)
            {
                // Foto cadastrada não encontrada, prosseguir sem verificação
                await ProcessarAssinatura();
                return;
            }

            // Verificar identidade
            var verifyDTO = new DeepFaceVerifyDTO
            {
                Imagem1 = fotoCadastrada,
                Imagem2 = fotoCapturada
            };

            var resultadoVerificacao = await DeepFaceService.VerificarAsync(verifyDTO);

            if (!resultadoVerificacao.Sucesso || resultadoVerificacao.Dados == null)
            {
                throw new Exception($"Erro ao verificar identidade: {resultadoVerificacao.Mensagem}");
            }

            if (!resultadoVerificacao.Dados.Verificado)
            {
                throw new Exception($"Identidade não verificada. As faces não correspondem. Distância: {resultadoVerificacao.Dados.Distancia:F4}");
            }

            // Identidade verificada, processar assinatura
            await ProcessarAssinatura();
        }
        catch (Exception ex)
        {
            processando = false;
            etapa = EtapaAssinatura.Erro;
            mensagemErro = ex.Message;
            StateHasChanged();
        }
    }

    private async Task ProcessarAssinatura()
    {
        try
        {
            mensagemProcessamento = "Processando assinatura...";
            StateHasChanged();

            // Buscar documento e assinante
            var documento = await DocumentoRepository.GetByIdComRelacionamentosAsync(DocumentoId);
            if (documento == null)
            {
                throw new Exception("Documento não encontrado");
            }

            var assinante = documento.Assinantes.FirstOrDefault(a => a.AssinanteId == UsuarioId);
            if (assinante == null)
            {
                throw new Exception("Você não é um assinante deste documento");
            }

            // Determinar tipo de assinatura baseado no status do documento
            var tipoAssinatura = documento.StatusDocumento == StatusDocumento.Aguardando_assinatura
                ? TipoAssinatura.Validacao
                : TipoAssinatura.Revogacao;

            // Salvar foto de reconhecimento facial
            var nomeArquivoFoto = $"{Guid.NewGuid()}.jpg";
            await FileStorageService.SalvarArquivoAsync(fotoCapturada!, nomeArquivoFoto);

            // Gerar hash da foto
            string hashFoto;
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(fotoCapturada!);
                hashFoto = Convert.ToBase64String(hashBytes);
            }

            // Gerar hash do documento assinado (usar hash original do documento)
            var hashDocumento = documento.HashDocumentoOriginal;

            // Obter informações do sistema
            var sistemaOperacional = await JSRuntime.InvokeAsync<string>("navigator.platform");
            var userAgent = await JSRuntime.InvokeAsync<string>("navigator.userAgent");

            // Criar assinatura
            var assinatura = new Assinatura
            {
                DataDaAssinatura = DateTime.Now,
                TipoAssinatura = tipoAssinatura,
                AssinanteId = assinante.Id,
                HashDocumentoAssinado = hashDocumento,
                HashNomeArquivoReconhecimentoFacial = hashFoto,
                NomeArquivoReconhecimentoFacial = nomeArquivoFoto,
                Latitude = 0, // TODO: Obter geolocalização se necessário
                Longitude = 0, // TODO: Obter geolocalização se necessário
                SistemaDeAssinatura = "Sistema Gestão de Assinaturas",
                VersaoSistema = "1.0.0",
                SistemaOperacional = sistemaOperacional ?? "Desconhecido",
                IPDaAssinatura = string.Empty, // TODO: Obter IP se necessário
                DocumentoId = DocumentoId
            };

            // Adicionar assinatura usando DbContext
            await DbContext.Set<Assinatura>().AddAsync(assinatura);
            await DbContext.SaveChangesAsync();

            // Verificar se todos os assinantes assinaram
            var todasAssinaturas = await DocumentoRepository.ObterAssinaturasPorDocumentoAsync(DocumentoId, TipoAssinatura.Validacao);
            var todosAssinantesAssinaram = documento.Assinantes.All(a => 
                todasAssinaturas.Any(ass => ass.AssinanteId == a.Id));

            if (todosAssinantesAssinaram && documento.StatusDocumento == StatusDocumento.Aguardando_assinatura)
            {
                documento.StatusDocumento = StatusDocumento.Assinado;
                await DocumentoRepository.AtualizarAsync(documento);
            }

            // Parar câmera
            await JSRuntime.InvokeVoidAsync("pararCamera");

            etapa = EtapaAssinatura.Sucesso;
            processando = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            processando = false;
            etapa = EtapaAssinatura.Erro;
            mensagemErro = $"Erro ao processar assinatura: {ex.Message}";
            StateHasChanged();
        }
    }

    private async Task TentarNovamente()
    {
        await JSRuntime.InvokeVoidAsync("pararCamera");
        await IniciarProcesso();
    }

    private async Task Cancelar()
    {
        await JSRuntime.InvokeVoidAsync("pararCamera");
        DialogService.Close(false);
    }

    private void Fechar()
    {
        DialogService.Close(true);
    }
}
