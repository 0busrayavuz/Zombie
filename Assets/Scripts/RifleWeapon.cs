using UnityEngine;

public class RifleWeapon : MonoBehaviour
{
    [SerializeField] Camera aimCamera;
    [SerializeField] int damageDealt = 25;
    [SerializeField] float range = 100f;
    [SerializeField] float fireDelay = 0.15f;
    [SerializeField] float tracerDuration = 0.06f;
    [SerializeField] LayerMask hitLayers = ~0;

    Health ownerHealth;
    AudioSource audioSource;
    AudioClip shotClip;
    AudioClip[] ricochetClips;
    Renderer weaponRenderer;
    float nextFireTime;
    float hitMarkerUntil;

    void Awake()
    {
        ownerHealth = GetComponent<Health>();
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 0f;
        shotClip = Resources.Load<AudioClip>("Audio/RifleShot");
        ricochetClips = Resources.LoadAll<AudioClip>("Audio/Ricochets");
        FindWeaponRenderer();
    }

    void Start()
    {
        FindWeaponRenderer();
    }

    void Update()
    {
        if (MainMenuGui.IsMenuOpen || aimCamera == null || GameManager.HasPlayerWon ||
            (ownerHealth != null && ownerHealth.IsDead))
        {
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireDelay;
        }
    }

    void Fire()
    {
        if (shotClip != null)
        {
            audioSource.PlayOneShot(shotClip, 0.65f);
        }

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 muzzlePosition = GetMuzzlePosition(ray.direction);
        CreateMuzzleFire(muzzlePosition, ray.direction);
        int layerMask = hitLayers.value == 0 ? Physics.DefaultRaycastLayers : hitLayers.value;
        Vector3 tracerEnd = ray.origin + ray.direction * range;

        RaycastHit[] hits = Physics.RaycastAll(ray, range, layerMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Health targetHealth = hit.transform.GetComponentInParent<Health>();
            if (targetHealth == ownerHealth)
            {
                continue;
            }

            if (targetHealth != null)
            {
                targetHealth.Damage(damageDealt);
                CombatEffects.CreateBlood(hit.point, hit.normal);
                hitMarkerUntil = Time.time + 0.12f;
            }
            else
            {
                PlayRicochet(hit.point);
            }

            tracerEnd = hit.point;
            break;
        }

        DrawTracer(muzzlePosition, tracerEnd);
    }

    void CreateMuzzleFire(Vector3 position, Vector3 direction)
    {
        GameObject effect = new GameObject("Muzzle Fire");
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.LookRotation(direction);

        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.04f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.025f, 0.055f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 1.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.085f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.78f, 0.18f, 0.95f),
            new Color(1f, 0.18f, 0.01f, 0.8f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 8;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 6)
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 7f;
        shape.radius = 0.01f;
        shape.length = 0.06f;

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.82f, 0.24f), 0f),
                new GradientColorKey(new Color(1f, 0.16f, 0.01f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.95f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = gradient;

        ParticleSystemRenderer particleRenderer = effect.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        particleRenderer.lengthScale = 1.8f;
        particleRenderer.velocityScale = 0.12f;
        particleRenderer.material = CreateMuzzleParticleMaterial();

        Light fireLight = effect.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.42f, 0.06f);
        fireLight.intensity = 0.65f;
        fireLight.range = 1.5f;
        fireLight.shadows = LightShadows.None;

        particles.Play();
        Destroy(effect, 0.09f);
    }

    Vector3 GetMuzzlePosition(Vector3 direction)
    {
        if (weaponRenderer == null)
        {
            FindWeaponRenderer();
        }

        if (weaponRenderer != null)
        {
            Bounds bounds = weaponRenderer.bounds;
            Vector3 normalizedDirection = direction.normalized;
            float extent =
                Mathf.Abs(normalizedDirection.x) * bounds.extents.x +
                Mathf.Abs(normalizedDirection.y) * bounds.extents.y +
                Mathf.Abs(normalizedDirection.z) * bounds.extents.z;
            return bounds.center + normalizedDirection * (extent + 0.035f);
        }

        return transform.position +
            Vector3.up * 1.05f +
            aimCamera.transform.right * 0.25f +
            direction * 0.45f;
    }

    void FindWeaponRenderer()
    {
        foreach (Renderer candidate in GetComponentsInChildren<Renderer>(true))
        {
            string candidateName = candidate.name.ToLowerInvariant();
            string materialName = candidate.sharedMaterial == null
                ? string.Empty
                : candidate.sharedMaterial.name.ToLowerInvariant();

            if (candidateName.Contains("weapon") ||
                candidateName.Contains("rifle") ||
                candidateName.Contains("gun") ||
                materialName.Contains("weapon"))
            {
                weaponRenderer = candidate;
                return;
            }
        }
    }

    Material CreateMuzzleParticleMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        Material material = new Material(shader);
        material.name = "Muzzle Fire Material";
        Color color = new Color(1f, 0.34f, 0.025f, 0.85f);
        material.color = color;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        return material;
    }

    void PlayRicochet(Vector3 position)
    {
        if (ricochetClips == null || ricochetClips.Length == 0)
        {
            return;
        }

        AudioClip clip = ricochetClips[Random.Range(0, ricochetClips.Length)];
        AudioSource.PlayClipAtPoint(clip, position, 0.42f);
    }

    void DrawTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = new GameObject("Shot Tracer");
        LineRenderer line = tracer.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.025f;
        line.endWidth = 0.01f;
        line.startColor = Color.red;
        line.endColor = new Color(1f, 0.6f, 0.1f, 0.2f);
        line.material = CreateTracerMaterial();
        Destroy(tracer, tracerDuration);
    }

    void OnGUI()
    {
        if (MainMenuGui.IsMenuOpen || Time.time > hitMarkerUntil)
        {
            return;
        }

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        Color previous = GUI.color;
        GUI.color = Color.white;

        DrawMarkerLine(centerX - 12f, centerY - 12f, 7f, 2f);
        DrawMarkerLine(centerX + 5f, centerY - 12f, 7f, 2f);
        DrawMarkerLine(centerX - 12f, centerY + 10f, 7f, 2f);
        DrawMarkerLine(centerX + 5f, centerY + 10f, 7f, 2f);

        GUI.color = previous;
    }

    static void DrawMarkerLine(float x, float y, float width, float height)
    {
        GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
    }

    static Material CreateTracerMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        return new Material(shader);
    }
}
