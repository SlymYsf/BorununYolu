using System.Collections;
using BorununYolu.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BorununYolu.Ending
{
    /// <summary>
    /// Yolun sonundaki Dede Korkut. Oyuncu yaklaşınca final sahnesini oynatır:
    /// ekran yarı kararır (map hafifçe görünür), söz kutusu (içinde portresiyle) yavaşça
    /// belirir, kıtlık için öğütler söylenir; sonra ekran tam kararır, bitiş ekranı
    /// altında hazırlanır ve karartı yavaşça açılır.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DedeKorkutNpc : MonoBehaviour
    {
        [Header("Sahne")]
        [SerializeField] private CanvasGroup finalLayer;    // Siyah zemin + söz kutusu; sonda hepsi birlikte söner
        [SerializeField] private Image backdrop;            // Final katmanının siyah zemini
        [SerializeField] private CanvasGroup lineBoxGroup;  // Söz kutusunun kökü (LineBox root, portre içinde)
        [SerializeField] private LineBox lineBox;
        [SerializeField] private EndingScreen endingScreen;

        [Header("Karartma")]
        [Tooltip("Öğütler sırasında zeminin koyuluğu (Deli Dumrul panelindeki gibi map hafifçe görünsün).")]
        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.6f;

        [Header("Süreler (sn)")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float appearDuration = 1.2f; // Söz kutusunun belirmesi
        [SerializeField] private float blackoutDuration = 0.8f; // Öğütlerden sonra tam siyaha geçiş
        [SerializeField] private float holdBeforeEnding = 0.5f;
        [SerializeField] private float fadeOutDuration = 1.5f;

        [Header("Öğütler")]
        [SerializeField] private string displayName = "Dede Korkut";
        [SerializeField, TextArea(2, 4)] private string[] lines =
        {
            "Gel oğul Aybars, yolun uzun oldu. Obanın derdini rüzgar bana çoktan getirdi: ekin kurudu, pınar dindi.",
            "Suyu esirgeyen kurur, paylaşan çoğalır. Kuyunu komşundan, sofranı yoksuldan sakınma; bereket paylaşılan sofraya iner.",
            "Ağaç dikin, kökü toprağı tutsun. Toprak tutulursa yağmur da geri döner.",
            "Obana dön, ulu çınarın dibinde ateş yak, herkesi bir araya topla. Gönüller birleşince gök de yumuşar.",
            "Haydi git yiğidim! Börü'ye iyi bak; bulut senin ardından gelecek."
        };

        [Header("Olaylar")]
        public UnityEvent ConversationStarted; // Oyuncu girdisini kilitlemek için
        public UnityEvent Finished;            // Karartı tamamen açıldığında

        private bool _played;

        private void Awake()
        {
            if (finalLayer == null || backdrop == null || lineBoxGroup == null || lineBox == null || endingScreen == null)
            {
                Debug.LogError($"{name}: Final Layer, Backdrop, Line Box Group, Line Box ya da Ending Screen atanmamış!");
                enabled = false;
                return;
            }

            GetComponent<Collider2D>().isTrigger = true;
            SetLayer(0f);
            SetBackdrop(0f);
            lineBoxGroup.alpha = 0f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled || _played || !other.CompareTag("Player")) return;

            _played = true; // Oyun burada biter; final bir kez oynar.
            StartCoroutine(PlayFinale());
        }

        private IEnumerator PlayFinale()
        {
            ConversationStarted?.Invoke();

            SetLayer(1f); // Katman açık; görünen sadece zeminin ve kutunun kendi şeffaflığı
            yield return Fade(SetBackdrop, 0f, dimAlpha, fadeInDuration);

            // Kutu ilk satırla dolu açılır ama görünmez, sonra yavaşça belirir.
            bool done = false;
            lineBox.Show(displayName, lines, () => done = true, appearDuration);
            yield return Fade(a => lineBoxGroup.alpha = a, 0f, 1f, appearDuration);
            yield return new WaitUntil(() => done);

            yield return Fade(SetBackdrop, dimAlpha, 1f, blackoutDuration);
            yield return new WaitForSecondsRealtime(holdBeforeEnding);

            // Bitiş ekranı karartının altında hazır olsun, sonra karartı açılsın.
            endingScreen.ShowImmediately();
            yield return Fade(SetLayer, 1f, 0f, fadeOutDuration);

            Finished?.Invoke();
        }

        private static IEnumerator Fade(System.Action<float> apply, float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                apply(Mathf.Lerp(from, to, t / duration));
                yield return null;
            }
            apply(to);
        }

        private void SetLayer(float alpha)
        {
            finalLayer.alpha = alpha;
            finalLayer.blocksRaycasts = alpha > 0f; // Kararmışken arkadaki düğmelere tıklanmasın.
        }

        private void SetBackdrop(float alpha)
        {
            Color c = backdrop.color;
            c.a = alpha;
            backdrop.color = c;
        }
    }
}
