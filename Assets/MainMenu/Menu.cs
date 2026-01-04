using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    /// <summary>
    /// Memuat scene game utama ketika tombol Play diklik
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("main");
    }

    /// <summary>
    /// Keluar dari aplikasi ketika tombol Quit diklik
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Game Keluar");
        Application.Quit();
        
        // Untuk testing di Editor, Unity tidak akan benar-benar quit
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
