using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] string[] legacyWalkAnimations = { "Move1", "Move2", "Move3" };
    [SerializeField] string animatorSpeedParameter = "Speed";

    Animation legacyAnimation;
    Animator animator;
    EnemyMovement movement;

    void Awake()
    {
        RefreshAnimationReferences();
        movement = GetComponent<EnemyMovement>();
    }

    void Start()
    {
        RefreshAnimationReferences();
        PlayRandomLegacyWalk();
    }

    void Update()
    {
        if (animator != null && animator.runtimeAnimatorController != null && movement != null)
        {
            animator.SetFloat(animatorSpeedParameter, movement.NormalizedSpeed);
        }
    }

    void PlayRandomLegacyWalk()
    {
        if (legacyAnimation == null || legacyWalkAnimations == null || legacyWalkAnimations.Length == 0)
        {
            return;
        }

        string clipName = legacyWalkAnimations[Random.Range(0, legacyWalkAnimations.Length)];
        AnimationState state = legacyAnimation[clipName];
        if (state == null)
        {
            return;
        }

        state.wrapMode = WrapMode.Loop;
        legacyAnimation.Play(clipName);
        state.normalizedTime = Random.value;
    }

    void RefreshAnimationReferences()
    {
        legacyAnimation = GetComponentInChildren<Animation>();
        animator = GetComponentInChildren<Animator>();
    }
}
