using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    InputControls inputControls;
    Rigidbody rb;
    Transform cameraTransform;

    [Header("Параметры движения:")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float lookSpeed = 2f;

    [Header("Ввод от пользователя:")]
    [SerializeField] Vector2 moveDelta;
    [SerializeField] Vector2 lookDelta;

    float cameraPitch = 0f; // Для ограничения поворота камеры по оси X

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputControls = new InputControls();
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        inputControls.Enable();
        inputControls.PlayerMove.WASD.performed += OnMove;
        inputControls.PlayerMove.WASD.canceled += ctx => moveDelta = Vector2.zero;
        inputControls.PlayerMove.LOOK.performed += OnLook;
        inputControls.PlayerMove.LOOK.canceled += ctx => lookDelta = Vector2.zero;
    }

    private void OnDisable()
    {
        inputControls.PlayerMove.WASD.performed -= OnMove;
        inputControls.PlayerMove.LOOK.performed -= OnLook;
        inputControls.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveDelta = ctx.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        lookDelta = ctx.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        HandleMove();
        HandleLook();
    }

    void HandleMove()
    {
        // Двигаем игрока в пространстве
        Vector3 move = transform.forward * moveDelta.y + transform.right * moveDelta.x;
        rb.velocity = new Vector3(move.x * moveSpeed, rb.velocity.y, move.z * moveSpeed);
    }

    void HandleLook()
    {
        // Вращение персонажа в горизонтальной плоскости
        transform.Rotate(Vector3.up * lookDelta.x * lookSpeed);

        // Вращение камеры вверх/вниз с ограничением угла
        cameraPitch -= lookDelta.y * lookSpeed;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }
}
