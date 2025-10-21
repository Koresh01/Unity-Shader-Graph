using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

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
    [SerializeField] float distance = 5f;  // Текущее расстояние до игрока
    [SerializeField] float mouseSens = 1f;  // Чувствительность мыши/стика.
    Vector3 dir;

    [Header("Параметры перемещения:")]
    [Tooltip("Скорость поворота персонажа к заданной точке.")]
    [SerializeField] float smoothSpeed = 5f;
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

        // Запоминаем направление взгляда камеры на шею игрока.
        dir = _camera.transform.position - head.transform.position;
    }

    private void OnEnable()
    {
        _playerInput.Enable();
        _playerInput.CharacterControls.WASD.performed += OnMove;
        _playerInput.CharacterControls.WASD.canceled += ctx => _moveDirection = Vector2.zero;
        _playerInput.CharacterControls.LOOK.performed += OnLook;
        _playerInput.CharacterControls.LOOK.canceled += ctx => _mouseDelta = Vector2.zero;



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
        _playerInput.CharacterControls.WASD.performed -= OnMove;
        _playerInput.CharacterControls.WASD.canceled -= ctx => _moveDirection = Vector2.zero;
        _playerInput.CharacterControls.LOOK.performed -= OnLook;
        _playerInput.CharacterControls.LOOK.canceled -= ctx => _mouseDelta = Vector2.zero;



        _playerInput.CharacterControls.Run.started  -= HandleRun;
        _playerInput.CharacterControls.Run.canceled -= HandleRun;

        _playerInput.CharacterControls.Jump.started -= HandleJump;
        _playerInput.CharacterControls.Jump.canceled -= HandleJump;

        // ВЫКЛЮЧАЕМ КУРСОР при включении
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _moveDirection.x = input.x;
        _moveDirection.z = input.y;
    }

    void OnLook(InputAction.CallbackContext context)
    {
        _mouseDelta = context.ReadValue<Vector2>();
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


    #region Camera

    /// <summary>
    /// Вращение камеры от 3 лица.
    /// </summary>
    void UpdateCameraPosition()
    {
        // Вращение камеры по орбите.
        dir = dir.normalized;


        Quaternion corRot = Quaternion.Euler(
            -_mouseDelta.y * mouseSens * Time.deltaTime,
            _mouseDelta.x * mouseSens * Time.deltaTime,
            0
        );
        Vector3 newDir = corRot * dir;

        _camera.transform.position = head.transform.position + newDir * distance;
        _camera.transform.rotation = Quaternion.LookRotation(-newDir);

        dir = newDir;
    }
    
    #endregion
}