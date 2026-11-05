using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BorununYolu.Ending
{
    /// <summary>
    /// Oyunun sonu: ekran kararır, son yazısı ve "Yeniden Oyna / Çık" düğmeleri gelir.
    /// DedeKorkutNpc final sahnesinin sonunda ShowImmediately() ile açar; Show() yavaşça belirir.
    /// </summary>
    public class EndingScreen : MonoBehaviour
    {
        [Header("Arayüz")]
        [SerializeField] private CanvasGroup screen;   // Tam ekran siyah zemin + yazılar + düğmeler
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        [Header("Süreler (sn)")]
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private float fadeDuration = 1.2f;

        public bool IsShown { get; private set; }

        private void Awake()
        {
            restartButton.onClick.AddListener(Restart);
            quitButton.onClick.AddListener(Quit);
            SetVisible(0f);
        }

        public void Show()
        {
            if (IsShown) return;
            IsShown = true;
            StartCoroutine(FadeIn());
        }

        /// <summary>Beklemeden tam görünür yapar (üstünde açılan bir karartının altında hazır durması için).</summary>
        public void ShowImmediately()
        {
            StopAllCoroutines();
            IsShown = true;
            SetVisible(1f);
        }

        private IEnumerator FadeIn()
        {
            yield return new WaitForSecondsRealtime(startDelay);

            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                SetVisible(t / fadeDuration);
                yield return null;
            }
            SetVisible(1f);
        }

        private void SetVisible(float alpha)
        {
            if (screen == null) return;
            screen.alpha = alpha;
            screen.blocksRaycasts = alpha > 0f;   // Görünürken arkadaki oyuna tıklanmasın.
            screen.interactable = alpha >= 1f;    // Düğmeler tam görününce çalışsın.
        }

        private void Restart()
        {
            SceneManager.LoadScene("01_ObaCikisi");
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
