using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    const string PlayerModelResourcePath = "Models/Player/Player";

    [SerializeField] string horizontalParameter = "Horizontal";
    [SerializeField] string verticalParameter = "Vertical";
    [SerializeField] string motionParameter = "Motion";
    [SerializeField] string idleAnimation = "Idle";
    [SerializeField] string runForwardAnimation = "RunForward";
    [SerializeField] string runBackwardAnimation = "RunBackward";
    [SerializeField] string runLeftAnimation = "RunLeft";
    [SerializeField] string runRightAnimation = "RunRight";

    Animator animator;
    Animation legacyAnimation;
    Health health;
    string currentLegacyClip;

    void Awake()
    {
        RefreshAnimationReferences();
        health = GetComponent<Health>();
    }

    void Start()
    {
        RefreshAnimationReferences();
        EnsureLegacyClipsAreConnected();
        ConfigureLegacyClip(idleAnimation);
        ConfigureLegacyClip(runForwardAnimation);
        ConfigureLegacyClip(runBackwardAnimation);
        ConfigureLegacyClip(runLeftAnimation);
        ConfigureLegacyClip(runRightAnimation);
        PlayLegacyClip(idleAnimation, true);
    }

    void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;
        bool canMove = !MainMenuGui.IsMenuOpen &&
            !GameManager.HasPlayerWon &&
            (health == null || !health.IsDead);

        if (canMove)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }

        bool isMoving = Mathf.Abs(horizontal) > 0.05f || Mathf.Abs(vertical) > 0.05f;
        UpdateAnimator(horizontal, vertical, isMoving);
        UpdateLegacyAnimation(horizontal, vertical, isMoving);
    }

    void RefreshAnimationReferences()
    {
        animator = GetComponentInChildren<Animator>();
        legacyAnimation = GetComponentInChildren<Animation>();
    }

    void EnsureLegacyClipsAreConnected()
    {
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>(PlayerModelResourcePath);
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("Player animasyon klipleri bulunamadi: " + PlayerModelResourcePath, this);
            return;
        }

        if (legacyAnimation == null)
        {
            Transform visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;
            legacyAnimation = visualRoot.gameObject.AddComponent<Animation>();
        }

        legacyAnimation.playAutomatically = false;
        legacyAnimation.cullingType = AnimationCullingType.AlwaysAnimate;

        foreach (AnimationClip clip in clips)
        {
            if (clip == null || legacyAnimation.GetClip(clip.name) != null)
            {
                continue;
            }

            legacyAnimation.AddClip(clip, clip.name);
        }
    }

    void UpdateAnimator(float horizontal, float vertical, bool isMoving)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.SetFloat(horizontalParameter, horizontal);
        animator.SetFloat(verticalParameter, vertical);
        animator.SetBool(motionParameter, isMoving);
    }

    void UpdateLegacyAnimation(float horizontal, float vertical, bool isMoving)
    {
        if (legacyAnimation == null)
        {
            RefreshAnimationReferences();
            if (legacyAnimation == null)
            {
                return;
            }
        }

        if (!isMoving)
        {
            PlayLegacyClip(idleAnimation);
            return;
        }

        string clipToPlay;
        if (Mathf.Abs(vertical) >= Mathf.Abs(horizontal))
        {
            clipToPlay = vertical >= 0f ? runForwardAnimation : runBackwardAnimation;
        }
        else
        {
            clipToPlay = horizontal >= 0f ? runRightAnimation : runLeftAnimation;
        }

        PlayLegacyClip(clipToPlay);
    }

    void ConfigureLegacyClip(string clipName)
    {
        if (legacyAnimation == null || string.IsNullOrEmpty(clipName))
        {
            return;
        }

        AnimationState state = legacyAnimation[clipName];
        if (state != null)
        {
            state.wrapMode = WrapMode.Loop;
            state.speed = 1.05f;
        }
    }

    void PlayLegacyClip(string clipName, bool immediately = false)
    {
        if (legacyAnimation == null || string.IsNullOrEmpty(clipName))
        {
            return;
        }

        AnimationState state = legacyAnimation[clipName];
        if (state == null || currentLegacyClip == clipName)
        {
            return;
        }

        state.wrapMode = WrapMode.Loop;
        if (immediately)
        {
            legacyAnimation.Play(clipName);
        }
        else
        {
            legacyAnimation.CrossFade(clipName, 0.12f);
        }

        currentLegacyClip = clipName;
    }
}
