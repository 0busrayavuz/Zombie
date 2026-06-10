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
        EnsureZombieAtmosphere();
        EnsureArenaPerimeter();
        EnsureRockObstacles();
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

    void EnsureZombieAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.42f, 0.46f, 0.47f);
        RenderSettings.fogStartDistance = 18f;
        RenderSettings.fogEndDistance = 85f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.48f, 0.53f, 0.57f);
        RenderSettings.ambientEquatorColor = new Color(0.34f, 0.38f, 0.38f);
        RenderSettings.ambientGroundColor = new Color(0.16f, 0.17f, 0.16f);
        RenderSettings.reflectionIntensity = 0.72f;

        foreach (Light sceneLight in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (sceneLight.type != LightType.Directional)
            {
                continue;
            }

            sceneLight.color = new Color(0.78f, 0.82f, 0.84f);
            sceneLight.intensity = 1.05f;
            sceneLight.shadows = LightShadows.Soft;
            sceneLight.shadowStrength = 0.68f;
            sceneLight.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
        }

        Camera sceneCamera = Camera.main;
        if (sceneCamera != null)
        {
            sceneCamera.clearFlags = CameraClearFlags.SolidColor;
            sceneCamera.backgroundColor = new Color(0.43f, 0.49f, 0.53f);
        }
    }

    void EnsureRockObstacles()
    {
        if (GameObject.Find("Rock Obstacles") != null || GameObject.Find("Ground") == null)
        {
            return;
        }

        GameObject rocks = new GameObject("Rock Obstacles");
        Material rockMaterial = CreateRuntimeMaterial(
            "Weathered Rock",
            new Color(0.22f, 0.235f, 0.23f)
        );
        if (rockMaterial.HasProperty("_Smoothness"))
        {
            rockMaterial.SetFloat("_Smoothness", 0.04f);
        }

        CreateRock(rocks.transform, new Vector3(-13f, 0.55f, 12f), new Vector3(2.4f, 1.1f, 1.8f), 18f, rockMaterial);
        CreateRock(rocks.transform, new Vector3(9f, 0.7f, 10f), new Vector3(1.8f, 1.4f, 2.2f), -24f, rockMaterial);
        CreateRock(rocks.transform, new Vector3(-11f, 0.45f, -9f), new Vector3(1.6f, 0.9f, 2.6f), 42f, rockMaterial);
        CreateRock(rocks.transform, new Vector3(12f, 0.6f, -11f), new Vector3(2.8f, 1.2f, 1.7f), -12f, rockMaterial);
        CreateRock(rocks.transform, new Vector3(-3f, 0.4f, 16f), new Vector3(1.4f, 0.8f, 1.3f), 31f, rockMaterial);
        CreateRock(rocks.transform, new Vector3(17f, 0.45f, 1f), new Vector3(1.5f, 0.9f, 1.9f), 64f, rockMaterial);
    }

    void CreateRock(
        Transform parent,
        Vector3 position,
        Vector3 scale,
        float yaw,
        Material material
    )
    {
        GameObject rock = new GameObject("Rock");
        rock.transform.SetParent(parent);
        rock.transform.position = position;
        rock.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        rock.transform.localScale = scale;

        CreateRockChunk(
            rock.transform,
            "Main Boulder",
            new Vector3(0f, 0.06f, 0f),
            new Vector3(1f, 1.15f, 1f),
            Quaternion.Euler(7f, 0f, -5f),
            Mathf.RoundToInt(yaw),
            material
        );
        CreateRockChunk(
            rock.transform,
            "Broken Rock",
            new Vector3(0.36f, -0.18f, 0.28f),
            new Vector3(0.58f, 0.62f, 0.64f),
            Quaternion.Euler(-12f, 34f, 16f),
            Mathf.RoundToInt(yaw) + 41,
            material
        );
        CreateRockChunk(
            rock.transform,
            "Rock Shard",
            new Vector3(-0.38f, -0.24f, -0.2f),
            new Vector3(0.48f, 0.46f, 0.56f),
            Quaternion.Euler(18f, -28f, -11f),
            Mathf.RoundToInt(yaw) - 27,
            material
        );
    }

    void CreateRockChunk(
        Transform parent,
        string chunkName,
        Vector3 localPosition,
        Vector3 localScale,
        Quaternion localRotation,
        int variant,
        Material material
    )
    {
        GameObject chunk = new GameObject(chunkName);
        chunk.transform.SetParent(parent);
        chunk.transform.localPosition = localPosition;
        chunk.transform.localRotation = localRotation;
        chunk.transform.localScale = localScale;

        Mesh rockMesh = CreateRockMesh(variant);
        MeshFilter filter = chunk.AddComponent<MeshFilter>();
        filter.sharedMesh = rockMesh;

        MeshRenderer renderer = chunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        MeshCollider collider = chunk.AddComponent<MeshCollider>();
        collider.sharedMesh = rockMesh;
        collider.convex = true;
    }

    Mesh CreateRockMesh(int variant)
    {
        const int sides = 8;
        Vector3[] bottom = new Vector3[sides];
        Vector3[] middle = new Vector3[sides];
        Vector3[] top = new Vector3[sides];

        for (int i = 0; i < sides; i++)
        {
            float angle = i * Mathf.PI * 2f / sides;
            bottom[i] = CreateRockRingPoint(angle, -0.55f, 0.76f, variant, i, 0);
            middle[i] = CreateRockRingPoint(angle, 0.02f, 1f, variant, i, 1);
            top[i] = CreateRockRingPoint(angle, 0.62f, 0.48f, variant, i, 2);
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Vector3 bottomCenter = new Vector3(0.04f, -0.58f, -0.03f);
        Vector3 topCenter = new Vector3(-0.12f, 0.68f, 0.08f);

        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            AddRockTriangle(vertices, triangles, bottomCenter, bottom[next], bottom[i]);
            AddRockQuad(vertices, triangles, bottom[i], bottom[next], middle[next], middle[i]);
            AddRockQuad(vertices, triangles, middle[i], middle[next], top[next], top[i]);
            AddRockTriangle(vertices, triangles, topCenter, top[i], top[next]);
        }

        Mesh mesh = new Mesh();
        mesh.name = "Runtime Fractured Rock";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    Vector3 CreateRockRingPoint(
        float angle,
        float height,
        float radius,
        int variant,
        int index,
        int ring
    )
    {
        float noise = Mathf.Sin((variant + 17) * 0.31f + index * 2.13f + ring * 1.71f);
        float secondNoise = Mathf.Cos((variant - 9) * 0.23f + index * 1.37f + ring * 2.29f);
        float unevenRadius = radius * (1f + noise * 0.18f);

        return new Vector3(
            Mathf.Cos(angle) * unevenRadius,
            height + secondNoise * 0.07f,
            Mathf.Sin(angle) * radius * (0.82f + secondNoise * 0.16f)
        );
    }

    void AddRockQuad(
        List<Vector3> vertices,
        List<int> triangles,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d
    )
    {
        AddRockTriangle(vertices, triangles, a, b, c);
        AddRockTriangle(vertices, triangles, a, c, d);
    }

    void AddRockTriangle(
        List<Vector3> vertices,
        List<int> triangles,
        Vector3 a,
        Vector3 b,
        Vector3 c
    )
    {
        int start = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        triangles.Add(start);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
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

    void EnsureArenaPerimeter()
    {
        if (GameObject.Find("Arena Perimeter") != null || GameObject.Find("Ground") == null)
        {
            return;
        }

        GameObject perimeter = new GameObject("Arena Perimeter");
        Material frameMaterial = CreateRuntimeMaterial(
            "Fence Frame",
            new Color(0.12f, 0.14f, 0.13f)
        );
        Material wireMaterial = CreateRuntimeMaterial(
            "Fence Wire",
            new Color(0.42f, 0.46f, 0.43f)
        );

        CreateFenceSide(
            perimeter.transform,
            "North Fence",
            new Vector3(-23.7f, 0f, 23.7f),
            Vector3.right,
            10,
            frameMaterial,
            wireMaterial
        );
        CreateFenceSide(
            perimeter.transform,
            "South Fence",
            new Vector3(23.7f, 0f, -23.7f),
            Vector3.left,
            10,
            frameMaterial,
            wireMaterial
        );
        CreateFenceSide(
            perimeter.transform,
            "East Fence",
            new Vector3(23.7f, 0f, 23.7f),
            Vector3.back,
            10,
            frameMaterial,
            wireMaterial
        );
        CreateFenceSide(
            perimeter.transform,
            "West Fence",
            new Vector3(-23.7f, 0f, -23.7f),
            Vector3.forward,
            10,
            frameMaterial,
            wireMaterial
        );
    }

    void CreateFenceSide(
        Transform parent,
        string sideName,
        Vector3 start,
        Vector3 direction,
        int panelCount,
        Material frameMaterial,
        Material wireMaterial
    )
    {
        const float panelLength = 4.74f;
        const float fenceHeight = 2.8f;
        GameObject side = new GameObject(sideName);
        side.transform.SetParent(parent);

        for (int i = 0; i <= panelCount; i++)
        {
            Vector3 postPosition = start + direction * panelLength * i;
            CreateFencePiece(
                side.transform,
                "Fence Post",
                postPosition + Vector3.up * (fenceHeight * 0.5f),
                new Vector3(0.13f, fenceHeight, 0.13f),
                Quaternion.identity,
                frameMaterial
            );
        }

        for (int i = 0; i < panelCount; i++)
        {
            Vector3 panelCenter = start + direction * panelLength * (i + 0.5f);
            bool alongX = Mathf.Abs(direction.x) > 0.5f;
            Vector3 railScale = alongX
                ? new Vector3(panelLength, 0.08f, 0.08f)
                : new Vector3(0.08f, 0.08f, panelLength);

            CreateFencePiece(
                side.transform,
                "Lower Rail",
                panelCenter + Vector3.up * 0.55f,
                railScale,
                Quaternion.identity,
                frameMaterial
            );
            CreateFencePiece(
                side.transform,
                "Upper Rail",
                panelCenter + Vector3.up * 2.45f,
                railScale,
                Quaternion.identity,
                frameMaterial
            );

            float wireLength = Mathf.Sqrt(panelLength * panelLength + 1.75f * 1.75f);
            float wireAngle = Mathf.Atan2(1.75f, panelLength) * Mathf.Rad2Deg;
            Vector3 wireScale = alongX
                ? new Vector3(wireLength, 0.035f, 0.035f)
                : new Vector3(0.035f, 0.035f, wireLength);
            Quaternion forwardDiagonal = alongX
                ? Quaternion.Euler(0f, 0f, wireAngle)
                : Quaternion.Euler(-wireAngle, 0f, 0f);
            Quaternion backwardDiagonal = alongX
                ? Quaternion.Euler(0f, 0f, -wireAngle)
                : Quaternion.Euler(wireAngle, 0f, 0f);

            CreateFencePiece(
                side.transform,
                "Diagonal Wire",
                panelCenter + Vector3.up * 1.5f,
                wireScale,
                forwardDiagonal,
                wireMaterial
            );
            CreateFencePiece(
                side.transform,
                "Diagonal Wire",
                panelCenter + Vector3.up * 1.5f,
                wireScale,
                backwardDiagonal,
                wireMaterial
            );
        }
    }

    void CreateFencePiece(
        Transform parent,
        string pieceName,
        Vector3 position,
        Vector3 scale,
        Quaternion rotation,
        Material material
    )
    {
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = pieceName;
        piece.transform.SetParent(parent);
        piece.transform.position = position;
        piece.transform.rotation = rotation;
        piece.transform.localScale = scale;

        Collider collider = piece.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = piece.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
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
