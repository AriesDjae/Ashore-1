using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

/// <summary>
/// Cutscene Manager dengan efek fade to black dan dialog di layar hitam.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneSlide
    {
        public Sprite image;
        [TextArea(3, 5)]
        public string dialogText;
        [TextArea(3, 5)]
        [Tooltip("Teks yang muncul di layar hitam SEBELUM slide ini")]
        public string transitionText;
        public AudioClip audioClip;
    }
    
    [Header("Cutscene Slides")]
    public CutsceneSlide[] slides;
    
    [Header("UI References")]
    public Image displayImage;
    public Image blackScreen;
    public TextMeshProUGUI dialogTextUI;
    public TextMeshProUGUI transitionTextUI; // Teks di layar hitam
    public GameObject dialogPanel;
    public GameObject continuePrompt;
    
    [Header("Transition Settings")]
    public float fadeToBlackDuration = 0.3f;
    public float blackScreenDuration = 0.5f;
    public float fadeFromBlackDuration = 0.3f;
    [Tooltip("Durasi tambahan untuk membaca teks transisi")]
    public float transitionTextReadTime = 2f;
    
    [Header("Typewriter Effect")]
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.03f;
    public bool useTypewriterOnTransition = true;
    
    [Header("Navigation")]
    public string nextSceneName = "";
    public bool allowSkip = true;
    
    [Header("End Screen (Opsional)")]
    [Tooltip("Panel yang muncul di akhir cutscene dengan tombol Main Lagi & Keluar")]
    public GameObject endScreenPanel;
    
    [Tooltip("Tombol Main Lagi di end screen")]
    public Button playAgainButton;
    
    [Tooltip("Tombol Keluar di end screen")]
    public Button exitButton;
    
    [Tooltip("Scene pertama game (untuk Main Lagi)")]
    public string firstLevelScene = "Forest";
    
    [Tooltip("Scene main menu (untuk Keluar)")]
    public string mainMenuScene = "Main Menu";
    
    [Header("Input")]
    [Tooltip("Input Actions asset (assign InputSystem_Actions)")]
    public InputActionAsset inputActionsAsset;
    private InputActionMap uiActionMap;
    private InputAction uiClickAction;
    private InputAction uiSubmitAction;
    private InputAction uiCancelAction;
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Tooltip("Titik mulai musik dalam detik (0 = dari awal)")]
    [Range(0f, 300f)]
    public float musicStartTime = 0f;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    
    [Header("Sound Effects")]
    [Tooltip("AudioSource khusus untuk SFX (akan dibuat otomatis jika kosong)")]
    public AudioSource sfxAudioSource;
    [Tooltip("SFX saat klik untuk lanjut ke slide berikutnya")]
    public AudioClip dialogClickSFX;
    [Tooltip("SFX saat skip typewriter effect")]
    public AudioClip skipTypewriterSFX;
    [Tooltip("SFX saat transisi antar slide")]
    public AudioClip transitionSFX;
    [Tooltip("SFX saat skip seluruh cutscene (ESC)")]
    public AudioClip skipCutsceneSFX;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Header("Typewriter Sound")]
    [Tooltip("SFX yang dimainkan setiap karakter muncul")]
    public AudioClip typewriterSFX;
    [Range(0f, 1f)]
    public float typewriterSFXVolume = 0.5f;
    [Tooltip("Minimum pitch untuk variasi sound")]
    [Range(0.5f, 1.5f)]
    public float typewriterPitchMin = 0.9f;
    [Tooltip("Maximum pitch untuk variasi sound")]
    [Range(0.5f, 1.5f)]
    public float typewriterPitchMax = 1.1f;
    [Tooltip("Jangan mainkan sound untuk spasi dan tanda baca")]
    public bool skipSoundOnSpaces = true;
    
    private int currentIndex = 0;
    private bool isTransitioning = false;
    private bool isTyping = false;
    private bool waitingForInput = false;
    private string fullText = "";
    private Coroutine typewriterCoroutine;
    
    void Awake()
    {
        // Initialize Input System
        if (inputActionsAsset != null)
        {
            uiActionMap = inputActionsAsset.FindActionMap("UI");
            if (uiActionMap != null)
            {
                uiClickAction = uiActionMap.FindAction("Click");
                uiSubmitAction = uiActionMap.FindAction("Submit");
                uiCancelAction = uiActionMap.FindAction("Cancel");
            }
        }
    }
    
    void OnEnable()
    {
        // Enable input actions
        if (uiActionMap != null)
        {
            uiActionMap.Enable();
        }
    }
    
    void OnDisable()
    {
        // Disable input actions
        if (uiActionMap != null)
        {
            uiActionMap.Disable();
        }
    }
    
    void Start()
    {
        // Setup SFX AudioSource jika belum ada
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
        }
        
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 0);
        }
        
        if (transitionTextUI != null)
        {
            transitionTextUI.text = "";
            transitionTextUI.gameObject.SetActive(false);
        }
        
        if (backgroundMusic != null && audioSource != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = musicVolume;
            audioSource.time = Mathf.Clamp(musicStartTime, 0f, backgroundMusic.length);
            audioSource.Play();
        }
        
        if (slides.Length > 0)
        {
            // Cek jika slide pertama punya transition text
            if (!string.IsNullOrEmpty(slides[0].transitionText))
            {
                StartCoroutine(ShowInitialTransition());
            }
            else
            {
                ShowSlide(0);
            }
        }
        
        // Hide end screen at start
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }
    }
    
    private IEnumerator ShowInitialTransition()
    {
        // Tampilkan layar hitam dengan teks untuk slide pertama
        if (blackScreen != null)
            blackScreen.color = Color.black;
        
        yield return StartCoroutine(ShowTransitionText(slides[0].transitionText));
        
        ShowSlide(0);
        
        yield return StartCoroutine(FadeBlackScreen(1f, 0f, fadeFromBlackDuration));
    }
    
    void Update()
    {
        // Check for click/space/enter input (single click detection)
        bool clickPressed = uiClickAction != null && uiClickAction.WasPressedThisFrame();
        bool submitPressed = uiSubmitAction != null && uiSubmitAction.WasPressedThisFrame();
        bool inputPressed = clickPressed || submitPressed;
        
        if (inputPressed)
        {
            if (isTyping)
            {
                // Jika sedang typing, skip typewriter dan tampilkan full text
                PlaySFX(skipTypewriterSFX);
                SkipTypewriter();
            }
            else if (waitingForInput)
            {
                // Jika sedang menunggu input (di transition text), lanjut
                PlaySFX(dialogClickSFX);
                waitingForInput = false;
            }
            else if (!isTransitioning)
            {
                // Jika tidak sedang typing dan tidak transitioning, lanjut ke slide berikutnya
                PlaySFX(dialogClickSFX);
                NextSlide();
            }
        }
        
        // ESC untuk skip seluruh cutscene
        if (allowSkip && uiCancelAction != null && uiCancelAction.WasPressedThisFrame())
        {
            PlaySFX(skipCutsceneSFX);
            SkipCutscene();
        }
    }
    
    private void ShowSlide(int index)
    {
        if (index < 0 || index >= slides.Length) return;
        
        currentIndex = index;
        CutsceneSlide slide = slides[index];
        
        // Reset typing state
        isTyping = false;
        waitingForInput = false;
        
        // Stop any ongoing typewriter effect
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        if (displayImage != null && slide.image != null)
        {
            displayImage.sprite = slide.image;
        }
        
        // Hide transition text
        if (transitionTextUI != null)
        {
            transitionTextUI.gameObject.SetActive(false);
        }
        
        if (dialogTextUI != null)
        {
            fullText = slide.dialogText ?? "";
            
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(!string.IsNullOrEmpty(fullText));
            }
            
            if (useTypewriterEffect && !string.IsNullOrEmpty(fullText))
            {
                // Reset text before starting typewriter
                dialogTextUI.text = "";
                typewriterCoroutine = StartCoroutine(TypewriterEffect(fullText, dialogTextUI));
            }
            else
            {
                dialogTextUI.text = fullText;
                isTyping = false; // Mark as not typing if no typewriter effect
            }
        }
        
        // Play slide-specific audio using SFX audio source (not the music source)
        if (sfxAudioSource != null && slide.audioClip != null)
        {
            sfxAudioSource.PlayOneShot(slide.audioClip);
        }
        
        UpdateContinuePrompt();
    }
    
    private IEnumerator TypewriterEffect(string text, TextMeshProUGUI targetText)
    {
        isTyping = true;
        targetText.text = "";
        
        foreach (char c in text)
        {
            // Check if typing was interrupted (for skip functionality)
            if (!isTyping)
            {
                break;
            }
            
            targetText.text += c;
            
            // Play typewriter sound for each character
            if (typewriterSFX != null && sfxAudioSource != null)
            {
                // Skip playing sound for spaces and punctuation if option is enabled
                if (!skipSoundOnSpaces || (!char.IsWhiteSpace(c) && !char.IsPunctuation(c)))
                {
                    sfxAudioSource.pitch = Random.Range(typewriterPitchMin, typewriterPitchMax);
                    sfxAudioSource.PlayOneShot(typewriterSFX, typewriterSFXVolume);
                }
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        // Reset pitch
        if (sfxAudioSource != null)
            sfxAudioSource.pitch = 1f;
        
        // Ensure full text is displayed when typing completes or is skipped
        if (targetText != null)
        {
            targetText.text = text;
        }
        
        isTyping = false;
    }
    
    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        if (transitionTextUI != null && transitionTextUI.gameObject.activeSelf)
        {
            transitionTextUI.text = fullText;
        }
        else if (dialogTextUI != null)
        {
            dialogTextUI.text = fullText;
        }
        
        isTyping = false;
    }
    
    public void NextSlide()
    {
        if (isTransitioning) return;
        
        if (currentIndex < slides.Length - 1)
        {
            StartCoroutine(TransitionWithBlackScreen(currentIndex + 1));
        }
        else
        {
            EndCutscene();
        }
    }
    
    private IEnumerator TransitionWithBlackScreen(int targetIndex)
    {
        isTransitioning = true;
        
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
        
        // 1. Fade to black
        yield return StartCoroutine(FadeBlackScreen(0f, 1f, fadeToBlackDuration));
        
        // 2. Tampilkan transition text jika ada
        CutsceneSlide nextSlide = slides[targetIndex];
        if (!string.IsNullOrEmpty(nextSlide.transitionText))
        {
            yield return StartCoroutine(ShowTransitionText(nextSlide.transitionText));
        }
        else
        {
            // Tunggu sebentar di layar hitam
            yield return new WaitForSeconds(blackScreenDuration);
        }
        
        // 3. Ganti gambar
        ShowSlide(targetIndex);
        
        // 4. Fade from black
        yield return StartCoroutine(FadeBlackScreen(1f, 0f, fadeFromBlackDuration));
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Tampilkan teks di layar hitam dan tunggu input user
    /// Bisa di-skip dengan 1x klik jika typing terlalu lama, atau klik untuk lanjut setelah selesai
    /// </summary>
    private IEnumerator ShowTransitionText(string text)
    {
        if (transitionTextUI == null) yield break;
        
        transitionTextUI.gameObject.SetActive(true);
        fullText = text;
        waitingForInput = false; // Reset state
        
        if (useTypewriterOnTransition)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterEffect(text, transitionTextUI));
            
            // Tunggu sampai selesai mengetik (bisa di-skip dengan klik)
            while (isTyping)
            {
                yield return null;
            }
        }
        else
        {
            transitionTextUI.text = text;
        }
        
        // Setelah typing selesai (atau di-skip), tunggu input user untuk lanjut ke slide berikutnya
        waitingForInput = true;
        while (waitingForInput)
        {
            yield return null;
        }
        
        transitionTextUI.gameObject.SetActive(false);
    }
    
    private IEnumerator FadeBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (blackScreen == null) yield break;
        
        float elapsed = 0f;
        Color color = blackScreen.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            blackScreen.color = color;
            yield return null;
        }
        
        color.a = toAlpha;
        blackScreen.color = color;
    }
    
    private void UpdateContinuePrompt()
    {
        if (continuePrompt != null)
        {
            continuePrompt.SetActive(currentIndex < slides.Length - 1);
        }
    }
    
    public void SkipCutscene()
    {
        StopAllCoroutines();
        EndCutscene();
    }
    
    private void EndCutscene()
    {
        // Jika ada end screen panel, tampilkan itu
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
            
            // Setup button listeners
            if (playAgainButton != null)
            {
                playAgainButton.onClick.RemoveAllListeners();
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            }
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(OnExitClicked);
            }
            
            Debug.Log("End screen displayed with buttons");
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called when Play Again button is clicked
    /// </summary>
    public void OnPlayAgainClicked()
    {
        Debug.Log("Play Again clicked from cutscene!");
        
        // Load first level
        if (!string.IsNullOrEmpty(firstLevelScene))
        {
            SceneManager.LoadScene(firstLevelScene);
        }
    }
    
    /// <summary>
    /// Called when Exit button is clicked
    /// </summary>
    public void OnExitClicked()
    {
        Debug.Log("Exit clicked from cutscene!");
        
        // Load main menu
        if (!string.IsNullOrEmpty(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    /// <summary>
    /// Memainkan sound effect dengan volume yang dikonfigurasi
    /// </summary>
    /// <param name="clip">Audio clip SFX yang akan dimainkan</param>
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip, sfxVolume);
        }
    }
}
