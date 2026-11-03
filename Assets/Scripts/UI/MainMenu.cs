using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OyunaBasla()
    {
        SceneManager.LoadScene("01_ObaCikisi");
    }
    public void OyundanCik()
    {
        Debug.Log("Oyun kapatıldı!");
        Application.Quit();
    }
}