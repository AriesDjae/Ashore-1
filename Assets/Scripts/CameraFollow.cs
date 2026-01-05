using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    public enum CameraMode { ThirdPerson, Isometric }
    [Header("Mode Settings")]
    public CameraMode currentMode = CameraMode.ThirdPerson;
    public LayerMask collisionLayers; 

    [Header("Third Person (Orbit)")]
    public float mouseSensitivity = 2f;
    public float distanceFromTarget = 5f;
    public Vector2 pitchLimits = new Vector2(-10, 60); 
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0); 
    public float rotationSmoothTime = 0.12f;

    [Header("Isometric (Fixed)")]
    public Vector3 isoOffset = new Vector3(-7, 5, -7);
    public float isoSmoothSpeed = 5f;

    // Internal State
    private Vector3 currentRotationVel;
    private Vector3 targetRotation;
    private float yaw;
    private float pitch;

    // Helper to allow UI to unlock cursor without Camera fighting it
    public bool isUIOpen = false;

    void Start()
    {
        // Initialize angles
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // Apply initial cursor state
        UpdateCursorState();
    }

    void LateUpdate()
    {
        if (!target) return;

        if (currentMode == CameraMode.ThirdPerson)
        {
            HandleOrbitCamera();
        }
        else
        {
            HandleIsometricCamera();
        }
    }

    void HandleOrbitCamera()
    {
        // Only rotate camera if UI is NOT open
        if (!isUIOpen)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
        }

        targetRotation = new Vector3(pitch, yaw);
        Quaternion finalRotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 desiredPosition = target.position + targetOffset - (finalRotation * Vector3.forward * distanceFromTarget);

        transform.rotation = finalRotation;
        transform.position = desiredPosition;
    }

    void HandleIsometricCamera()
    {
        Vector3 desiredPosition = target.position + isoOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, isoSmoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
        transform.LookAt(target.position + Vector3.up); 
    }

    void Update()
    {
        // Toggle Modes with Tab
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            // Switch Enum
            currentMode = (currentMode == CameraMode.ThirdPerson) ? CameraMode.Isometric : CameraMode.ThirdPerson;
            
            // Apply Cursor Logic
            UpdateCursorState();
        }
    }

    public void UpdateCursorState()
    {
        // If UI is open, always unlock cursor
        if (isUIOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Otherwise, depend on Camera Mode
        if (currentMode == CameraMode.ThirdPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else // Isometric
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    // Call this from UIManager
    public void SetUIState(bool isOpen)
    {
        isUIOpen = isOpen;
        UpdateCursorState();
    }
}