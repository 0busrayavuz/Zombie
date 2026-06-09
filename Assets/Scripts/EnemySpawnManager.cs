using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    static int livingZombies;

    [SerializeField] GameObject enemyToSpawn;
    [SerializeField] float spawnDelay = 2f;
    [SerializeField] float minimumSpawnDelay = 0.7f;
    [SerializeField] float difficultyRampSeconds = 75f;
    [SerializeField] int enemyLimit = 18;
    [SerializeField] LayerMask spawnLayer = ~0;
    [SerializeField] float spawnDistance = 18f;
    [SerializeField] float spawnYOffset = 1f;
    [SerializeField] Vector2 verticalViewportRange = new Vector2(0.05f, 0.95f);

    float nextSpawnTime = -1f;
    GameObject fallbackEnemyTemplate;
    float baseSpawnDelay;
    float baseMinimumSpawnDelay;
    int baseEnemyLimit;

    public static int LivingZombies { get { return livingZombies; } }

    public static void OnEnemySpawned()
    {
        livingZombies++;
    }

    public static void OnEnemyDeath()
    {
        livingZombies = Mathf.Max(0, livingZombies - 1);
    }

    void Awake()
    {
        baseSpawnDelay = spawnDelay;
        baseMinimumSpawnDelay = minimumSpawnDelay;
        baseEnemyLimit = enemyLimit;
        ApplyDifficulty(GameSettings.Difficulty);
    }

    public void ApplyDifficulty(int difficulty)
    {
        float delayMultiplier = difficulty == 0 ? 1.25f : difficulty == 2 ? 0.72f : 1f;
        float limitMultiplier = difficulty == 0 ? 0.7f : difficulty == 2 ? 1.35f : 1f;
        spawnDelay = baseSpawnDelay * delayMultiplier;
        minimumSpawnDelay = baseMinimumSpawnDelay * delayMultiplier;
        enemyLimit = Mathf.Max(6, Mathf.RoundToInt(baseEnemyLimit * limitMultiplier));
    }

    void Update()
    {
        if (GameManager.HasPlayerWon || Time.time < nextSpawnTime || livingZombies >= enemyLimit)
        {
            return;
        }

        if (TrySpawn())
        {
            float difficulty = Mathf.Clamp01(Time.timeSinceLevelLoad / difficultyRampSeconds);
            float currentDelay = Mathf.Lerp(spawnDelay, minimumSpawnDelay, difficulty);
            nextSpawnTime = Time.time + currentDelay;
        }
    }

    bool TrySpawn()
    {
        GameObject prefab = enemyToSpawn != null ? enemyToSpawn : GetFallbackEnemyTemplate();
        if (prefab == null)
        {
            return false;
        }

        Vector3 placeToSpawn;
        if (!TryFindViewportSpawnPoint(out placeToSpawn))
        {
            placeToSpawn = FindFallbackPointAroundPlayer();
        }

        Quaternion directionToFace = Quaternion.identity;
        GameObject enemy = Instantiate(prefab, placeToSpawn, directionToFace);
        enemy.name = "Zombie";
        enemy.SetActive(true);
        OnEnemySpawned();
        return true;
    }

    bool TryFindViewportSpawnPoint(out Vector3 spawnPoint)
    {
        spawnPoint = Vector3.zero;
        Camera camera = Camera.main;
        if (camera == null)
        {
            return false;
        }

        float x = Random.value > 0.5f ? 1.25f : -0.25f;
        float y = Random.Range(verticalViewportRange.x, verticalViewportRange.y);
        Ray ray = camera.ViewportPointToRay(new Vector3(x, y, spawnDistance));
        RaycastHit hit;
        int mask = spawnLayer.value == 0 ? Physics.DefaultRaycastLayers : spawnLayer.value;

        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, mask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        spawnPoint = hit.point + Vector3.up * spawnYOffset;
        return true;
    }

    Vector3 FindFallbackPointAroundPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 center = player != null ? player.transform.position : Vector3.zero;
        Vector2 circle = Random.insideUnitCircle.normalized * spawnDistance;
        return center + new Vector3(circle.x, spawnYOffset, circle.y);
    }

    GameObject GetFallbackEnemyTemplate()
    {
        if (fallbackEnemyTemplate != null)
        {
            return fallbackEnemyTemplate;
        }

        fallbackEnemyTemplate = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        fallbackEnemyTemplate.name = "Runtime Zombie Template";
        fallbackEnemyTemplate.transform.localScale = new Vector3(1f, 1f, 1f);
        fallbackEnemyTemplate.SetActive(false);

        Collider primitiveCollider = fallbackEnemyTemplate.GetComponent<Collider>();
        if (primitiveCollider != null)
        {
            Destroy(primitiveCollider);
        }

        CharacterController controller = fallbackEnemyTemplate.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.4f;
        controller.center = Vector3.zero;
        controller.stepOffset = 0.25f;

        fallbackEnemyTemplate.AddComponent<Health>();
        fallbackEnemyTemplate.AddComponent<EnemyMovement>();
        fallbackEnemyTemplate.AddComponent<EnemyAnimation>();
        fallbackEnemyTemplate.AddComponent<EnemyDrops>();

        Renderer enemyRenderer = fallbackEnemyTemplate.GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            enemyRenderer.enabled = false;
        }

        RuntimeVisualUtility.AttachModel(
            fallbackEnemyTemplate.transform,
            "Models/Zombie/ZombieMixamo",
            2f,
            "Models/Zombie/ZombieLowRes_Texture_2k",
            "Models/Zombie/Zombie_NM",
            null,
            null,
            -1f
        );

        GameObject attackRange = new GameObject("EnemyAttack");
        attackRange.transform.SetParent(fallbackEnemyTemplate.transform);
        attackRange.transform.localPosition = Vector3.zero;
        SphereCollider trigger = attackRange.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.25f;
        attackRange.AddComponent<EnemyAttack>();

        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycast >= 0)
        {
            attackRange.layer = ignoreRaycast;
        }

        return fallbackEnemyTemplate;
    }

    static Material CreateRuntimeMaterial(string materialName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = materialName;
        material.color = color;
        return material;
    }
}
