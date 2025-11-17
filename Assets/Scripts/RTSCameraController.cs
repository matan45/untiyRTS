using UnityEngine;
using UnityEngine.InputSystem;

public class RTSCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float panBorderThickness = 10f;
    [SerializeField] private bool useEdgeScrolling = false;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 50f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;

    [Header("Boundaries")]
    [SerializeField] private Vector2 panLimitX = new Vector2(-50f, 50f);
    [SerializeField] private Vector2 panLimitZ = new Vector2(-50f, 50f);

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private Vector3 currentPosition;
    private float currentZoom;
    private bool isDragging = false;
    private Vector2 lastMousePosition;

    // Input Actions
    private InputAction movementAction;
    private InputAction rotateAction;
    private InputAction zoomAction;
    private InputAction mousePositionAction;
    private InputAction middleMouseDragAction;

    void OnEnable()
    {
        // Get the Camera action map
        var cameraMap = inputActions.FindActionMap("Camera");

        // Get individual actions
        movementAction = cameraMap.FindAction("Movement");
        rotateAction = cameraMap.FindAction("Rotate");
        zoomAction = cameraMap.FindAction("Zoom");
        mousePositionAction = cameraMap.FindAction("MousePosition");
        middleMouseDragAction = cameraMap.FindAction("MiddleMouseDrag");

        // Enable all actions
        cameraMap.Enable();

        // Subscribe to middle mouse button events
        middleMouseDragAction.started += OnMiddleMouseStart;
        middleMouseDragAction.canceled += OnMiddleMouseEnd;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        middleMouseDragAction.started -= OnMiddleMouseStart;
        middleMouseDragAction.canceled -= OnMiddleMouseEnd;

        // Disable all actions
        inputActions.FindActionMap("Camera").Disable();
    }

    void Start()
    {
        currentPosition = transform.position;
        currentZoom = transform.position.y;
    }

    void Update()
    {
        HandleKeyboardMovement();
        HandleMouseEdgeScrolling();
        HandleMouseDragPan();
        HandleZoom();
        HandleRotation();

        // Apply movement with boundaries
        ApplyMovement();
    }

    void HandleKeyboardMovement()
    {
        Vector2 inputVector = movementAction.ReadValue<Vector2>();

        if (inputVector.sqrMagnitude > 0)
        {
            Vector3 move = transform.forward * inputVector.y + transform.right * inputVector.x;
            move.y = 0;
            currentPosition += move.normalized * panSpeed * Time.deltaTime;
        }
    }

    void HandleMouseEdgeScrolling()
    {
        if (!useEdgeScrolling) return;

        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Vector3 move = Vector3.zero;

        // Edge scrolling
        if (mousePos.x >= Screen.width - panBorderThickness)
            move += transform.right;
        if (mousePos.x <= panBorderThickness)
            move -= transform.right;
        if (mousePos.y >= Screen.height - panBorderThickness)
            move += transform.forward;
        if (mousePos.y <= panBorderThickness)
            move -= transform.forward;

        move.y = 0;
        currentPosition += move.normalized * panSpeed * Time.deltaTime;
    }

    void HandleMouseDragPan()
    {
        if (isDragging)
        {
            Vector2 currentMousePos = mousePositionAction.ReadValue<Vector2>();
            Vector2 delta = currentMousePos - lastMousePosition;
            Vector3 move = -transform.right * delta.x * 0.1f - transform.forward * delta.y * 0.1f;
            move.y = 0;
            currentPosition += move;
            lastMousePosition = currentMousePos;
        }
    }

    void HandleZoom()
    {
        float scroll = zoomAction.ReadValue<float>();
        currentZoom -= scroll * zoomSpeed * 0.1f; // Scaled down for better feel
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    void HandleRotation()
    {
        float rotateInput = rotateAction.ReadValue<float>();
        if (Mathf.Abs(rotateInput) > 0.01f)
        {
            transform.Rotate(Vector3.up, rotateInput * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    void ApplyMovement()
    {
        // Clamp position to boundaries
        currentPosition.x = Mathf.Clamp(currentPosition.x, panLimitX.x, panLimitX.y);
        currentPosition.z = Mathf.Clamp(currentPosition.z, panLimitZ.x, panLimitZ.y);
        currentPosition.y = currentZoom;

        // Smoothly move camera
        transform.position = Vector3.Lerp(transform.position, currentPosition, Time.deltaTime * 10f);
    }

    void OnMiddleMouseStart(InputAction.CallbackContext context)
    {
        isDragging = true;
        lastMousePosition = mousePositionAction.ReadValue<Vector2>();
    }

    void OnMiddleMouseEnd(InputAction.CallbackContext context)
    {
        isDragging = false;
    }
}