using System.Collections.Generic;
using UnityEngine;

public class ZombieSurvivalBootstrapper : MonoBehaviour
{
    [SerializeField] bool buildOnAwake = true;
    [SerializeField] bool onlyWhenPlayerIsMissing = true;
    [SerializeField] float medikitPickupDistance = 1.6f;

    readonly List<GameObject> activeMedikits = new List<GameObject>();
    Health playerHealth;
    Transform playerTransform;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreateForEmptyScenes()
    {
        if (FindFirstObjectByType<ZombieSurvivalBootstrapper>() != null)
        {
            return;
        }

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            return;
        }

        GameObject bootstrapper = new GameObject("ZombieSurvivalBootstrapper");
        bootstrapper.AddComponent<ZombieSurvivalBootstrapper>();
    }

    void Awake()
    {
        RemoveLegacyStartingMedikits();

        if (buildOnAwake)
        {
            BuildScene();
        }

        EnsureExtendedFeatures();
        RefreshMedikits();
    }

    static void RemoveLegacyStartingMedikits()
    {
        foreach (GameObject candidate in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (candidate.name == "Medikit" && candidate.transform.parent == null)
            {
                Destroy(candidate);
            }
        }
    }

    void Update()
    {
        UpdateMedikitPickups();
    }

    public void BuildScene()
    {
        if (onlyWhenPlayerIsMissing && GameObject.FindGameObjectWithTag("Player") != null)
        {
            return;
        }

        CreateGround();
        CreateLightIfMissing();
        GameObject player = CreatePlayer();
        Helicopter helicopter = CreateHelicopter();
        CreateManagers(helicopter);
        CreateBoundaries();

        player.transform.position = new Vector3(0f, 1f, 0f);
        RefreshMedikits();
    }

    void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(50f, 1f, 50f);

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material groundMaterial = CreateRuntimeMaterial(
                "Runtime Ground Material",
                new Color(0.22f, 0.24f, 0.25f)
            );
            Texture2D diffuse = Resources.Load<Texture2D>("Textures/Dark concrete diffuse");
            Texture2D normal = Resources.Load<Texture2D>("Textures/Dark concrete normal");

            if (diffuse != null)
            {
                groundMaterial.SetTexture("_BaseMap", diffuse);
                groundMaterial.SetTextureScale("_BaseMap", new Vector2(10f, 10f));
            }

            if (normal != null)
            {
                groundMaterial.SetTexture("_BumpMap", normal);
                groundMaterial.SetTextureScale("_BumpMap", new Vector2(10f, 10f));
                groundMaterial.EnableKeyword("_NORMALMAP");
            }

            renderer.material = groundMaterial;
        }
    }

    void CreateLightIfMissing()
    {
        if (FindFirstObjectByType<Light>() != null)
        {
            return;
        }

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
    }

    GameObject CreatePlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        TrySetTag(player, "Player");
        RemoveCollider(player);

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = Vector3.zero;

        player.AddComponent<Health>();
        player.AddComponent<PlayerStats>();
        player.AddComponent<PlayerMovement>();
        player.AddComponent<PlayerLook>();
        player.AddComponent<RifleWeapon>();
        player.AddComponent<MissileWeapon>();
        player.AddComponent<WeaponSwitcher>();
        player.AddComponent<PlayerAnimation>();
        player.AddComponent<PlayerGui>();

        Renderer playerRenderer = player.GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.enabled = false;
        }

        RuntimeVisualUtility.AttachModel(
            player.transform,
            "Models/Player/Player",
            1.9f,
            "Models/Player/Player",
            "Models/Player/Player_NRM",
            "Models/Player/Weapon",
            "Models/Player/Weapon_NRM",
            -0.95f
        );

        Camera camera = Camera.main;
        GameObject cameraObject;
        if (camera == null)
        {
            cameraObject = new GameObject("Main Camera");
            TrySetTag(cameraObject, "MainCamera");
            camera = cameraObject.AddComponent<Camera>();
        }
        else
        {
            cameraObject = camera.gameObject;
        }

        if (cameraObject.GetComponent<AudioListener>() == null)
        {
            cameraObject.AddComponent<AudioListener>();
        }

        camera.transform.SetParent(player.transform);
        camera.transform.localPosition = new Vector3(0f, 1.35f, -6f);
        camera.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);

        return player;
    }

    Helicopter CreateHelicopter()
    {
        GameObject helicopterObject = new GameObject("Rescue Helicopter");
        helicopterObject.transform.position = new Vector3(16f, 18f, 16f);

        RuntimeVisualUtility.AttachModel(
            helicopterObject.transform,
            "Models/Helicopter/Helicopter",
            4.5f,
            "Models/Helicopter/Body",
            null,
            "Models/Helicopter/blade"
        );

        GameObject landingTarget = new GameObject("Helicopter Landing Target");
        landingTarget.transform.position = new Vector3(16f, 0f, 16f);

        Helicopter helicopter = helicopterObject.AddComponent<Helicopter>();
        helicopter.Configure(landingTarget.transform);

        GameObject rescueTrigger = new GameObject("Helicopter Rescue Trigger");
        rescueTrigger.transform.SetParent(helicopterObject.transform);
        rescueTrigger.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        BoxCollider trigger = rescueTrigger.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(7f, 4f, 7f);
        rescueTrigger.AddComponent<HelicopterDoors>();

        return helicopter;
    }

    void CreateManagers(Helicopter helicopter)
    {
        GameObject manager = new GameObject("GameManager");
        GameManager gameManager = manager.AddComponent<GameManager>();
        gameManager.Configure(15f, helicopter);
        manager.AddComponent<CombatGui>();
        manager.AddComponent<MainMenuGui>();

        AudioClip music = Resources.Load<AudioClip>("Audio/BackgroundMusic");
        if (music != null)
        {
            AudioSource musicSource = manager.AddComponent<AudioSource>();
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.volume = 0.22f;
            if (Application.isPlaying)
            {
                musicSource.Play();
            }
        }

        GameObject spawner = new GameObject("EnemySpawnManager");
        spawner.AddComponent<EnemySpawnManager>();
    }

    void CreateBoundaries()
    {
        CreateBoundary("North Boundary", new Vector3(0f, 2f, 24.5f), new Vector3(50f, 4f, 1f));
        CreateBoundary("South Boundary", new Vector3(0f, 2f, -24.5f), new Vector3(50f, 4f, 1f));
        CreateBoundary("East Boundary", new Vector3(24.5f, 2f, 0f), new Vector3(1f, 4f, 50f));
        CreateBoundary("West Boundary", new Vector3(-24.5f, 2f, 0f), new Vector3(1f, 4f, 50f));
    }

    void CreateBoundary(string objectName, Vector3 position, Vector3 scale)
    {
        GameObject boundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boundary.name = objectName;
        boundary.transform.position = position;
        boundary.transform.localScale = scale;
        Renderer renderer = boundary.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    void CreateMedikits()
    {
        Vector3[] positions =
        {
            new Vector3(8f, 0.65f, 7f),
            new Vector3(-9f, 0.65f, 8f),
            new Vector3(10f, 0.65f, -9f),
            new Vector3(-11f, 0.65f, -7f)
        };

        foreach (Vector3 position in positions)
        {
            CreateMedikit(position);
        }
    }

    void CreateMedikit(Vector3 position)
    {
        GameObject medikit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        medikit.name = "Medikit";
        medikit.transform.position = position;
        medikit.transform.localScale = new Vector3(0.7f, 0.45f, 0.9f);

        BoxCollider trigger = medikit.GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        medikit.AddComponent<MedikitPickup>();

        Renderer renderer = medikit.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = CreateRuntimeMaterial("Medikit Green", new Color(0.08f, 0.68f, 0.22f));
        }

        CreateMedikitBar(medikit.transform, new Vector3(0f, 0.28f, 0f), new Vector3(0.42f, 0.08f, 0.12f));
        CreateMedikitBar(medikit.transform, new Vector3(0f, 0.28f, 0f), new Vector3(0.12f, 0.08f, 0.42f));
    }

    void CreateMedikitBar(Transform parent, Vector3 localPosition, Vector3 localScale)
    {
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "White Cross";
        bar.transform.SetParent(parent);
        bar.transform.localPosition = localPosition;
        bar.transform.localRotation = Quaternion.identity;
        bar.transform.localScale = localScale;

        Collider collider = bar.GetComponent<Collider>();
        if (collider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }

        Renderer renderer = bar.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = CreateRuntimeMaterial("Medikit White", Color.white);
        }
    }

    void RefreshMedikits()
    {
        activeMedikits.Clear();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<Health>();
        }

        foreach (GameObject candidate in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (candidate.name != "Medikit")
            {
                continue;
            }

            activeMedikits.Add(candidate);
            SetMedikitGreen(candidate);

            MedikitPickup oldPickup = candidate.GetComponent<MedikitPickup>();
            if (oldPickup != null)
            {
                oldPickup.enabled = false;
            }
        }
    }

    void EnsureExtendedFeatures()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.GetComponent<MissileWeapon>() == null)
            {
                player.AddComponent<MissileWeapon>();
            }

            if (player.GetComponent<WeaponSwitcher>() == null)
            {
                player.AddComponent<WeaponSwitcher>();
            }
        }

        if (FindFirstObjectByType<MainMenuGui>() == null)
        {
            gameObject.AddComponent<MainMenuGui>();
        }
    }

    void UpdateMedikitPickups()
    {
        if (playerTransform == null || playerHealth == null)
        {
            RefreshMedikits();
            return;
        }

        for (int i = activeMedikits.Count - 1; i >= 0; i--)
        {
            GameObject medikit = activeMedikits[i];
            if (medikit == null)
            {
                activeMedikits.RemoveAt(i);
                continue;
            }

            Vector3 difference = playerTransform.position - medikit.transform.position;
            difference.y = 0f;
            if (difference.sqrMagnitude > medikitPickupDistance * medikitPickupDistance)
            {
                continue;
            }

            if (playerHealth.IsDead || playerHealth.CurrentHealth >= playerHealth.MaximumHealth)
            {
                continue;
            }

            playerHealth.Heal(35);
            AudioClip healClip = Resources.Load<AudioClip>("Audio/health1");
            if (healClip != null)
            {
                AudioSource.PlayClipAtPoint(healClip, medikit.transform.position, 0.8f);
            }

            activeMedikits.RemoveAt(i);
            Destroy(medikit);
        }
    }

    static void SetMedikitGreen(GameObject medikit)
    {
        Renderer renderer = medikit.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material material = renderer.material;
        material.name = "Medikit Green";
        material.color = new Color(0.08f, 0.68f, 0.22f);
    }

    static void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(collider);
        }
        else
        {
            DestroyImmediate(collider);
        }
    }

    static void TrySetTag(GameObject target, string tagName)
    {
        try
        {
            target.tag = tagName;
        }
        catch (UnityException)
        {
            // The project can still run if a custom tag is missing.
        }
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
