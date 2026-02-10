using Domain.DTOs;

namespace Domain.Interfaces.Service;

public interface IDeepFaceService
{
    /// <summary>
    /// Verifica se duas faces pertencem à mesma pessoa
    /// </summary>
    Task<RespostaDTO<DeepFaceVerifyRespostaDTO>> VerificarAsync(DeepFaceVerifyDTO dto);

    /// <summary>
    /// Analisa atributos faciais: emoções, idade, gênero, raça
    /// </summary>
    Task<RespostaDTO<DeepFaceAnalyzeRespostaDTO>> AnalisarAsync(DeepFaceAnalyzeDTO dto);

    /// <summary>
    /// Detecta faces em uma imagem
    /// </summary>
    Task<RespostaDTO<DeepFaceDetectRespostaDTO>> DetectarAsync(DeepFaceDetectDTO dto);

    /// <summary>
    /// Gera uma instrução aleatória de prova de vida
    /// </summary>
    Task<RespostaDTO<LivenessInstructionRespostaDTO>> GerarInstrucaoProvaVidaAsync(string? ultimaInstrucao = null);

    /// <summary>
    /// Valida prova de vida em uma foto
    /// </summary>
    Task<RespostaDTO<LivenessValidationRespostaDTO>> ValidarProvaVidaAsync(LivenessValidationDTO dto);

    /// <summary>
    /// Valida prova de vida em múltiplas fotos com instruções aleatórias
    /// </summary>
    Task<RespostaDTO<LivenessValidationMultiplaRespostaDTO>> ValidarProvaVidaMultiplaAsync(LivenessValidationMultiplaDTO dto);
}
