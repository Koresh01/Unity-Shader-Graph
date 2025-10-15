using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    public UnityAction Jump;
    public UnityAction WASDchanged;

    [Header("Основные настройки")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [SerializeField] private Transform cameraTransform;

    [Header("Прыжок")]
    [SerializeField] private float jumpForce = 5f;
    public bool isGrounded;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()   // чуткое ослеживание нажатия пробела
    {
        CheckGround();
        HandleJumping();
    }

    private void FixedUpdate()  // применения силы
    {
        HandleMovement();   
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float rayLength = capsule.bounds.extents.y + 0.2f;
        isGrounded = Physics.Raycast(origin, Vector3.down, rayLength);
    }

    private void HandleMovement()
    {
        // if (!isGrounded) return;

        if (cameraTransform == null)
        {
            Debug.LogWarning("Не назначена камера в PlayerMovement!");
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical + right * horizontal).normalized;

        // Двигаем Rigidbody
        Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        // Поворачиваем только если есть движение
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        WASDchanged?.Invoke();
    }

    private void HandleJumping()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // сбрасываем Y, чтобы прыжок был стабильным
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            Jump?.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (capsule == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float rayLength = capsule.bounds.extents.y + 0.2f;
        Gizmos.DrawLine(origin, origin + Vector3.down * rayLength);
    }
}
