using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("Joystick (Optional)")]
    public VariableJoystick joystick;

    [Header("UI Control")]
    public bool uiOpen = false; // لو UI فاتح هوقف الماوس و الحركة

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // الماوس يكون ظاهر
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Auto-find joystick if not assigned
        if (joystick == null)
        {
            // Try to find any joystick type
            joystick = FindObjectOfType<VariableJoystick>();
            
            // If still null, try to find by name
            if (joystick == null)
            {
                GameObject joystickObj = GameObject.Find("Variable Joystick");
                if (joystickObj != null)
                {
                    joystick = joystickObj.GetComponent<VariableJoystick>();
                }
            }
        }
        
        // Log joystick status for debugging
        if (joystick != null)
        {
            Debug.Log($"FirstPersonController: Joystick found - {joystick.gameObject.name}");
        }
        else
        {
            Debug.LogError("FirstPersonController: NO JOYSTICK FOUND! Mobile controls will not work!");
        }
        
        // Log EventSystem status
        if (EventSystem.current != null)
        {
            Debug.Log("FirstPersonController: EventSystem found");
        }
        else
        {
            Debug.LogError("FirstPersonController: NO EventSystem! UI input will not work!");
        }
    }

    void Update()
    {
        // لو الـ Popup مفتوح → امنع الحركة ولف الكاميرا
        if (uiOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        HandleMovement();
        HandleMouseLook();
        HandleJump();
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Get joystick input - this is the PRIMARY input method on mobile
        if (joystick != null)
        {
            // Use joystick values directly (not adding, as keyboard won't be used on mobile)
            float joyX = joystick.Horizontal;
            float joyZ = joystick.Vertical;
            
            // Only use joystick if it has significant input (dead zone)
            if (Mathf.Abs(joyX) > 0.1f || Mathf.Abs(joyZ) > 0.1f)
            {
                x = joyX;
                z = joyZ;
            }
        }

        // Apply movement
        if (Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f)
        {
            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * moveSpeed * Time.deltaTime);
        }

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        // Skip camera rotation when touching UI elements (like the joystick)
        // This prevents the camera from rotating while using the movement joystick on mobile
        if (IsTouchingUI())
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// Checks if any touch or mouse click is over a UI element.
    /// Used to prevent camera rotation when interacting with UI (joystick, buttons, etc.)
    /// More reliable version for iOS that handles edge cases.
    /// </summary>
    private bool IsTouchingUI()
    {
        // Safety check - if no EventSystem, assume not touching UI
        if (EventSystem.current == null)
            return false;
            
        // Check for touch input on mobile devices
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                // Skip ended/cancelled touches
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    continue;
                
                // Use try-catch because IsPointerOverGameObject can fail on iOS in some cases
                try
                {
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return true;
                    }
                }
                catch
                {
                    // If it fails, check if we're touching the joystick area directly
                    if (joystick != null && IsTouchInJoystickArea(touch.position))
                    {
                        return true;
                    }
                }
            }
        }
        // Check for mouse input (for editor testing and desktop)
        else if (Input.GetMouseButton(0))
        {
            try
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Fallback check if touch is in joystick area (for when EventSystem fails)
    /// </summary>
    private bool IsTouchInJoystickArea(Vector2 touchPosition)
    {
        if (joystick == null) return false;
        
        RectTransform joystickRect = joystick.GetComponent<RectTransform>();
        if (joystickRect == null) return false;
        
        // Simple check: if touch is in bottom-left quadrant, assume joystick
        // Joystick is typically positioned in bottom-left
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        return touchPosition.x < screenWidth * 0.4f && touchPosition.y < screenHeight * 0.5f;
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // نستدعي الدالة دي من سكربت الكيوب عند فتح/قفل الـ UI
    public void SetUIOpen(bool isOpen)
    {
        uiOpen = isOpen;

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
