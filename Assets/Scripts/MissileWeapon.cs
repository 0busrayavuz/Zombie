using UnityEngine;

public class MissileWeapon : MonoBehaviour
{
    [SerializeField] Camera aimCamera;
    [SerializeField] float fireDelay = 0.9f;
    [SerializeField] float range = 120f;

    Health ownerHealth;
    AudioSource audioSource;
    AudioClip fireClip;
    float nextFireTime;

    void Awake()
    {
        ownerHealth = GetComponent<Health>();
        aimCamera = aimCamera != null ? aimCamera : Camera.main;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        fireClip = Resources.Load<AudioClip>("Audio/RifleShot");
    }

    void Update()
    {
        if (MainMenuGui.IsMenuOpen || aimCamera == null || GameManager.HasPlayerWon ||
            (ownerHealth != null && ownerHealth.IsDead))
        {
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireDelay;
        }
    }

    void Fire()
    {
        Ray aimRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 target = aimRay.origin + aimRay.direction * range;
        if (Physics.Raycast(aimRay, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            target = hit.point;
        }

        Vector3 muzzle = transform.position + Vector3.up * 1.15f +
            transform.forward * 0.9f + transform.right * 0.28f;
        Vector3 direction = (target - muzzle).normalized;

        GameObject missileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        missileObject.name = "Missile";
        missileObject.transform.position = muzzle;
        missileObject.transform.localScale = Vector3.one * 0.24f;

        SphereCollider collider = missileObject.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        Rigidbody body = missileObject.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Renderer renderer = missileObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = new Color(0.18f, 0.2f, 0.22f);
            renderer.material = material;
        }

        GameObject visual = RuntimeVisualUtility.AttachModel(
            missileObject.transform,
            "Models/Missile/MinimalMissile1",
            0.45f,
            null
        );
        if (visual != null && renderer != null)
        {
            renderer.enabled = false;
        }

        Missile missile = missileObject.AddComponent<Missile>();
        missile.Launch(transform, direction);

        if (fireClip != null)
        {
            audioSource.pitch = 0.72f;
            audioSource.PlayOneShot(fireClip, 0.8f);
            audioSource.pitch = 1f;
        }
    }
}
