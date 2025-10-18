using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Необходимые ссылки:")]
    [SerializeField] PlayerMovement playerMovement;

    [Header("Настройки анимации:")]
    [SerializeField] private float dampTime = 0.1f; // Плавность переходов анимации

    private Animator animator;
    private int hasVelocity;
    private int hashForward;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Кэшируем хэши параметров — это ускоряет работу
        hasVelocity = Animator.StringToHash("velocity");
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
        // Плавно изменяем параметры аниматора
        animator.SetFloat(hasVelocity, moveDirection.magnitude, dampTime, Time.deltaTime);
    }

    void HandleJumping()
    {
        animator.SetTrigger("Jump");
    }
}
