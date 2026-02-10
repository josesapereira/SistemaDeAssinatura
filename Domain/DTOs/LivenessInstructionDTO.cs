using System.Text.Json.Serialization;

namespace Domain.DTOs;

public class LivenessInstructionDTO
{
    public string? UltimaInstrucao { get; set; }
}

public class LivenessInstructionRespostaDTO
{
    [JsonPropertyName("instrucao")]
    public string Instrucao { get; set; } = string.Empty;
    
    [JsonPropertyName("acao")]
    public string Acao { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}
