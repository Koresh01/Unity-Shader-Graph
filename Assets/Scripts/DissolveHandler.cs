using UnityEngine;

public class DissolveHandler : MonoBehaviour
{
    [SerializeField] float speed = 1.0f;
    [SerializeField] float dissolve = 0.0f;

    [SerializeField] Material _material; // Личный материал для объекта

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
        // Колебания от 0 до 1
        dissolve = Mathf.Abs(Mathf.Sin(Time.time * speed));

        // Передаём значение в шейдер
        if (_material.HasProperty("_dissolve"))
        {
            _material.SetFloat("_dissolve", dissolve);
        }
        else Debug.LogError("Свойство отсутствует");
        
    }
}
