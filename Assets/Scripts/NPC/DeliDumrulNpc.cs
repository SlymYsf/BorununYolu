using BorununYolu.Core;
using BorununYolu.LLM;
using BorununYolu.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BorununYolu.NPC
{
    /// <summary>
    /// Köprüyü tutan Deli Dumrul. Oyuncu yaklaşıp E'ye basınca konuşma panelini açar.
    /// Konuşma (NpcDialogue) panel kapanıp açılsa da devam eder.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeliDumrulNpc : MonoBehaviour
    {
        [Header("NPC")]
        [SerializeField] private NpcPersona persona;
        [SerializeField] private DialoguePanel dialoguePanel;
        [SerializeField] private GameObject talkPrompt; // "E: Konuş" yazısı (isteğe bağlı)
        [SerializeField] private int baslangicAkcesi = 12;

        [Header("Ollama")]
        [SerializeField] private string baseUrl = "http://localhost:11434";
        [SerializeField] private string model = "qwen3.5:4b";
        [SerializeField] private float timeoutSeconds = 30f;

        [Header("Kavga")]
        [SerializeField, Range(0, 10)] private int kavgaSonrasiOfkeDususu = 3;

        [Header("Test")]
        [SerializeField] private bool openOnStart;

        [Header("Olaylar")]
        public UnityEvent ConversationStarted;
        public UnityEvent ConversationEnded;

        private NpcDialogue _dialogue;
        private IModelWarmup _warmup;
        private bool _warmupStarted;
        private bool _playerInRange;

        private void Awake()
        {
            if (persona == null || dialoguePanel == null)
            {
                Debug.LogError($"{name}: Persona ya da Dialogue Panel atanmamış!");
                enabled = false;
                return;
            }

            GetComponent<Collider2D>().isTrigger = true;

            var client = new OllamaClient(baseUrl, model, timeoutSeconds);
            _warmup = client;
            _dialogue = new NpcDialogue(client, persona, baslangicAkcesi);

            SetPromptVisible(false);
        }

        private void OnEnable()
        {
            if (dialoguePanel != null) dialoguePanel.Closed.AddListener(OnPanelClosed);
        }

        private void OnDisable()
        {
            if (dialoguePanel != null) dialoguePanel.Closed.RemoveListener(OnPanelClosed);
        }

        private void Start()
        {
            if (openOnStart) StartConversation();
        }

        private void Update()
        {
            if (!_playerInRange || dialoguePanel.IsOpen || _dialogue.IsOver) return;

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                StartConversation();
        }

        public void StartConversation()
        {
            if (dialoguePanel.IsOpen) return;

            SetPromptVisible(false);
            dialoguePanel.Open(_dialogue, persona, persona.AcilisSozu);
            ConversationStarted?.Invoke();
        }

        /// <summary>
        /// Modeli arka planda belleğe yükletir; oyun beklemez. Köprü bölgesine girişte
        /// (BridgeZoneTrigger.PlayerEntered) Inspector'dan çağrılır. Sadece bir kez çalışır.
        /// </summary>
        public async void WarmUpModel()
        {
            if (_warmup == null || _warmupStarted) return;
            _warmupStarted = true;

            LLMResult result = await _warmup.WarmUpAsync(destroyCancellationToken);
            if (this == null) return;

            if (!result.IsSuccess && result.Status != LLMStatus.Cancelled)
            {
                // Isıtma başarısızsa sorun değil; ilk mesajda hata panelde anlaşılır biçimde gösterilir.
                Debug.LogWarning($"Model ısıtılamadı ({result.Status}): {result.ErrorMessage}");
                _warmupStarted = false; // Oyuncu bölgeye tekrar girince yeniden denensin.
            }
        }

        /// <summary>
        /// Kavga bittiğinde (FightSequence.BlackedOut) çağrılır: öfke biraz düşer,
        /// oyuncu yeniden konuşabilir.
        /// </summary>
        public void CalmAfterFight()
        {
            _dialogue?.KavgaSonrasiSakinles(kavgaSonrasiOfkeDususu);
        }

        private void OnPanelClosed()
        {
            ConversationEnded?.Invoke();
            SetPromptVisible(_playerInRange && !_dialogue.IsOver);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            SetPromptVisible(!dialoguePanel.IsOpen && !_dialogue.IsOver);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            SetPromptVisible(false);
        }

        private void SetPromptVisible(bool visible)
        {
            if (talkPrompt != null) talkPrompt.SetActive(visible);
        }
    }
}