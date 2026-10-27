using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BorununYolu.Core;

namespace BorununYolu.LLM
{
    public enum DialogueEvent
    {
        None,
        DoorOpened,
        FightStarted
    }

    public sealed class DialogueTurnResult
    {
        public bool Success { get; }
        public LLMStatus Status { get; }
        public string NpcText { get; }
        public string ErrorMessage { get; }
        public DialogueEvent Event { get; }
        public bool DoorBlockedByRules { get; }
        public NpcResponse Response { get; }
        public double ElapsedSeconds { get; }

        private DialogueTurnResult(bool success, LLMStatus status, string npcText, string errorMessage,
            DialogueEvent ev, bool doorBlocked, NpcResponse response, double elapsed)
        {
            Success = success;
            Status = status;
            NpcText = npcText;
            ErrorMessage = errorMessage;
            Event = ev;
            DoorBlockedByRules = doorBlocked;
            Response = response;
            ElapsedSeconds = elapsed;
        }

        public static DialogueTurnResult Ok(NpcResponse response, DialogueEvent ev, bool doorBlocked, double elapsed) =>
            new DialogueTurnResult(true, LLMStatus.Success, response.Cevap, null, ev, doorBlocked, response, elapsed);

        public static DialogueTurnResult Fail(LLMStatus status, string errorMessage) =>
            new DialogueTurnResult(false, status, null, errorMessage, DialogueEvent.None, false, null, 0);
    }

    /// <summary>
    /// Bir NPC ile yapılan konuşmanın tamamını yönetir: geçmiş, akçe, öfke ve oyun kuralları.
    /// Model kapıyı açmak istese bile kararı oyun kurallarıyla doğrular.
    /// </summary>
    public sealed class NpcDialogue
    {
        private const int OfkeMaxDusus = 2;
        private const int OfkeMaxArtis = 4;

        private readonly ILLMClient _client;
        private readonly NpcPersona _persona;
        private readonly List<ChatMessage> _history = new List<ChatMessage>();

        public int Akce { get; private set; }
        public int Ofke { get; private set; }
        public bool BedelOdendi { get; private set; }
        public int OyuncuMesajSayisi { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsOver { get; private set; }

        public bool CanPay => !BedelOdendi && !IsOver && !IsBusy && Akce >= _persona.GecisBedeli;

        public NpcDialogue(ILLMClient client, NpcPersona persona, int baslangicAkcesi)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _persona = persona ?? throw new ArgumentNullException(nameof(persona));
            Reset(baslangicAkcesi);
        }

        public void Reset(int akce)
        {
            Akce = akce;
            Ofke = _persona.BaslangicOfkesi;
            BedelOdendi = false;
            OyuncuMesajSayisi = 0;
            IsOver = false;
            _history.Clear();
            _history.Add(new ChatMessage("system", _persona.BuildSystemPrompt(Akce, Ofke, BedelOdendi)));
        }

        public Task<DialogueTurnResult> SayAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(DialogueTurnResult.Fail(LLMStatus.InvalidResponse, "Boş mesaj gönderilemez."));

            return RunTurnAsync(text.Trim(), payment: false, ct);
        }

                public Task<DialogueTurnResult> PayAsync(string text = null, CancellationToken ct = default)
        {
            if (!CanPay)
                return Task.FromResult(DialogueTurnResult.Fail(LLMStatus.InvalidResponse,
                    "Kesende yeterli akçe yok ya da bedel zaten ödendi."));

            string odemeMesaji = $"(Geçiş bedeli olan {_persona.GecisBedeli} akçeyi sana veriyorum.)";
            string mesaj = string.IsNullOrWhiteSpace(text)
                ? odemeMesaji
                : $"{text.Trim()} {odemeMesaji}";

            return RunTurnAsync(mesaj, payment: true, ct);
        }
        private async Task<DialogueTurnResult> RunTurnAsync(string text, bool payment, CancellationToken ct)
        {
            if (IsBusy) return DialogueTurnResult.Fail(LLMStatus.InvalidResponse, "Önceki cevap bekleniyor.");
            if (IsOver) return DialogueTurnResult.Fail(LLMStatus.InvalidResponse, "Konuşma sona erdi.");

            // Bu turda geçerli olacak durum (ödeme ancak cevap başarılı gelirse kesinleşir).
            bool bedelBuTur = BedelOdendi || payment;
            int akceBuTur = payment ? Akce - _persona.GecisBedeli : Akce;

            _history[0] = new ChatMessage("system", _persona.BuildSystemPrompt(akceBuTur, Ofke, bedelBuTur));
            _history.Add(new ChatMessage("user", text));

            IsBusy = true;
            var stopwatch = Stopwatch.StartNew();
            LLMResult result;
            try
            {
                result = await _client.SendAsync(_history, ct);
            }
            finally
            {
                IsBusy = false;
            }
            stopwatch.Stop();

            if (!result.IsSuccess)
            {
                _history.RemoveAt(_history.Count - 1);
                return DialogueTurnResult.Fail(result.Status, result.ErrorMessage);
            }

            if (!NpcResponseParser.TryParse(result.RawText, out NpcResponse npc, out string parseError))
            {
                _history.RemoveAt(_history.Count - 1);
                return DialogueTurnResult.Fail(LLMStatus.InvalidResponse, parseError);
            }

            _history.Add(new ChatMessage("assistant", result.RawText));

            // Tur başarılı: durumu kesinleştir.
            if (payment)
            {
                Akce = akceBuTur;
                BedelOdendi = true;
            }
            OyuncuMesajSayisi++;

            // Öfke yumuşatma: model öfkeyi bir anda değiştiremez.
            int yeniOfke = Math.Max(Ofke - OfkeMaxDusus, Math.Min(Ofke + OfkeMaxArtis, npc.Ofke));
            Ofke = Math.Max(0, Math.Min(10, yeniOfke));

            // Oyun kuralları: modelin kararını doğrula.
            DialogueEvent ev = DialogueEvent.None;
            bool doorBlocked = false;

            if (npc.KavgaBaslat || Ofke >= _persona.KavgaOfkesi)
            {
                ev = DialogueEvent.FightStarted;
            }
            else if (npc.KapiyiAc)
            {
                bool odemeYolu = BedelOdendi;
                bool iknaYolu = Ofke <= _persona.KapiIcinMaxOfke &&
                                OyuncuMesajSayisi >= _persona.IknaIcinMinMesaj;

                if (odemeYolu || iknaYolu)
                {
                    ev = DialogueEvent.DoorOpened;
                    IsOver = true;
                }
                else
                {
                    doorBlocked = true; // Model açmak istedi, kurallar izin vermedi.
                }
            }

            return DialogueTurnResult.Ok(npc, ev, doorBlocked, stopwatch.Elapsed.TotalSeconds);
        }
    }
}