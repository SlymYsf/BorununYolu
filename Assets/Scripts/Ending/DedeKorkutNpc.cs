using System.Collections.Generic;
using BorununYolu.LLM;
using BorununYolu.NPC;
using BorununYolu.UI;
using UnityEngine;
using UnityEngine.Events;

namespace BorununYolu.Ending
{
    /// <summary>
    /// Yolun sonundaki Dede Korkut. Oyuncu yaklaşınca kapanış sözünü söyler,
    /// söz bitince Finished olayını yayar (Inspector'dan EndingScreen.Show bağlanır).
    /// Sözün ortası, köprünün nasıl geçildiğine göre değişir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DedeKorkutNpc : MonoBehaviour
    {
        [Header("Bağlantılar")]
        [SerializeField] private LineBox lineBox;
        [Tooltip("Köprü sahnesinde Deli Dumrul. Boşsa aşağıdaki test değerleri kullanılır.")]
        [SerializeField] private DeliDumrulNpc deliDumrul;

        [Header("Test (Deli Dumrul yokken)")]
        [SerializeField] private GecisYolu testGecisYolu = GecisYolu.Odeme;
        [SerializeField] private int testKavgaSayisi;

        [Header("Kapanış sözü")]
        [SerializeField] private string displayName = "Dede Korkut";
        [SerializeField, TextArea(2, 4)] private string acilis =
            "Hoş geldin Aybars! Gök kurdun ardınca gelen yiğidin yolu kısa olmaz, sen bu yolu yürüdün.";
        [SerializeField, TextArea(2, 4)] private string odemeSozu =
            "Deli Dumrul'a akçeni verip geçmişsin. Cömertlik de yiğitliktir, yerinde verilen akçe yitmez.";
        [SerializeField, TextArea(2, 4)] private string iknaSozu =
            "Deli Dumrul'u sözünle geçmişsin. Demek dilin kılıcından keskinmiş.";
        [SerializeField, TextArea(2, 4)] private string bilinmeyenSozu =
            "Köprüyü nasıl geçtin bilmem, ama yol seni bana getirdi.";
        [SerializeField, TextArea(2, 4)] private string kavgaEki =
            "Hele bir de Deli Dumrul'la güreşip yılmamışsın!";
        [SerializeField, TextArea(2, 4)] private string kapanis =
            "Börü'ye iyi bak; o seni nereye götürürse orası yurdundur. Senin hikâyeni de ben söyleyeyim, kopuzum yanımda.";

        [Header("Olaylar")]
        public UnityEvent ConversationStarted; // Oyuncu girdisini kilitlemek için
        public UnityEvent Finished;            // Bitiş ekranı için

        private bool _spoken;

        private void Awake()
        {
            if (lineBox == null)
            {
                Debug.LogError($"{name}: Line Box atanmamış!");
                enabled = false;
                return;
            }

            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled || _spoken || !other.CompareTag("Player")) return;

            _spoken = true; // Oyun burada biter; söz bir kez söylenir.
            ConversationStarted?.Invoke();
            lineBox.Show(displayName, BuildLines(), () => Finished?.Invoke());
        }

        private List<string> BuildLines()
        {
            GecisYolu yol = deliDumrul != null ? deliDumrul.GecisYolu : testGecisYolu;
            int kavga = deliDumrul != null ? deliDumrul.KavgaSayisi : testKavgaSayisi;

            string orta;
            switch (yol)
            {
                case GecisYolu.Odeme: orta = odemeSozu; break;
                case GecisYolu.Ikna: orta = iknaSozu; break;
                default: orta = bilinmeyenSozu; break;
            }
            if (kavga > 0) orta += " " + kavgaEki;

            return new List<string> { acilis, orta, kapanis };
        }
    }
}
