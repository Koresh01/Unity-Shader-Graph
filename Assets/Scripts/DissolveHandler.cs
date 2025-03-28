using System.Collections;
using UnityEngine;

public class DissolveEffect : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;  // Рендер объекта
    [SerializeField] private float dissolveDuration = 1.5f;  // Время эффекта
    [SerializeField] private float interval = 3f;  // Интервал между появлением и исчезновением

    private Material material;
    private bool isVisible = false;  // Статус объекта

    private void Start()
    {
        material = objectRenderer.material;
        StartCoroutine(ToggleDissolveEffect());
    }

    private IEnumerator ToggleDissolveEffect()
    {
        while (true)
        {
            if (isVisible)
                yield return StartCoroutine(HideObject());
            else
                yield return StartCoroutine(ShowObject());

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator ShowObject()
    {
        float time = 0;
        while (time < dissolveDuration)
        {
            time += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(1, 0, time / dissolveDuration);
            material.SetFloat("_dissolve", dissolveValue);
            yield return null;
        }
        isVisible = true;
    }

    private IEnumerator HideObject()
    {
        float time = 0;
        while (time < dissolveDuration)
        {
            time += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(0, 1, time / dissolveDuration);
            material.SetFloat("_dissolve", dissolveValue);
            yield return null;
        }
        isVisible = false;
    }
}
