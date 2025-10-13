using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform cameraTransform; // Ссылка на Main Camera

    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Чтобы Rigidbody не падал и не вращался
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("Не назначена камера в PlayerMovement!");
            return;
        }

        // Получаем ввод от пользователя (WASD / стрелки)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Направление движения относительно ориентации камеры
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Игнорируем наклон камеры (чтобы движение было по земле)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Итоговое направление движения
        moveDirection = forward * vertical + right * horizontal;
        moveDirection.Normalize();

        // Двигаем персонажа с помощью Rigidbody
        Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        // Если персонаж движется — поворачиваем в направлении движения
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
