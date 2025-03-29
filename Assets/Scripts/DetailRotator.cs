using UnityEngine;

[AddComponentMenu("Custom/(������� ��������)")]
public class DetailRotator : MonoBehaviour
{
    [Tooltip("�������� ��������.")]
    [SerializeField] float speed = 1.0f;

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * speed, Space.World);
    }
}
