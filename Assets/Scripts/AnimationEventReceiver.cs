using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        // Find the main script on the parent object
        playerController = GetComponentInParent<PlayerController>();
    }

    // This function name matches the Error message "Hit" exactly
    public void Hit()
    {
        if (playerController != null)
        {
            playerController.OnAttackHit();
        }
    }

    // Asset packs often include footstep events too. 
    // Adding these empty functions prevents errors if your animation has them.
    public void FootR() { }
    public void FootL() { }
    public void Land() { }
}