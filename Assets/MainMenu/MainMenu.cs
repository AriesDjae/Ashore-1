using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Button Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    [Tooltip("Titik mulai audio dalam detik (0 = dari awal)")]
    [SerializeField] private float buttonClickStartTime = 0f;
    
    [Space(10)]
    [SerializeField] private AudioClip quitButtonSound; // Optional: different sound for quit
    [Tooltip("Titik mulai audio dalam detik (0 = dari awal)")]
    [SerializeField] private float quitButtonStartTime = 0f;
    
    [Header("Settings")]
    [Tooltip("Delay sebelum pindah scene setelah sound dimainkan")]
    [SerializeField] private float delayBeforeSceneLoad = 0.3f;
    
    private AudioSource audioSource;

    private void Awake()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayGame()
    {
        StartCoroutine(PlaySoundAndLoadScene());
    }

    private IEnumerator PlaySoundAndLoadScene()
    {
        // Play button click sound with custom start time
        if (buttonClickSound != null && audioSource != null)
        {
            PlaySoundWithStartTime(buttonClickSound, buttonClickStartTime);
            // Wait for sound to finish (or a short delay)
            yield return new WaitForSeconds(delayBeforeSceneLoad);
        }
        
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        // Play quit button sound (uses buttonClickSound if quitButtonSound is not assigned)
        AudioClip soundToPlay = quitButtonSound != null ? quitButtonSound : buttonClickSound;
        float startTime = quitButtonSound != null ? quitButtonStartTime : buttonClickStartTime;
        
        if (soundToPlay != null && audioSource != null)
        {
            PlaySoundWithStartTime(soundToPlay, startTime);
        }

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Memainkan audio dengan titik mulai kustom
    /// </summary>
    /// <param name="clip">Audio clip yang akan dimainkan</param>
    /// <param name="startTime">Titik mulai dalam detik</param>
    private void PlaySoundWithStartTime(AudioClip clip, float startTime)
    {
        audioSource.clip = clip;
        audioSource.time = Mathf.Clamp(startTime, 0f, clip.length);
        audioSource.Play();
    }
}