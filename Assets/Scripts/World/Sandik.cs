using UnityEngine;

public class Sandik : MonoBehaviour
{
    private Animator anim;
    private bool acildiMi = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D temas)
    {
        // Aybars sandığa değerse ve sandık henüz açılmadıysa çalışır
        if (temas.CompareTag("Player") && !acildiMi)
        {
            acildiMi = true;
            anim.SetTrigger("AC");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CoinEkle(3);
            }
        }
    }
}