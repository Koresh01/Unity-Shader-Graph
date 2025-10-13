using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Настройки анимации:")]
    [SerializeField] private float dampTime = 0.1f; // Плавность переходов анимации

    private Animator animator;
    private int hashHorizontal;
    private int hashForward;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Кэшируем хэши параметров — это ускоряет работу
        hashHorizontal = Animator.StringToHash("horizontal");
        hashForward = Animator.StringToHash("forward");
    }

    private void Update()
    {
        // Получаем ввод от пользователя (старая система)
        float horizontal = Input.GetAxis("Horizontal"); // A/D или стрелки ← →
        float forward = Input.GetAxis("Vertical");       // W/S или стрелки ↑ ↓

        // Плавно изменяем параметры аниматора
        animator.SetFloat(hashHorizontal, horizontal, dampTime, Time.deltaTime);
        animator.SetFloat(hashForward, forward, dampTime, Time.deltaTime);
    }
}
