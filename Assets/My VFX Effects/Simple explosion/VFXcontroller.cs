using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Скрипт для создания определённого vfx эффекта и его удаления по истечению времени.
/// </summary>
public class VFXcontroller : MonoBehaviour
{
    [Header("VFX Prefab")]
    [SerializeField] private VisualEffect explosionVFXPrefab;

    [Header("Spawn Settings:")]
    [SerializeField] private float spawnInterval = 10f;

    [Header("Destory Settings:")]
    [SerializeField] private float playingTime = 6f;

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

        // Запускаем и уничтожаем через "playingTime" секунд
        explosion.Play();
        Destroy(explosion.gameObject, playingTime);
    }
}