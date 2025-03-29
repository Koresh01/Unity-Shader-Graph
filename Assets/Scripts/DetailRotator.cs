using UnityEngine;

[AddComponentMenu("Custom/(Вращает детальку)")]
public class DetailRotator : MonoBehaviour
{
    [Tooltip("Скорость вращения.")]
    [SerializeField] float speed = 1.0f;

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * speed, Space.World);
    }
}
