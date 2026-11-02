using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BorununYolu.Ending
{
    /// <summary>
    /// Oyunun sonu: ekran kararır, son yazısı ve "Yeniden Oyna / Çık" düğmeleri gelir.
    /// Show() Inspector'dan DedeKorkutNpc.Finished olayına bağlanır.
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
            Scene current = SceneManager.GetActiveScene();
            if (current.buildIndex < 0)
            {
                Debug.LogError($"'{current.name}' sahnesi Build Settings'te yok; yeniden yüklenemiyor. " +
                               "File > Build Profiles > Scene List'e ekleyin.");
                return;
            }
            SceneManager.LoadScene(current.buildIndex);
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
