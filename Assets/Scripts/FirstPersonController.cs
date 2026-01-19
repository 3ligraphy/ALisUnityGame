using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Touch Look Settings")]
    [Tooltip("Sensitivity for touch-based camera rotation")]
    public float touchSensitivity = 0.15f;
    [Tooltip("Mouse sensitivity for desktop testing")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("Joystick (Optional)")]
    public VariableJoystick joystick;

    [Header("UI Control")]
    public bool uiOpen = false;

    [Header("Touch Zones")]
    [Tooltip("The left portion of the screen reserved for joystick (0.0-1.0)")]
    public float joystickZoneWidth = 0.4f;
    [Tooltip("The bottom portion of screen for joystick (0.0-1.0)")]
    public float joystickZoneHeight = 0.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    
    // Touch tracking for multi-touch support
    private int lookTouchId = -1;  // ID of the finger used for camera look
    private Vector2 lastLookTouchPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Cursor visible on mobile
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Enable multi-touch
        Input.multiTouchEnabled = true;
        
        // Try to find joystick if not assigned
        if (joystick == null)
        {
            joystick = FindObjectOfType<VariableJoystick>();
            if (joystick != null)
            {
                Debug.Log("FirstPersonController: Found VariableJoystick automatically");
            }
            else
            {
                Debug.LogWarning("FirstPersonController: No VariableJoystick found in scene!");
            }
        }
    }

    void Update()
    {
        // ALWAYS allow movement (so player can walk away from popup)
        HandleMovement();
        
        // If popup/UI is open, only disable camera rotation (not movement)
        if (uiOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            lookTouchId = -1;  // Reset look touch - no camera rotation while UI open
            // Skip camera look and jump, but movement still works above
            return;
        }

        // Only process camera look when UI is NOT open
        HandleTouchLook();
        HandleMouseLook();  // For desktop/editor testing
        HandleJump();
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Get joystick input - this is the PRIMARY input for mobile
        if (joystick != null)
        {
            float joyX = joystick.Horizontal;
            float joyZ = joystick.Vertical;
            
            // Add joystick input to movement
            x += joyX;
            z += joyZ;
        }

        // Clamp to prevent faster diagonal movement
        Vector2 inputVector = new Vector2(x, z);
        if (inputVector.magnitude > 1f)
        {
            inputVector.Normalize();
            x = inputVector.x;
            z = inputVector.y;
        }

        // Apply movement
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Handle gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Handle touch-based camera look for iOS/mobile with proper multi-touch support.
    /// Only uses touches on the RIGHT side of the screen (outside joystick zone).
    /// </summary>
    void HandleTouchLook()
    {
        if (Input.touchCount == 0)
        {
            lookTouchId = -1;
            return;
        }

        // Process all touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            
            // Handle new touch - find one on the right side of the screen for look
            if (touch.phase == TouchPhase.Began)
            {
                // Skip if this touch is in the joystick zone (left/bottom)
                if (IsTouchInJoystickZone(touch.position))
                    continue;
                
                // Skip if touching UI elements
                if (IsTouchOverUI(touch.fingerId))
                    continue;
                
                // This touch is valid for camera look
                if (lookTouchId == -1)
                {
                    lookTouchId = touch.fingerId;
                    lastLookTouchPosition = touch.position;
                }
            }
            // Handle movement of the look touch
            else if (touch.fingerId == lookTouchId)
            {
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    // Calculate delta movement
                    Vector2 delta = touch.position - lastLookTouchPosition;
                    lastLookTouchPosition = touch.position;
                    
                    // Apply camera rotation based on touch delta
                    float rotateX = delta.x * touchSensitivity;
                    float rotateY = delta.y * touchSensitivity;
                    
                    // Rotate player (horizontal look)
                    transform.Rotate(Vector3.up * rotateX);
                    
                    // Rotate camera (vertical look)
                    xRotation -= rotateY;
                    xRotation = Mathf.Clamp(xRotation, -80f, 80f);
                    playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    lookTouchId = -1;
                }
            }
        }
    }

    /// <summary>
    /// Handle mouse look for desktop/editor testing only.
    /// On mobile, this is disabled in favor of touch.
    /// </summary>
    void HandleMouseLook()
    {
        // Skip on mobile - use touch instead
        if (Input.touchCount > 0)
            return;
        
        // Skip if clicking on UI
        if (Input.GetMouseButton(0) && EventSystem.current != null && 
            EventSystem.current.IsPointerOverGameObject())
            return;
        
        // Only process if right mouse button is held (for desktop testing)
        // Or if left click is not on UI
        if (Input.GetMouseButton(1) || 
            (Input.GetMouseButton(0) && !IsMouseOverUI()))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    /// <summary>
    /// Checks if a touch position is in the joystick zone (left/bottom of screen)
    /// </summary>
    private bool IsTouchInJoystickZone(Vector2 touchPosition)
    {
        float screenWidthThreshold = Screen.width * joystickZoneWidth;
        float screenHeightThreshold = Screen.height * joystickZoneHeight;
        
        return touchPosition.x < screenWidthThreshold && 
               touchPosition.y < screenHeightThreshold;
    }

    /// <summary>
    /// Checks if a specific touch is over a UI element using EventSystem
    /// </summary>
    private bool IsTouchOverUI(int fingerId)
    {
        if (EventSystem.current == null)
            return false;
            
        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    /// <summary>
    /// Checks if mouse is over UI (for desktop testing)
    /// </summary>
    private bool IsMouseOverUI()
    {
        if (EventSystem.current == null)
            return false;
            
        return EventSystem.current.IsPointerOverGameObject();
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // Called from popup scripts when UI opens/closes
    public void SetUIOpen(bool isOpen)
    {
        uiOpen = isOpen;
        
        if (isOpen)
        {
            lookTouchId = -1;  // Reset look touch when UI opens
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
