using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    CharacterController _characterController;
    PlayerInput _playerInput;

    public UnityAction Jump;
    public UnityAction<Vector3> WASDchanged;

    [Header("Камера:")]
    [SerializeField] Camera _camera;
    [SerializeField] Transform head;  // Цель куда смотрит камера(голова игрока).
    [SerializeField] float horizontalAngle = 0f;       // Горизонтальный угол относительно персонажа
    [SerializeField] float verticalAngle = 20f;    // Вертикальный угол (вверх/вниз)
    [SerializeField] float distance = 5f;  // Текущее расстояние до игрока

    [Tooltip("Скорость поворота персонажа к заданной точке.")]
    [SerializeField] float smoothSpeed = 5f;

    [Header("Параметры перемещения:")]
    [SerializeField] float walkSpeed = 1.5f;
    [SerializeField, Range(1f, 3f)] float runSpeedMultiplier = 2f;
    [SerializeField] float jumpForce = 30f;

    [SerializeField] bool isRunPressed;
    [SerializeField] bool isJumpPressed;


    [Header("Сила гравитации:")]
    [SerializeField] float gravity = 9.8f;

    [Header("Ссчитываем от пользователя:")]
    [SerializeField] Vector3 _moveDirection;
    [SerializeField] Vector2 _mouseDelta;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        _playerInput.Enable();
        _playerInput.CharacterControls.Move.performed += HandleWASD;
        _playerInput.CharacterControls.Move.canceled += HandleWASD;

        _playerInput.CharacterControls.Look.performed += HandleLook;
        _playerInput.CharacterControls.Look.canceled += HandleLook;

        _playerInput.CharacterControls.Run.started += HandleRun;
        _playerInput.CharacterControls.Run.canceled += HandleRun;

        _playerInput.CharacterControls.Jump.started += HandleJump;
        _playerInput.CharacterControls.Jump.canceled += HandleJump;

        // ВЫКЛЮЧАЕМ КУРСОР при включении
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        _playerInput.Disable();
        
        _playerInput.CharacterControls.Move.performed -= HandleWASD;
        _playerInput.CharacterControls.Move.canceled -= HandleWASD;

        _playerInput.CharacterControls.Look.performed -= HandleLook;
        _playerInput.CharacterControls.Look.canceled -= HandleLook;

        _playerInput.CharacterControls.Run.started  -= HandleRun;
        _playerInput.CharacterControls.Run.canceled -= HandleRun;

        _playerInput.CharacterControls.Jump.started -= HandleJump;
        _playerInput.CharacterControls.Jump.canceled -= HandleJump;

        // ВЫКЛЮЧАЕМ КУРСОР при включении
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    void HandleWASD(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _moveDirection.x = input.x;
        _moveDirection.z = input.y;
    }

    void HandleLook(InputAction.CallbackContext context)
    {
        _mouseDelta = context.ReadValue<Vector2>();
        horizontalAngle += _mouseDelta.x;
        verticalAngle -= _mouseDelta.y;
    }

    void HandleRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void HandleJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }







    private void Update()
    {
        HandleGravity();

        // Игрок
        ApplyJump();
        ApplyMovement();
        ApplyRotation();

        // Камера
        UpdateCameraPosition();
    }

    /// <summary>
    /// Перемещение персонажа относительно вида камеры.
    /// </summary>
    void ApplyMovement()
    {
        // Преобразуем локальное направление в глобальное относительно камеры
        Vector3 cameraForward = _camera.transform.forward;
        Vector3 cameraRight = _camera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        // Создаем глобальное направление движения (только горизонтальное)
        Vector3 horizontalMovement = (cameraRight * _moveDirection.x) + (cameraForward * _moveDirection.z);
        
        // Если зажат shift, то бежим
        if (isRunPressed)
            horizontalMovement *= runSpeedMultiplier;



        // Создаем полный вектор движения: горизонтальное + вертикальное (гравитация)
        Vector3 totalMovement = (horizontalMovement * walkSpeed * Time.deltaTime) +
                               (Vector3.up * _moveDirection.y * Time.deltaTime);




        _characterController.Move(totalMovement);



        float animationSpeed = isRunPressed ? 1 : 0.5f; // 0.5f - ходьба 1.0f - бег
        WASDchanged?.Invoke(horizontalMovement.normalized * animationSpeed);
    }

    /// <summary>
    /// Вращение персонажа в сторону направления его движения.
    /// </summary>
    void ApplyRotation()
    {
        if (_moveDirection.magnitude < 0.3f) return;

        // Преобразуем локальное направление в глобальное относительно камеры
        Vector3 cameraForward = _camera.transform.forward;
        Vector3 cameraRight = _camera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        // Создаем глобальное направление движения
        Vector3 positionToLookAt = (cameraForward * _moveDirection.z) + (cameraRight * _moveDirection.x);

        if (positionToLookAt == Vector3.zero) return;   // Если пользователь не жмёт WASD, то вращать не нужно.

        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);

        
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Вращение камеры от 3 лица.
    /// </summary>
    void UpdateCameraPosition()
    {
        verticalAngle = Mathf.Clamp(verticalAngle, -60f, 60f);

        // Вычисляем позицию камеры на сфере вокруг игрока
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
        Vector3 offset = rotation * new Vector3(0,0,-distance);

        // Устанавливаем позицию и направление
        Vector3 targetPosition = head.position + offset;

        // Плавное перемещение
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        _camera.transform.LookAt(head.position);
    }

    void ApplyJump()
    {
        if (isJumpPressed && _characterController.isGrounded)
        {
            _moveDirection.y = jumpForce;
            Jump?.Invoke();
        }
    }

    /// <summary>
    /// Обработка гравитации.
    /// </summary>
    void HandleGravity()
    {
        if (_characterController.isGrounded)
        {
            _moveDirection.y = -0.5f;
        }
        else
        {
            // Используем отрицательное значение гравитации
            _moveDirection.y -= gravity * Time.deltaTime;
            _moveDirection.y = Mathf.Max(_moveDirection.y, -gravity * 2f);  // Ограничим максимальную скорость падения.
        }
    }
}