using UnityEngine;

public class DissolveHandler : MonoBehaviour
{
    [SerializeField] float speed = 1.0f;
    [SerializeField] float dissolve = 0.0f;

    [SerializeField] Material _material; // ������ �������� ��� �������

    private void Start()
    {
        Shader shader = _material.shader;
        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            Debug.Log($"Property {i}: {shader.GetPropertyName(i)} ({shader.GetPropertyType(i)})");
        }
    }

    private void Update()
    {
        // ��������� �� 0 �� 1
        dissolve = Mathf.Abs(Mathf.Sin(Time.time * speed));

        // ������� �������� � ������
        if (_material.HasProperty("_dissolve"))
        {
            _material.SetFloat("_dissolve", dissolve);
        }
        else Debug.LogError("�������� �����������");
        
    }
}
