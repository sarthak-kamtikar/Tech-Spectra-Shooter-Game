using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Needed for UI Image

public class PlayerController : MonoBehaviour
{
    [Header("Look Settings")]
    public float mouseSensitivity = 1.5f;
    public Transform cameraTransform;

    [Header("Scope Settings")]
    public float normalFOV = 60f;      // Default FOV
    public float scopedFOV = 20f;      // Zoomed-in FOV
    public float scopeSpeed = 10f;     // Speed of zoom transition
    public Image scopeUI;              // Scope overlay (UI Image on Canvas)

    private InputSystem_Actions inputActions;
    private Vector2 lookInput;
    private float cameraPitch;
    private bool isScoped = false;

    private Camera cam;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        cam = cameraTransform.GetComponent<Camera>();

        if (cam == null)
            Debug.LogError("CameraTransform must have a Camera component!");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hide scope overlay at start
        if (scopeUI != null)
            scopeUI.enabled = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Scope.performed += ctx => ToggleScope(true);
        inputActions.Player.Scope.canceled += ctx => ToggleScope(false);
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        HandleLook();
        HandleScope();
        lookInput = Vector2.zero;
    }

    private void HandleLook()
    {
        if (cameraTransform == null) return;

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime * 100f;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void ToggleScope(bool enable)
    {
        isScoped = enable;

        // Show the 2D scope sprite overlay
        if (scopeUI != null)
            scopeUI.enabled = enable;
    }

    private void HandleScope()
    {
        if (cam == null) return;

        float targetFOV = isScoped ? scopedFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, scopeSpeed * Time.deltaTime);
    }
}
