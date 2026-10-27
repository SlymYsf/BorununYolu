using System.Text;
using BorununYolu.Core;
using UnityEngine;

namespace BorununYolu.LLM
{
    /// <summary>
    /// GEÇİCİ TEST ARACI: NpcDialogue'u sahnede denemek için.
    /// Sağ üstteki kare sayacı, model düşünürken oyunun donmadığını gösterir.
    /// </summary>
    public class LLMTestWindow : MonoBehaviour
    {
        [Header("Ollama")]
        [SerializeField] private string baseUrl = "http://localhost:11434";
        [SerializeField] private string model = "qwen3.5:4b";
        [SerializeField] private float timeoutSeconds = 30f;

        [Header("NPC")]
        [SerializeField] private NpcPersona persona;
        [SerializeField] private int baslangicAkcesi = 12;

        private NpcDialogue _dialogue;
        private readonly StringBuilder _log = new StringBuilder();
        private string _input = string.Empty;
        private float _waitStart;
        private Vector2 _scroll;

        private void Awake()
        {
            if (persona == null)
            {
                Debug.LogError("LLMTestWindow: Persona atanmamış! Inspector'dan DeliDumrul dosyasını sürükleyin.");
                enabled = false;
                return;
            }

            ILLMClient client = new OllamaClient(baseUrl, model, timeoutSeconds);
            _dialogue = new NpcDialogue(client, persona, baslangicAkcesi);
            ResetConversation();
        }

        private void ResetConversation()
        {
            _dialogue.Reset(baslangicAkcesi);
            _log.Clear();
            _log.AppendLine($"Model: {model}  |  Bedel: {persona.GecisBedeli} akçe");
        }

        private async void Send()
        {
            if (_dialogue.IsBusy || string.IsNullOrWhiteSpace(_input)) return;

            string text = _input.Trim();
            _input = string.Empty;
            _log.AppendLine().AppendLine("Sen: " + text);

            _waitStart = Time.realtimeSinceStartup;
            DialogueTurnResult result = await _dialogue.SayAsync(text, destroyCancellationToken);
            if (this == null) return;
            LogResult(result);
        }

                private async void Pay()
        {
            if (!_dialogue.CanPay) return;

            string text = _input.Trim();
            _input = string.Empty;

            string logText = string.IsNullOrEmpty(text)
                ? $"(Akçe ver: {persona.GecisBedeli})"
                : $"{text} (Akçe ver: {persona.GecisBedeli})";
            _log.AppendLine().AppendLine("Sen: " + logText);

            _waitStart = Time.realtimeSinceStartup;
            DialogueTurnResult result = await _dialogue.PayAsync(text, destroyCancellationToken);
            if (this == null) return;
            LogResult(result);
        }

        private void LogResult(DialogueTurnResult result)
        {
            if (!result.Success)
            {
                _log.AppendLine($"[HATA - {result.Status}] {result.ErrorMessage}");
                Debug.LogWarning($"Diyalog hatası ({result.Status}): {result.ErrorMessage}");
                return;
            }

            NpcResponse r = result.Response;
            _log.AppendLine($"{persona.DisplayName}: {result.NpcText}");
            _log.AppendLine($"    model → kapı: {r.KapiyiAc} | kavga: {r.KavgaBaslat} | öfke: {r.Ofke}");
            _log.AppendLine($"    oyun  → öfke: {_dialogue.Ofke} | kese: {_dialogue.Akce} | " +
                            $"mesaj: {_dialogue.OyuncuMesajSayisi} | {result.ElapsedSeconds:0.0} sn");

            if (result.Event == DialogueEvent.DoorOpened)
                _log.AppendLine("    >>> KAPI AÇILDI <<<");
            else if (result.Event == DialogueEvent.FightStarted)
                _log.AppendLine("    >>> KAVGA BAŞLADI <<<");
            else if (result.DoorBlockedByRules)
                _log.AppendLine("    (Model kapıyı açmak istedi, oyun kuralı engelledi)");

            _scroll.y = float.MaxValue;
        }

        private void OnGUI()
        {
            if (_dialogue == null) return;

            const float width = 600f;
            GUILayout.BeginArea(new Rect(10, 10, width, Screen.height - 20), GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>LLM Test – {persona.DisplayName}</b>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Kese: {_dialogue.Akce} | Öfke: {_dialogue.Ofke} | Kare: {Time.frameCount}");
            GUILayout.EndHorizontal();

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
            GUILayout.Label(_log.ToString(), new GUIStyle(GUI.skin.label) { wordWrap = true });
            GUILayout.EndScrollView();

            if (_dialogue.IsBusy)
            {
                int dots = 1 + (int)((Time.realtimeSinceStartup - _waitStart) * 2f) % 3;
                GUILayout.Label($"{persona.DisplayName} düşünüyor" + new string('.', dots));
            }

            Event e = Event.current;
            if (e.type == EventType.KeyDown &&
                (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) &&
                GUI.GetNameOfFocusedControl() == "llmInput")
            {
                Send();
                e.Use();
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = !_dialogue.IsBusy && !_dialogue.IsOver;
            GUI.SetNextControlName("llmInput");
            _input = GUILayout.TextField(_input, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Gönder", GUILayout.Width(70))) Send();
            GUI.enabled = _dialogue.CanPay;
            if (GUILayout.Button($"Akçe ver ({persona.GecisBedeli})", GUILayout.Width(110))) Pay();
            GUI.enabled = !_dialogue.IsBusy;
            if (GUILayout.Button("Sıfırla", GUILayout.Width(60))) ResetConversation();
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}