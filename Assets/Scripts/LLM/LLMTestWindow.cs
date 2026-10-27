using System.Collections.Generic;
using System.Text;
using BorununYolu.Core;
using UnityEngine;

namespace BorununYolu.LLM
{
    /// <summary>
    /// GEÇİCİ TEST ARACI: Asıl diyalog arayüzü yapılana kadar Ollama bağlantısını
    /// sahnede denemek için basit bir pencere. Boş bir GameObject'e ekleyip Play'e basın.
    /// Sağ üstteki kare sayacı, model düşünürken oyunun donmadığını gösterir.
    /// </summary>
    public class LLMTestWindow : MonoBehaviour
    {
        [Header("Ollama")]
        [SerializeField] private string baseUrl = "http://localhost:11434";
        [SerializeField] private string model = "qwen3.5:4b";
        [SerializeField] private float timeoutSeconds = 30f;

        [Header("Geçici system prompt (asıl prompt sonra ayrı dosyaya taşınacak)")]
        [SerializeField, TextArea(4, 10)]
        private string systemPrompt =
            "Sen Deli Dumrul'sun. Kuru derenin üstüne köprü kurdun, geçenden bedel istersin. " +
            "Kaba, kendinden emin, orta çağ Oğuz üslubuyla konuşursun. Yolcuya hep 'sen' diye hitap edersin. " +
            "Yalvaranı sevmezsin, yiğitliği ve köprüsünü öveni seversin. " +
            "Bedel alınmadan ya da gerçekten ikna olmadan kapıyı açmazsın. Hakaret edilirse öfkelenirsin. " +
            "Cevabın en fazla 2 cümle olsun, sahne tarifi yazma. " +
            "Sadece şu JSON'u döndür: {\"cevap\": string, \"kapiyi_ac\": bool, \"kavga_baslat\": bool, \"ofke\": 0-10 arası tam sayı}";

        private ILLMClient _client;
        private readonly List<ChatMessage> _history = new List<ChatMessage>();
        private readonly StringBuilder _log = new StringBuilder();
        private string _input = string.Empty;
        private bool _waiting;
        private float _waitStart;
        private Vector2 _scroll;

        private void Awake()
        {
            _client = new OllamaClient(baseUrl, model, timeoutSeconds);
            ResetConversation();
        }

        private void ResetConversation()
        {
            _history.Clear();
            _history.Add(new ChatMessage("system", systemPrompt));
            _log.Clear();
            _log.AppendLine($"Model: {model}  |  Yazıp Enter'a basın.");
        }

        private async void Send()
        {
            if (_waiting || string.IsNullOrWhiteSpace(_input)) return;

            string text = _input.Trim();
            _input = string.Empty;
            _log.AppendLine().AppendLine("Sen: " + text);
            _history.Add(new ChatMessage("user", text));

            _waiting = true;
            _waitStart = Time.realtimeSinceStartup;

            LLMResult result = await _client.SendAsync(_history, destroyCancellationToken);

            if (this == null) return; // Beklerken sahne kapandıysa hiçbir şey yapma.
            _waiting = false;
            float elapsed = Time.realtimeSinceStartup - _waitStart;

            if (!result.IsSuccess)
            {
                _history.RemoveAt(_history.Count - 1); // Başarısız mesajı geçmişte tutma.
                _log.AppendLine($"[HATA - {result.Status}] {result.ErrorMessage}");
                Debug.LogWarning($"LLM hatası ({result.Status}): {result.ErrorMessage}");
                return;
            }

            if (NpcResponseParser.TryParse(result.RawText, out NpcResponse npc, out string parseError))
            {
                _history.Add(new ChatMessage("assistant", result.RawText));
                _log.AppendLine("Deli Dumrul: " + npc.Cevap);
                _log.AppendLine($"    kapı: {npc.KapiyiAc} | kavga: {npc.KavgaBaslat} | öfke: {npc.Ofke} | {elapsed:0.0} sn");
            }
            else
            {
                _history.RemoveAt(_history.Count - 1);
                _log.AppendLine($"[BOZUK JSON] {parseError}");
                _log.AppendLine("    Ham yanıt: " + result.RawText);
                Debug.LogWarning($"JSON ayrıştırılamadı: {parseError}\n{result.RawText}");
            }

            _scroll.y = float.MaxValue;
        }

        private void OnGUI()
        {
            const float width = 560f;
            GUILayout.BeginArea(new Rect(10, 10, width, Screen.height - 20), GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>LLM Test – Deli Dumrul</b>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Kare: {Time.frameCount}"); // Donma kontrolü
            GUILayout.EndHorizontal();

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
            GUILayout.Label(_log.ToString(), new GUIStyle(GUI.skin.label) { wordWrap = true });
            GUILayout.EndScrollView();

            if (_waiting)
            {
                int dots = 1 + (int)((Time.realtimeSinceStartup - _waitStart) * 2f) % 3;
                GUILayout.Label("Deli Dumrul düşünüyor" + new string('.', dots));
            }

            // Enter ile gönderme
            Event e = Event.current;
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                && GUI.GetNameOfFocusedControl() == "llmInput")
            {
                Send();
                e.Use();
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = !_waiting;
            GUI.SetNextControlName("llmInput");
            _input = GUILayout.TextField(_input, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Gönder", GUILayout.Width(80))) Send();
            if (GUILayout.Button("Sıfırla", GUILayout.Width(70))) ResetConversation();
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
