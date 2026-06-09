using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] bool startAsKinematic = true;
    [SerializeField] float deathForce = 2f;

    Rigidbody[] bodies;
    Animator animator;
    Animation legacyAnimation;
    bool isDead;

    void Awake()
    {
        bodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        legacyAnimation = GetComponentInChildren<Animation>();

        if (startAsKinematic)
        {
            SetRagdollActive(false);
        }
    }

    public void OnDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (legacyAnimation != null)
        {
            legacyAnimation.Stop();
            legacyAnimation.enabled = false;
        }

        SetRagdollActive(true);

        foreach (Rigidbody body in bodies)
        {
            if (body != null)
            {
                body.AddForce(Random.insideUnitSphere * deathForce, ForceMode.Impulse);
            }
        }
    }

    void SetRagdollActive(bool active)
    {
        foreach (Rigidbody body in bodies)
        {
            if (body != null)
            {
                body.isKinematic = !active;
                Collider[] bodyColliders = body.GetComponents<Collider>();
                foreach (Collider colliderPart in bodyColliders)
                {
                    if (colliderPart != null)
                    {
                        colliderPart.enabled = active;
                    }
                }
            }
        }
    }
}
