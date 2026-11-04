using UnityEngine;

namespace BorununYolu.World
{
    /// <summary>
    /// Konuşma ve ara sahnelerde oyuncuyu yerinde durdurur: hareket script'ini kapatır,
    /// hızı sıfırlar ve Animator'ü durma animasyonuna geçirir (yoksa koşarken donup kalır).
    /// Lock/Unlock Inspector'dan konuşma olaylarına bağlanır.
    /// </summary>
    public class PlayerLock : MonoBehaviour
    {
        [SerializeField] private Behaviour movement;     // Oyuncunun hareket script'i (ör. AybarsHareket)
        [SerializeField] private Rigidbody2D body;       // İsteğe bağlı
        [SerializeField] private Animator animator;      // İsteğe bağlı
        [SerializeField] private string idleState = "Idle";

        public bool IsLocked { get; private set; }

        public void Lock()
        {
            IsLocked = true;
            if (movement != null) movement.enabled = false;
            if (body != null) body.linearVelocity = Vector2.zero;
            if (animator != null && !string.IsNullOrEmpty(idleState)) animator.Play(idleState);
        }

        public void Unlock()
        {
            IsLocked = false;
            if (movement != null) movement.enabled = true;
        }
    }
}
