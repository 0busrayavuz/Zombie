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

        DrawTracer(ray.origin, tracerEnd);
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
