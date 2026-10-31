using UnityEngine;
using UnityEngine.Events;

namespace BorununYolu.World
{
    /// <summary>
    /// Köprüye yaklaşan bölgeyi işaretleyen görünmez tetikleyici.
    /// Oyuncu girince PlayerEntered olayını yayar; ne yapılacağını bilmez.
    /// (Örn. Inspector'dan DeliDumrulNpc.WarmUpModel bağlanır.)
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BridgeZoneTrigger : MonoBehaviour
    {
        [Header("Olaylar")]
        public UnityEvent PlayerEntered;
        public UnityEvent PlayerExited;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) PlayerEntered?.Invoke();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) PlayerExited?.Invoke();
        }
    }
}
