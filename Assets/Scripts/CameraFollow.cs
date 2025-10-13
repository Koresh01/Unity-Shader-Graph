using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Цель для слежения (игрок):")]
    [SerializeField] private Transform target;

    [Header("Параметры вращения:")]
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Параметры приближения:")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;

    private float yaw = 0f;       // Горизонтальный угол (вокруг игрока)
    private float pitch = 20f;    // Вертикальный угол (вверх/вниз)
    private float distance = 5f;  // Текущее расстояние до игрока

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: не назначен объект 'target'!");
            enabled = false;
            return;
        }

        // Прячем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    private void HandleInput()
    {
        // Получаем движение мыши
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Прокрутка колеса мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Вращение вокруг игрока
        yaw += mouseX * rotationSpeed * Time.deltaTime;
        pitch -= mouseY * rotationSpeed * 0.5f * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Приближение/отдаление
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void UpdateCameraPosition()
    {
        // Вычисляем позицию камеры на сфере вокруг игрока
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        // Устанавливаем позицию и направление
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }
}
