using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace BorununYolu.World
{
    /// <summary>
    /// Köprünün ağzındaki kapı. Açılınca geçişi engelleyen collider kapanır.
    /// Kimin açtığını bilmez; Open() metodu Inspector'dan bir olaya bağlanır.
    /// </summary>
    public class BridgeGate : MonoBehaviour
    {
        [SerializeField] private Collider2D blocker;
        [SerializeField] private SpriteRenderer gateRenderer;
        [SerializeField] private Sprite openSprite;   // Varsa açık kapı görseli; yoksa kapı solar
        [SerializeField] private float fadeDuration = 0.6f;

        public UnityEvent Opened;

        public bool IsOpen { get; private set; }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;

            if (blocker != null)
                blocker.enabled = false;

            if (gateRenderer != null)
            {
                if (openSprite != null)
                    gateRenderer.sprite = openSprite;
                else
                    StartCoroutine(FadeOut());
            }

            Opened?.Invoke();
        }

        private IEnumerator FadeOut()
        {
            Color start = gateRenderer.color;
            const float hedefAlpha = 0.25f;

            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                float a = Mathf.Lerp(start.a, hedefAlpha, t / fadeDuration);
                gateRenderer.color = new Color(start.r, start.g, start.b, a);
                yield return null;
            }

            gateRenderer.color = new Color(start.r, start.g, start.b, hedefAlpha);
        }
    }
}