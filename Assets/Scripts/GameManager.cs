using UnityEngine;
using TMPro; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int toplamCoin = 0;

    public TextMeshProUGUI akceYazisi; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CoinEkle(int miktar)
    {
        toplamCoin += miktar;
        Debug.Log("Mevcut Akçe: " + toplamCoin);

        if (akceYazisi != null)
        {
            akceYazisi.text = "Akçe: " + toplamCoin.ToString();
        }
    }
}
