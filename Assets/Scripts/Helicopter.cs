using System.Collections;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    [SerializeField] Transform landingTarget;
    [SerializeField] float landingDuration = 5f;
    [SerializeField] string flyingClip = "Flying";
    [SerializeField] string landingClip = "Landing";
    [SerializeField] string landedClip = "Landed";

    Animation legacyAnimation;
    AudioSource rotorAudio;
    bool hasLanded;
    bool landingStarted;

    public bool HasLanded { get { return hasLanded; } }

    void Awake()
    {
        legacyAnimation = GetComponent<Animation>();
        AudioClip rotorClip = Resources.Load<AudioClip>("Audio/HelicopterRotor");
        if (rotorClip != null)
        {
            rotorAudio = gameObject.AddComponent<AudioSource>();
            rotorAudio.clip = rotorClip;
            rotorAudio.loop = true;
            rotorAudio.spatialBlend = 1f;
            rotorAudio.minDistance = 5f;
            rotorAudio.maxDistance = 60f;
            rotorAudio.volume = 0.55f;
            rotorAudio.Play();
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

        if (legacyAnimation != null && legacyAnimation[landedClip] != null)
        {
            legacyAnimation[landedClip].wrapMode = WrapMode.ClampForever;
            legacyAnimation.Play(landedClip);
        }
    }
}
