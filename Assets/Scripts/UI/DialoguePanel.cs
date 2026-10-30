using System.Threading.Tasks;
using BorununYolu.Core;
using BorununYolu.LLM;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BorununYolu.UI
{
    /// <summary>
    /// Ekranın altındaki konuşma paneli. Sadece gösterimden sorumludur;
    /// konuşmanın kuralları NpcDialogue'dadır.
    /// </summary>
    public class DialoguePanel : MonoBehaviour
    {
        [Header("Arayüz")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text npcNameText;
        [SerializeField] private TMP_Text npcLineText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text thinkingText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button payButton;
        [SerializeField] private Button closeButton;

        [Header("Olaylar")]
        public UnityEvent DoorOpened;
        public UnityEvent FightStarted;
        public UnityEvent Closed;

        private NpcDialogue _dialogue;
        private NpcPersona _persona;

        public bool IsOpen => root.activeSelf;

        private void Awake()
        {
            sendButton.onClick.AddListener(OnSendClicked);
            payButton.onClick.AddListener(OnPayClicked);
            closeButton.onClick.AddListener(Close);
            inputField.onSubmit.AddListener(_ => OnSendClicked());
            root.SetActive(false);
        }

        public void Open(NpcDialogue dialogue, NpcPersona persona, string greeting)
        {
            _dialogue = dialogue;
            _persona = persona;
            npcNameText.text = persona.DisplayName;
            npcLineText.text = greeting;
            inputField.text = string.Empty;
            root.SetActive(true);
            RefreshUI();
            FocusInput();
        }

        public void Close()
        {
            if (_dialogue != null && _dialogue.IsBusy) return; // Cevap beklenirken kapatma.
            root.SetActive(false);
            Closed?.Invoke();
        }

                private async void OnSendClicked()
        {
            if (_dialogue == null || _dialogue.IsBusy || _dialogue.IsOver) return;

            string text = inputField.text.Trim();
            if (text.Length == 0 && !_dialogue.OdemeBildirilmedi) return;
            inputField.text = string.Empty;

            await RunTurn(_dialogue.SayAsync(text, destroyCancellationToken));
        }

        private void OnPayClicked()
        {
            if (_dialogue == null || !_dialogue.Pay()) return;

            RefreshUI();
            FocusInput(); // Oyuncu hemen yazmaya devam edebilsin.
        }

        private async Task RunTurn(Task<DialogueTurnResult> turn)
        {
            RefreshUI(); // "Düşünüyor..." görünsün, düğmeler kilitlensin.

            DialogueTurnResult result = await turn;
            if (this == null) return;

            if (!result.Success)
            {
                npcLineText.text = FriendlyError(result);
                Debug.LogWarning($"Diyalog hatası ({result.Status}): {result.ErrorMessage}");
            }
            else
            {
                npcLineText.text = result.NpcText;

                if (result.Event == DialogueEvent.DoorOpened)
                    DoorOpened?.Invoke();
                else if (result.Event == DialogueEvent.FightStarted)
                    FightStarted?.Invoke();
            }

            RefreshUI();
            FocusInput();
        }

        private void Update()
        {
            if (_dialogue == null || !_dialogue.IsBusy) return;

            int dots = 1 + (int)(Time.time * 2f) % 3;
            thinkingText.text = $"{_persona.DisplayName} düşünüyor" + new string('.', dots);
        }

        private void RefreshUI()
        {
            bool busy = _dialogue.IsBusy;
            bool over = _dialogue.IsOver;

            thinkingText.gameObject.SetActive(busy);
            inputField.interactable = !busy && !over;
            sendButton.interactable = !busy && !over;
            payButton.interactable = _dialogue.CanPay;
            closeButton.interactable = !busy;

            statusText.text = _dialogue.BedelOdendi
                ? $"Kese: {_dialogue.Akce} akçe   (Bedel ödendi)"
                : $"Kese: {_dialogue.Akce} akçe   Bedel: {_persona.GecisBedeli} akçe";
        }

        private void FocusInput()
        {
            if (!inputField.interactable) return;
            inputField.Select();
            inputField.ActivateInputField();
        }

        private static string FriendlyError(DialogueTurnResult result)
        {
            switch (result.Status)
            {
                case LLMStatus.ServerUnreachable:
                    return "(Deli Dumrul seni duymuyor... Ollama kapalı olabilir. Ollama'yı başlatıp tekrar dene.)";
                case LLMStatus.Timeout:
                    return "(Deli Dumrul uzun uzun düşündü ama bir şey demedi. Tekrar dene.)";
                case LLMStatus.ModelNotFound:
                    return "(Dil modeli bulunamadı. " + result.ErrorMessage + ")";
                case LLMStatus.InvalidResponse:
                    return "(Deli Dumrul bir şeyler mırıldandı ama anlaşılmadı. Tekrar dene.)";
                default:
                    return "(" + result.ErrorMessage + ")";
            }
        }
    }
}