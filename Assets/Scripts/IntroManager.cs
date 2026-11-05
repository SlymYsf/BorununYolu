using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BorununYolu.Intro
{
    public class IntroManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup blackScreen;
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI storyText;
        [SerializeField] private GameObject continueHint;
        [SerializeField] private GameObject coinUI;
        [SerializeField, TextArea(2, 4)] private string[] introLines;
        [SerializeField] private MonoBehaviour playerMovementScript;

        private int currentIndex = 0;
        private bool isStoryActive = false;

        private void Start()
        {
            if (blackScreen != null) blackScreen.alpha = 1f;
            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (coinUI != null) coinUI.SetActive(false);

            if (introLines.Length > 0)
            {
                if (dialogPanel != null) dialogPanel.SetActive(true);
                storyText.text = introLines[0];
                isStoryActive = true;
                if (continueHint != null) continueHint.SetActive(true);
            }
            else
            {
                FinishStory();
            }
        }

        private void Update()
        {
            if (!isStoryActive) return;

            bool advance = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) advance = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) advance = true;

            if (advance)
            {
                currentIndex++;
                if (currentIndex < introLines.Length)
                {
                    storyText.text = introLines[currentIndex];
                }
                else
                {
                    FinishStory();
                }
            }
        }

        private void FinishStory()
        {
            isStoryActive = false;
            if (dialogPanel != null) dialogPanel.SetActive(false);
            if (continueHint != null) continueHint.SetActive(false);
            if (coinUI != null) coinUI.SetActive(true);

            StartCoroutine(FadeOutBlackScreen());
        }

        private IEnumerator FadeOutBlackScreen()
        {
            float duration = 1.5f;
            float startAlpha = blackScreen.alpha;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                blackScreen.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
                yield return null;
            }

            blackScreen.alpha = 0f;
            blackScreen.blocksRaycasts = false;
            if (playerMovementScript != null)
                playerMovementScript.enabled = true;
        }
    }
}