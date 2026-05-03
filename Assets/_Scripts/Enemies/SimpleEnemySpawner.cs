/*
* SimpleEnemySpawner
* Ќазначение: максимально упрощЄнный спавнер дл€ ранних шагов обучени€ (Instantiate + лимит активных врагов).
* „то делает: спавнит один выбранный тип врага в случайных точках и управл€ет простым циклом auto-spawn.
* —в€зи: использует EnemyData/EnemyBase; может работать как fallback дл€ EncounterTrigger в учебном режиме.
* ѕаттерны: Composition, Fail Fast, Local Validation.
*
*  онтракт дл€ уроков:
*  - Ёто облегчЄнный вариант, чтобы ученики быстрее освоили основы спавна.
*  - ќсновной канон дл€ encounter/wave сло€ в teacher repo Ч EnemySpawner.
*  - EncounterTrigger может использовать этот компонент как fallback, чтобы ученик не получал "молчаливую" поломку.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ”прощЄнный спавнер врагов дл€ базовых уроков без pooling/factory.
/// </summary>
public class SimpleEnemySpawner : MonoBehaviour
{
    [Header("“ип врага")]
    [Tooltip("ƒанные врага, которого будем спавнить в упрощЄнном режиме.")]
    [SerializeField] private EnemyData enemyData;

    [Header("“очки спавна")]
    [Tooltip("ћассив точек, где могут по€вл€тьс€ враги.")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Ќастройки спавна")]
    [Min(0.1f)]
    [Tooltip("»нтервал между спавнами (в секундах).")]
    [SerializeField] private float spawnInterval = 5f;

    [Min(0)]
    [Tooltip("ћаксимальное количество одновременно активных врагов.")]
    [SerializeField] private int maxEnemies = 10;

    [Tooltip("«апускать ли спавн автоматически при старте.")]
    [SerializeField] private bool spawnOnStart = true;

    [Header("ќтладка")]
    [Tooltip("ѕоказывать подробные логи спавнера.")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isSpawning;
    private Coroutine spawnCoroutine;
    private Transform playerTarget;
    private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();

    /// <summary>
    /// “очки спавна (read-only) дл€ внешних систем, например EncounterTrigger fallback.
    /// </summary>
    public IReadOnlyList<Transform> SpawnPoints => spawnPoints;

    private void Start()
    {
        ResolvePlayerTarget();

        if (!ValidateSetup())
            return;

        if (spawnOnStart)
            StartSpawning();
    }

    /// <summary>
    /// «апускает периодический auto-spawn.
    /// Ёто учебный базовый цикл, не wave/encounter оркестратор.
    /// </summary>
    public void StartSpawning()
    {
        if (isSpawning)
        {
            if (showDebugLogs)
                Debug.LogWarning($"{name}: спавн уже запущен.", this);
            return;
        }

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnCoroutine());

        if (showDebugLogs)
            Debug.Log($"{name}: спавн врагов запущен.", this);
    }

    /// <summary>
    /// ќстанавливает периодический auto-spawn.
    /// </summary>
    public void StopSpawning()
    {
        if (!isSpawning)
        {
            if (showDebugLogs)
                Debug.LogWarning($"{name}: спавн не был запущен.", this);
            return;
        }

        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (showDebugLogs)
            Debug.Log($"{name}: спавн врагов остановлен.", this);
    }

    /// <summary>
    /// —павнит врага из локального enemyData в случайной точке.
    /// </summary>
    public EnemyBase SpawnEnemy()
    {
        if (!ValidateSetup())
            return null;

        if (playerTarget == null)
            ResolvePlayerTarget();

        return SpawnInternal(enemyData, GetRandomSpawnPoint(), playerTarget);
    }

    /// <summary>
    /// Fallback-метод дл€ encounter-системы.
    /// ѕозвол€ет EncounterTrigger заспавнить конкретный EnemyData, если в сцене нет EnemySpawner.
    /// </summary>
    public EnemyBase SpawnEnemyForEncounter(EnemyData overrideData, Transform spawnPointOverride, Transform targetOverride)
    {
        EnemyData dataToSpawn = overrideData != null ? overrideData : enemyData;
        if (dataToSpawn == null || dataToSpawn.prefab == null)
        {
            Debug.LogError($"{name}: encounter fallback не может заспавнить врага Ч невалидный EnemyData.", this);
            return null;
        }

        if (showDebugLogs)
        {
            Debug.LogWarning(
                $"{name}: encounter использует SimpleEnemySpawner как fallback. " +
                "ƒл€ каноничного сценари€ урока 7.4 рекомендуетс€ EnemySpawner.", this);
        }

        Transform spawnPoint = spawnPointOverride != null ? spawnPointOverride : GetRandomSpawnPoint();
        Transform target = targetOverride != null ? targetOverride : playerTarget;

        if (target == null)
        {
            ResolvePlayerTarget();
            target = playerTarget;
        }

        return SpawnInternal(dataToSpawn, spawnPoint, target);
    }

    private EnemyBase SpawnInternal(EnemyData data, Transform spawnPoint, Transform target)
    {
        if (data == null || data.prefab == null)
            return null;

        CleanupInactiveEnemies();
        if (activeEnemies.Count >= maxEnemies)
        {
            if (showDebugLogs)
                Debug.Log($"{name}: достигнут лимит врагов. ѕропускаем спавн.", this);
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning($"{name}: не найдена валидна€ точка спавна.", this);
            return null;
        }

        GameObject enemyObject = Instantiate(data.prefab, spawnPoint.position, spawnPoint.rotation);
        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogError($"{name}: на префабе {data.prefab.name} отсутствует EnemyBase.", this);
            Destroy(enemyObject);
            return null;
        }

        enemy.Setup(data);
        if (target != null)
            enemy.SetTarget(target);

        activeEnemies.Add(enemy);

        if (showDebugLogs)
            Debug.Log($"{name}: создан враг {data.enemyName} в точке {spawnPoint.name}.", this);

        return enemy;
    }

    private IEnumerator SpawnCoroutine()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    private bool ValidateSetup()
    {
        if (enemyData == null || enemyData.prefab == null)
        {
            Debug.LogWarning($"{name}: не назначены EnemyData или prefab.", this);
            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: нет точек спавна.", this);
            return false;
        }

        return true;
    }

    private void ResolvePlayerTarget()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        playerTarget = player != null ? player.transform : null;
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        List<Transform> validPoints = new List<Transform>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
                validPoints.Add(spawnPoints[i]);
        }

        if (validPoints.Count == 0)
            return null;

        return validPoints[Random.Range(0, validPoints.Count)];
    }

    private void CleanupInactiveEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
    }

    private void OnDestroy()
    {
        StopSpawning();
    }
}