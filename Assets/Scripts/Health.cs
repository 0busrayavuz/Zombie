using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [SerializeField] int maximumHealth = 100;
    [SerializeField] bool destroyEnemyWhenNoRagdoll = true;

    int currentHealth;
    bool isDead;
    AudioClip[] playerHitSounds;
    AudioClip playerDeathSound;
    AudioSource playerDeathAudio;

    public event Action<Health> Died;
    public event Action<int, int> HealthChanged;

    public int CurrentHealth { get { return currentHealth; } }
    public int MaximumHealth { get { return maximumHealth; } }
    public bool IsDead { get { return isDead; } }

    void Awake()
    {
        maximumHealth = Mathf.Max(1, maximumHealth);
        currentHealth = maximumHealth;
        playerHitSounds = Resources.LoadAll<AudioClip>("Audio/PlayerHits");
        playerDeathSound = Resources.Load<AudioClip>("Audio/PlayerDeath");

        if (CompareTag("Player") && playerDeathSound != null)
        {
            AudioListener.pause = false;
            playerDeathAudio = gameObject.AddComponent<AudioSource>();
            playerDeathAudio.clip = playerDeathSound;
            playerDeathAudio.playOnAwake = false;
            playerDeathAudio.loop = false;
            playerDeathAudio.spatialBlend = 0f;
            playerDeathAudio.priority = 0;
            playerDeathAudio.volume = 1f;
            playerDeathAudio.ignoreListenerPause = true;
        }
    }

    public void Damage(int damageValue)
    {
        if (isDead || damageValue <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damageValue);
        HealthChanged?.Invoke(currentHealth, maximumHealth);
        if (currentHealth > 0)
        {
            PlayHitSound();
        }

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int healValue)
    {
        if (isDead || healValue <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(maximumHealth, currentHealth + healValue);
        HealthChanged?.Invoke(currentHealth, maximumHealth);
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        bool isPlayer = CompareTag("Player");
        if (isPlayer && playerDeathSound != null)
        {
            PlayPlayerDeathAudio();
        }

        if (!isPlayer)
        {
            EnemySpawnManager.OnEnemyDeath();
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            if (stats != null)
            {
                stats.ZombiesKilled++;
            }

            EnemyDrops drops = GetComponent<EnemyDrops>();
            if (drops != null)
            {
                drops.OnDeath();
            }
        }

        DisableLivingComponents(isPlayer);

        Ragdoll ragdoll = GetComponent<Ragdoll>();
        if (ragdoll != null)
        {
            ragdoll.OnDeath();
        }
        else if (!isPlayer && destroyEnemyWhenNoRagdoll)
        {
            Destroy(gameObject, 0.05f);
        }

        if (!isPlayer)
        {
            AudioClip deathClip = Resources.Load<AudioClip>("Audio/ZombieDeath");
            if (deathClip != null)
            {
                AudioSource.PlayClipAtPoint(deathClip, transform.position, 0.8f);
            }
        }

        Died?.Invoke(this);
    }

    void PlayPlayerDeathAudio()
    {
        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (source != playerDeathAudio)
            {
                source.Stop();
            }
        }

        if (playerDeathAudio != null)
        {
            AudioListener.pause = true;
            playerDeathAudio.time = 0f;
            playerDeathAudio.Play();
        }
    }

    void DisableLivingComponents(bool isPlayer)
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        PlayerLook playerLook = GetComponent<PlayerLook>();
        if (playerLook != null)
        {
            playerLook.enabled = false;
        }

        RifleWeapon rifleWeapon = GetComponent<RifleWeapon>();
        if (rifleWeapon != null)
        {
            rifleWeapon.enabled = false;
        }

        PlayerAnimation playerAnimation = GetComponent<PlayerAnimation>();
        if (playerAnimation != null)
        {
            playerAnimation.enabled = false;
        }

        MissileWeapon missileWeapon = GetComponent<MissileWeapon>();
        if (missileWeapon != null)
        {
            missileWeapon.enabled = false;
        }

        WeaponSwitcher weaponSwitcher = GetComponent<WeaponSwitcher>();
        if (weaponSwitcher != null)
        {
            weaponSwitcher.enabled = false;
        }

        EnemyMovement enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        EnemyAttack enemyAttack = GetComponentInChildren<EnemyAttack>();
        if (enemyAttack != null)
        {
            enemyAttack.enabled = false;
        }

        EnemyAnimation enemyAnimation = GetComponent<EnemyAnimation>();
        if (enemyAnimation != null)
        {
            enemyAnimation.enabled = false;
        }

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null && !isPlayer)
        {
            controller.enabled = false;
        }
    }

    public override string ToString()
    {
        return currentHealth + " / " + maximumHealth;
    }

    void PlayHitSound()
    {
        if (!CompareTag("Player") || playerHitSounds == null || playerHitSounds.Length == 0)
        {
            return;
        }

        AudioClip clip = playerHitSounds[UnityEngine.Random.Range(0, playerHitSounds.Length)];
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.75f);
    }
}
