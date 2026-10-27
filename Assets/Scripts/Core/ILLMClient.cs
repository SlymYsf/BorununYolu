using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BorununYolu.Core
{
    /// <summary>LLM isteğinin sonucunu oyunun anlayacağı biçimde özetler.</summary>
    public enum LLMStatus
    {
        Success,
        ServerUnreachable, // Ollama kapalı / bağlantı yok
        Timeout,           // Yanıt süresi aşıldı
        ModelNotFound,     // Model yüklü değil
        InvalidResponse,   // Yanıt geldi ama JSON bozuk / beklenen alanlar yok
        Cancelled          // İstek oyun tarafından iptal edildi (sahne kapandı vb.)
    }

    public sealed class LLMResult
    {
        public LLMStatus Status { get; }
        public string RawText { get; }
        public string ErrorMessage { get; }

        public bool IsSuccess => Status == LLMStatus.Success;

        private LLMResult(LLMStatus status, string rawText, string errorMessage)
        {
            Status = status;
            RawText = rawText;
            ErrorMessage = errorMessage;
        }

        public static LLMResult Ok(string rawText) => new LLMResult(LLMStatus.Success, rawText, null);
        public static LLMResult Fail(LLMStatus status, string errorMessage) => new LLMResult(status, null, errorMessage);
    }

    /// <summary>Konuşma geçmişindeki tek bir mesaj.</summary>
    public sealed class ChatMessage
    {
        public string Role { get; }    // "system", "user", "assistant"
        public string Content { get; }

        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    /// <summary>
    /// Yerel dil modeli istemcisi için ortak sözleşme.
    /// OllamaClient bunu uygular; oyunun geri kalanı Ollama'yı doğrudan bilmez.
    /// Uygulama hiçbir durumda exception fırlatmamalı, hatayı LLMResult ile bildirmelidir.
    /// </summary>
    public interface ILLMClient
    {
        Task<LLMResult> SendAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
    }
}
