using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionController : MonoBehaviour
{
    [Header("VFX Prefab")]
    [SerializeField] private VisualEffect explosionVFXPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;

    void Start()
    {
        StartCoroutine(VfxSpawner());
    }

    IEnumerator VfxSpawner()
    {
        while (true)
        {
            SpawnExplosion();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnExplosion()
    {
        if (explosionVFXPrefab == null) return;

        // Создаем взрыв в позиции этого объекта
        VisualEffect explosion = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        // Запускаем и уничтожаем через 2 секунды
        explosion.Play();
        Destroy(explosion.gameObject, 2f);
    }
}