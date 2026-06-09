using System.Collections;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    [SerializeField] Transform landingTarget;
    [SerializeField] float landingDuration = 5f;
    [SerializeField] string flyingClip = "Flying";
    [SerializeField] string landingClip = "Landing";
    [SerializeField] string landedClip = "Landed";
    [SerializeField] float rotorSpeed = 1100f;
    [SerializeField] float landedRotorSpeed = 420f;

    Animation legacyAnimation;
    AudioSource rotorAudio;
    Transform mainRotor;
    Transform tailRotor;
    bool hasLanded;
    bool landingStarted;

    public bool HasLanded { get { return hasLanded; } }

    void Awake()
    {
        legacyAnimation = GetComponentInChildren<Animation>();
        mainRotor = FindChildByName(transform, "Blade");
        tailRotor = FindChildByName(transform, "Blade2");
        AudioClip rotorClip = Resources.Load<AudioClip>("Audio/HelicopterRotor");
        if (rotorClip != null)
        {
            rotorAudio = gameObject.AddComponent<AudioSource>();
            rotorAudio.clip = rotorClip;
            rotorAudio.loop = true;
            rotorAudio.playOnAwake = false;
            rotorAudio.spatialBlend = 0.65f;
            rotorAudio.rolloffMode = AudioRolloffMode.Linear;
            rotorAudio.minDistance = 12f;
            rotorAudio.maxDistance = 100f;
            rotorAudio.volume = 0.85f;
        }
    }

    void Update()
    {
        float speed = hasLanded ? landedRotorSpeed : rotorSpeed;
        float rotation = speed * Time.deltaTime;

        if (mainRotor != null)
        {
            mainRotor.Rotate(Vector3.up, rotation, Space.World);
        }

        if (tailRotor != null)
        {
            tailRotor.Rotate(Vector3.up, -rotation * 1.35f, Space.Self);
        }
    }

    public void Configure(Transform target)
    {
        landingTarget = target;
    }

    void Start()
    {
        if (legacyAnimation != null && legacyAnimation[flyingClip] != null)
        {
            legacyAnimation[flyingClip].wrapMode = WrapMode.Loop;
            legacyAnimation.Play(flyingClip);
        }
    }

    public void Land()
    {
        if (landingStarted)
        {
            return;
        }

        landingStarted = true;
        StartRotorAudio();

        if (legacyAnimation != null && legacyAnimation[landingClip] != null)
        {
            legacyAnimation[landingClip].wrapMode = WrapMode.ClampForever;
            legacyAnimation.Play(landingClip);
            StartCoroutine(WaitForLegacyLanding());
        }
        else
        {
            StartCoroutine(LandWithTransform());
        }
    }

    IEnumerator WaitForLegacyLanding()
    {
        float length = legacyAnimation[landingClip].length;
        yield return new WaitForSeconds(length);
        FinishLanding();
    }

    IEnumerator LandWithTransform()
    {
        Vector3 start = transform.position;
        Vector3 target = landingTarget != null ? landingTarget.position : new Vector3(start.x, 0f, start.z);
        float elapsed = 0f;

        while (elapsed < landingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / landingDuration);
            transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        FinishLanding();
    }

    void FinishLanding()
    {
        hasLanded = true;

        if (rotorAudio != null)
        {
            rotorAudio.volume = 0.48f;
        }

        if (legacyAnimation != null && legacyAnimation[landedClip] != null)
        {
            legacyAnimation[landedClip].wrapMode = WrapMode.ClampForever;
            legacyAnimation.Play(landedClip);
        }
    }

    void StartRotorAudio()
    {
        if (rotorAudio == null || rotorAudio.clip == null)
        {
            return;
        }

        rotorAudio.Stop();
        rotorAudio.time = 0f;
        rotorAudio.volume = 0.85f;
        rotorAudio.Play();
    }

    static Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }
}
