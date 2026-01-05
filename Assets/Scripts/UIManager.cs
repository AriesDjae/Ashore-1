using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro is used, if not use UnityEngine.UI for Text

public class UIManager : MonoBehaviour
{
    [Header("Player References")]
    public PlayerStats playerStats;

    [Header("HUD Elements")]
    public Image healthBarFill;
    public Image staminaBarFill;
    public Image xpBarFill;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI skillPointsText;

    [Header("Panels")]
    public GameObject skillTreePanel;
    public GameObject hudPanel;

    private bool isSkillTreeOpen = false;

    void Start()
    {
        // Close skill tree on start
        if(skillTreePanel) skillTreePanel.SetActive(false);
    }

    void Update()
    {
        UpdateHUD();
        HandleInput();
    }

    void HandleInput()
    {
        // Toggle Skill Tree with 'K' or 'Tab' (Conflict check needed with camera tab)
        if (Input.GetKeyDown(KeyCode.K)) 
        {
            ToggleSkillTree();
        }
    }

    public void ToggleSkillTree()
    {
        isSkillTreeOpen = !isSkillTreeOpen;
        skillTreePanel.SetActive(isSkillTreeOpen);
        
        // Find camera and tell if UI is open/closed
        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if(cam) cam.SetUIState(isSkillTreeOpen);
    }

    void UpdateHUD()
    {
        if (playerStats == null) return;

        // Update Bars
        if(healthBarFill) 
            healthBarFill.fillAmount = playerStats.currentHealth / playerStats.maxHealth;
        
        if(staminaBarFill) 
            staminaBarFill.fillAmount = playerStats.currentStamina / playerStats.maxStamina;

        if(xpBarFill)
            xpBarFill.fillAmount = playerStats.currentXP / playerStats.maxXP;

        // Update Text
        if(moneyText) 
            moneyText.text = "$" + playerStats.currentMoney.ToString();

        if(skillPointsText)
            skillPointsText.text = "SP: " + playerStats.skillPoints.ToString();
    }
}