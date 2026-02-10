using System.Text.Json.Serialization;

namespace Domain.DTOs;

public class LivenessValidationDTO
{
    public byte[]? Imagem { get; set; }
    public string Instrucao { get; set; } = string.Empty; // "piscar", "sorrir", "virar_esquerda", "virar_direita"
}

public class LivenessValidationMultiplaDTO
{
    public List<LivenessFotoInstrucaoDTO> Fotos { get; set; } = new();
}

public class LivenessFotoInstrucaoDTO
{
    public byte[] Imagem { get; set; } = Array.Empty<byte>();
    public string Instrucao { get; set; } = string.Empty; // "piscar", "sorrir", "virar_esquerda", "virar_direita"
}

public class LivenessValidationDetalhesDTO
{
    [JsonPropertyName("instrucao_solicitada")]
    public string InstrucaoSolicitada { get; set; } = string.Empty;
    
    [JsonPropertyName("blink_detectado")]
    public bool BlinkDetectado { get; set; }
    
    [JsonPropertyName("blink_confidence")]
    public double BlinkConfidence { get; set; }
    
    [JsonPropertyName("smile_detectado")]
    public bool SmileDetectado { get; set; }
    
    [JsonPropertyName("smile_confidence")]
    public double SmileConfidence { get; set; }
    
    [JsonPropertyName("head_pose")]
    public string HeadPose { get; set; } = string.Empty;
    
    [JsonPropertyName("head_confidence")]
    public double HeadConfidence { get; set; }
    
    [JsonPropertyName("erro")]
    public string? Erro { get; set; }
}

public class LivenessValidationRespostaDTO
{
    [JsonPropertyName("aprovado")]
    public bool Aprovado { get; set; }
    
    [JsonPropertyName("acao_detectada")]
    public string AcaoDetectada { get; set; } = string.Empty;
    
    [JsonPropertyName("confianca")]
    public double Confianca { get; set; }
    
    [JsonPropertyName("detalhes")]
    public LivenessValidationDetalhesDTO? Detalhes { get; set; }
}

public class LivenessValidationMultiplaRespostaDTO
{
    [JsonPropertyName("aprovado")]
    public bool Aprovado { get; set; }
    
    [JsonPropertyName("total_fotos")]
    public int TotalFotos { get; set; }
    
    [JsonPropertyName("fotos_aprovadas")]
    public int FotosAprovadas { get; set; }
    
    [JsonPropertyName("resultados")]
    public List<LivenessValidationRespostaDTO> Resultados { get; set; } = new();
}
