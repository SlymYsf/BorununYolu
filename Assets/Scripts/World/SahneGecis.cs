using UnityEngine;
using UnityEngine.SceneManagement;

public class SahneGecis : MonoBehaviour
{
    public string gidecegiSahneAdi; // Hangi sahneye gidileceğini Unity den gireceğiz böylece siğer sahnelerde de bu prefabsı kullanacağız

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(gidecegiSahneAdi);
        }
    }
}