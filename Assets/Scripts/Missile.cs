using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] float speed = 30f;
    [SerializeField] int damageDealt = 90;
    [SerializeField] float explosionRadius = 4.5f;

    Transform owner;
    Rigidbody body;
    bool exploded;

    public void Launch(Transform missileOwner, Vector3 direction)
    {
        owner = missileOwner;
        body = GetComponent<Rigidbody>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.linearVelocity = direction.normalized * speed;
        transform.forward = direction.normalized;
        Destroy(gameObject, 7f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (exploded || (owner != null && other.transform.root == owner.root))
        {
            return;
        }

        Explode();
    }

    void Explode()
    {
        exploded = true;
        CombatEffects.CreateExplosion(transform.position);

        HashSet<Health> damaged = new HashSet<Health>();
        foreach (Collider collider in Physics.OverlapSphere(
            transform.position,
            explosionRadius,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore))
        {
            Health health = collider.GetComponentInParent<Health>();
            if (health == null || damaged.Contains(health) ||
                (owner != null && health.transform.root == owner.root))
            {
                continue;
            }

            damaged.Add(health);
            health.Damage(damageDealt);
        }

        Destroy(gameObject);
    }
}
