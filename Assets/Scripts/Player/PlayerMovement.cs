using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    CharacterController _characterController;
    PlayerInput _playerInput;

    [Header("Параметры перемещения:")]
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] Vector3 _currentMovement;

    [Header("Параметры камеры:")]
    [SerializeField] float verticalAngle;
    [SerializeField] float horizontalAngle;
    [SerializeField] float distance;

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
    }

    private void OnDisable()
    {
        _playerInput.Disable();
        _playerInput.CharacterControls.Move.performed -= HandleWASD;
        _playerInput.CharacterControls.Move.canceled -= HandleWASD;
    }

    void HandleWASD(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _currentMovement = new Vector3(input.x, 0, input.y);
    }

    private void Update()
    {
        ApplyMovement();
    }

    void ApplyMovement()
    {
        _characterController.Move(_currentMovement * moveSpeed * Time.deltaTime);
    }
}