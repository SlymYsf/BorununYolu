using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BorununYolu.UI
{
    /// <summary>
    /// Önceden yazılmış satırları sırayla gösteren basit konuşma kutusu.
    /// Oyuncu E, Boşluk ya da sol tıkla ilerletir; son satırdan sonra kapanır.
    /// Satırların ne olduğunu bilmez, sadece gösterir.
    /// </summary>
    public class LineBox : MonoBehaviour
    {
        [Header("Arayüz")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text lineText;
        [SerializeField] private GameObject continueHint; // "Devam (E)" yazısı (isteğe bağlı)

        private readonly List<string> _lines = new List<string>();
        private int _index;
        private int _shownFrame;
        private Action _onFinished;

        public bool IsOpen => root.activeSelf;

        private void Awake()
        {
            root.SetActive(false);
        }

        public void Show(string speaker, IReadOnlyList<string> lines, Action onFinished)
        {
            if (lines == null || lines.Count == 0)
            {
                onFinished?.Invoke();
                return;
            }

            _lines.Clear();
            _lines.AddRange(lines);
            _index = 0;
            _onFinished = onFinished;
            _shownFrame = Time.frameCount; // Açan tuş aynı karede ilk satırı geçmesin.

            speakerText.text = speaker;
            lineText.text = _lines[0];
            if (continueHint != null) continueHint.SetActive(true);
            root.SetActive(true);
        }

        private void Update()
        {
            if (!IsOpen || Time.frameCount == _shownFrame || !AdvancePressed()) return;

            _index++;
            if (_index < _lines.Count)
            {
                lineText.text = _lines[_index];
                return;
            }

            root.SetActive(false);
            Action onFinished = _onFinished;
            _onFinished = null;
            onFinished?.Invoke();
        }

        private static bool AdvancePressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                return true;

            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }
    }
}
