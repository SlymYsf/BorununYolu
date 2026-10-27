using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BorununYolu.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BorununYolu.LLM
{
    /// <summary>
    /// Yerelde çalışan Ollama sunucusuna /api/chat üzerinden istek atan istemci.
    /// - İstekler asenkron çalışır, oyun donmaz.
    /// - Modelden JSON formatında yanıt istenir (format: "json").
    /// - Düşünme (thinking) modu kapalıdır; aksi halde küçük modeller dakikalarca bekletebilir.
    /// - Hiçbir durumda exception fırlatmaz; her hata LLMResult içinde LLMStatus ile döner.
    /// </summary>
    public sealed class OllamaClient : ILLMClient
    {
        // HttpClient tek örnek olarak paylaşılır (her istekte yeni oluşturmak soket sızıntısı yapar).
        // Zaman aşımını kendimiz CancellationToken ile yönettiğimiz için buradaki süre sınırsız.
        private static readonly HttpClient Http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        private readonly string _chatUrl;
        private readonly string _model;
        private readonly TimeSpan _timeout;
        private readonly float _temperature;

        public string Model => _model;

        public OllamaClient(
            string baseUrl = "http://localhost:11434",
            string model = "qwen3.5:4b",
            float timeoutSeconds = 30f,
            float temperature = 0.7f)
        {
            _chatUrl = baseUrl.TrimEnd('/') + "/api/chat";
            _model = model;
            _timeout = TimeSpan.FromSeconds(Math.Max(1f, timeoutSeconds));
            _temperature = temperature;
        }

        public async Task<LLMResult> SendAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            if (messages == null || messages.Count == 0)
                return LLMResult.Fail(LLMStatus.InvalidResponse, "Modele gönderilecek mesaj yok.");

            string requestJson = BuildRequestJson(messages);

            using (var timeoutCts = new CancellationTokenSource(_timeout))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
            {
                try
                {
                    using (var content = new StringContent(requestJson, Encoding.UTF8, "application/json"))
                    using (var response = await Http.PostAsync(_chatUrl, content, linkedCts.Token))
                    {
                        string body = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                            return HandleErrorStatus(response.StatusCode, body);

                        string messageContent = ExtractMessageContent(body);
                        if (string.IsNullOrWhiteSpace(messageContent))
                            return LLMResult.Fail(LLMStatus.InvalidResponse, "Model boş bir yanıt döndürdü.");

                        return LLMResult.Ok(messageContent);
                    }
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return LLMResult.Fail(LLMStatus.Cancelled, "İstek iptal edildi.");

                    return LLMResult.Fail(LLMStatus.Timeout,
                        $"Model {_timeout.TotalSeconds:0} saniye içinde yanıt vermedi.");
                }
                catch (HttpRequestException)
                {
                    return LLMResult.Fail(LLMStatus.ServerUnreachable,
                        "Ollama sunucusuna ulaşılamadı. Ollama'nın açık olduğundan emin olun.");
                }
                catch (Exception ex)
                {
                    // Beklenmeyen her şey: oyun asla çökmesin.
                    return LLMResult.Fail(LLMStatus.InvalidResponse, "Beklenmeyen hata: " + ex.Message);
                }
            }
        }

        private string BuildRequestJson(IReadOnlyList<ChatMessage> messages)
        {
            var messageArray = new JArray();
            foreach (var m in messages)
            {
                if (m == null) continue;
                messageArray.Add(new JObject
                {
                    ["role"] = m.Role,
                    ["content"] = m.Content ?? string.Empty
                });
            }

            var payload = new JObject
            {
                ["model"] = _model,
                ["messages"] = messageArray,
                ["stream"] = false,
                ["think"] = false,
                ["format"] = "json",
                ["keep_alive"] = "15m", // Model 15 dk bellekte kalsın, her mesajda yeniden yüklenmesin.
                ["options"] = new JObject { ["temperature"] = _temperature }
            };

            return payload.ToString(Formatting.None);
        }

        private LLMResult HandleErrorStatus(HttpStatusCode statusCode, string body)
        {
            string serverError = TryReadErrorField(body);

            bool modelMissing = statusCode == HttpStatusCode.NotFound ||
                                (serverError != null && serverError.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0);

            if (modelMissing)
                return LLMResult.Fail(LLMStatus.ModelNotFound,
                    $"'{_model}' modeli yüklü değil. Komut satırında 'ollama pull {_model}' çalıştırın.");

            return LLMResult.Fail(LLMStatus.InvalidResponse,
                $"Ollama hata döndürdü ({(int)statusCode}): {serverError ?? "ayrıntı yok"}");
        }

        /// <summary>Ollama'nın { "message": { "content": "..." } } yanıtından content alanını alır.</summary>
        private static string ExtractMessageContent(string body)
        {
            try
            {
                var root = JObject.Parse(body);
                return root["message"]?["content"]?.Value<string>();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string TryReadErrorField(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                return JObject.Parse(body)["error"]?.Value<string>();
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
