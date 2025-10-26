using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Необходимые ссылки:")]
    [SerializeField] PlayerMovement playerMovement;

    [Header("Настройки анимации:")]
    [SerializeField] private float dampTime = 0.1f; // Плавность переходов анимации

    private Animator animator;
    private int hash_Velocity;
    private int hash_IsRunning;
    private int hash_IsCrouching;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Кэшируем хэши параметров — это ускоряет работу
        hash_Velocity = Animator.StringToHash("velocity");
        hash_IsRunning = Animator.StringToHash("IsRunning");
        hash_IsCrouching = Animator.StringToHash("IsCrouching");
    }

    void OnEnable()
    {
        playerMovement.Jump += HandleJumping;
        playerMovement.WASDchanged += HandleWASD;
    }

    void OnDisable()
    {
        playerMovement.Jump += HandleJumping;
        playerMovement.WASDchanged -= HandleWASD;
    }

    void HandleWASD(Vector3 moveDirection)
    {
        animator.SetBool(hash_IsRunning, playerMovement.isRunPressed);
        animator.SetBool(hash_IsCrouching, playerMovement.isCrouchPressed);

        // Плавно изменяем параметры аниматора
        animator.SetFloat(hash_Velocity, moveDirection.magnitude, dampTime, Time.deltaTime);
    }

    void HandleJumping()
    {
        animator.SetTrigger("Jump");
    }
}
