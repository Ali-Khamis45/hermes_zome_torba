namespace HermesZoneTorba.Domain.HermesManagement;

/// <summary>Which LLM runtime a HermesInstance targets. See docs/07-local-llm.md#provider-abstraction.</summary>
public enum ModelProvider
{
    Ollama,
    LmStudio,
    VLlm,
    OpenAiCompatible,
    Anthropic,
    Gemini
}
