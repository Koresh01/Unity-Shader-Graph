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

    public UnityAction<Vector3> WASDchanged;

    [Header("Камера:")]
    [SerializeField] Camera _camera;
    [SerializeField] Transform head;  // Цель куда смотрит камера(голова игрока).
    [SerializeField] float horizontalAngle = 0f;       // Горизонтальный угол относительно персонажа
    [SerializeField] float verticalAngle = 20f;    // Вертикальный угол (вверх/вниз)
    [SerializeField] float distance = 5f;  // Текущее расстояние до игрока

    [SerializeField] float smoothSpeed = 5f;

    [Header("Параметры перемещения:")]
    [SerializeField] float moveSpeed = 1f;

    [Header("Сила гравитации:")]
    [SerializeField] float gravity = 9.8f;

    Vector3 _moveDirection;
    Vector2 _mouseDelta;

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
        horizontalAngle += _mouseDelta.x * 0.05f;
        verticalAngle -= _mouseDelta.y * 0.05f;
    }

    private void Update()
    {
        HandleGravity();

        // Игрок
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

        // Нормализуем чтобы диагональное движение не было быстрее
        if (horizontalMovement.magnitude > 1f)
            horizontalMovement.Normalize();

        // Создаем полный вектор движения: горизонтальное + вертикальное (гравитация)
        Vector3 totalMovement = (horizontalMovement * moveSpeed * Time.deltaTime) +
                               (Vector3.up * _moveDirection.y * Time.deltaTime);

        _characterController.Move(totalMovement);
        WASDchanged?.Invoke(horizontalMovement);
    }

    /// <summary>
    /// Вращение персонажа в сторону направления его движения.
    /// </summary>
    void ApplyRotation()
    {
        // Преобразуем локальное направление в глобальное относительно камеры
        Vector3 cameraForward = _camera.transform.forward;
        Vector3 cameraRight = _camera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        // Создаем глобальное направление движения
        Vector3 positionToLookAt = (cameraForward * _moveDirection.z) + (cameraRight * _moveDirection.x);

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
            _moveDirection.y = Mathf.Max(_moveDirection.y, -gravity * 2f);
        }
    }
}