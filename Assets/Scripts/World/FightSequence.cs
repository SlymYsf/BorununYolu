using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace BorununYolu.World
{
    /// <summary>
    /// Deli Dumrul ile kavga sonrası kısa sahne: ekran kararır, oyuncu köprü başına
    /// geri konur, ekran açılır. Kimin kavga ettiğini bilmez; Play() Inspector'dan
    /// DialoguePanel.FightStarted olayına bağlanır.
    /// </summary>
    public class FightSequence : MonoBehaviour
    {
        [Header("Sahne")]
        [SerializeField] private CanvasGroup blackScreen;   // Tam ekran siyah Image'ın CanvasGroup'u
        [SerializeField] private Transform player;
        [SerializeField] private Transform respawnPoint;    // Köprü başı

        [Header("Süreler (sn)")]
        [SerializeField] private float startDelay = 1.5f;   // Oyuncu Deli Dumrul'un son sözünü okuyabilsin
        [SerializeField] private float fadeDuration = 0.4f;
        [SerializeField] private float holdDuration = 0.8f;

        [Header("Olaylar")]
        public UnityEvent Started;     // Oyuncu girdisini kilitlemek için
        public UnityEvent BlackedOut;  // Ekran tam karardığında: panel kapanır, öfke düşer
        public UnityEvent Finished;    // Oyuncu girdisini açmak için

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            SetBlack(0f);
        }

        public void Play()
        {
            if (IsPlaying) return;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            IsPlaying = true;
            Started?.Invoke();

            yield return new WaitForSecondsRealtime(startDelay);
            yield return Fade(0f, 1f);

            MovePlayerToRespawn();
            BlackedOut?.Invoke();
            yield return new WaitForSecondsRealtime(holdDuration);

            yield return Fade(1f, 0f);

            IsPlaying = false;
            Finished?.Invoke();
        }

        private void MovePlayerToRespawn()
        {
            if (player == null || respawnPoint == null) return;

            // Rigidbody2D varsa onun üzerinden taşı ki fizik eski konuma geri çekmesin.
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.position = respawnPoint.position;
                body.linearVelocity = Vector2.zero;
            }
            player.position = respawnPoint.position;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                SetBlack(Mathf.Lerp(from, to, t / fadeDuration));
                yield return null;
            }
            SetBlack(to);
        }

        private void SetBlack(float alpha)
        {
            if (blackScreen == null) return;
            blackScreen.alpha = alpha;
            blackScreen.blocksRaycasts = alpha > 0f; // Kararmışken tıklamalar geçmesin.
        }
    }
}
