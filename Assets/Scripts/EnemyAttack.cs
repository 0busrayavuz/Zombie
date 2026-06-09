using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] int damageDealt = 5;
    [SerializeField] float attackDelay = 1f;

    Health ownHealth;
    float nextTimeAttackIsAllowed = -1f;

    void Awake()
    {
        ownHealth = GetComponentInParent<Health>();
    }

    void OnTriggerStay(Collider other)
    {
        if (ownHealth != null && ownHealth.IsDead)
        {
            return;
        }

        Transform root = other.transform.root;
        if (!root.CompareTag("Player") || Time.time < nextTimeAttackIsAllowed)
        {
            return;
        }

        Health playerHealth = root.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.Damage(damageDealt);
            Vector3 hitDirection = (transform.root.position - root.position).normalized;
            CombatEffects.CreateBlood(root.position + Vector3.up * 1.35f, hitDirection);
            nextTimeAttackIsAllowed = Time.time + attackDelay;
        }
    }
}
