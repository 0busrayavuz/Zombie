using UnityEngine;

public class MedikitPickup : MonoBehaviour
{
    [SerializeField] int healAmount = 35;
    [SerializeField] float pickupDistance = 1.6f;
    [SerializeField] float rotationSpeed = 70f;
    [SerializeField] float bobHeight = 0.18f;
    [SerializeField] float bobSpeed = 2.2f;

    Vector3 startPosition;
    Transform player;
    Health playerHealth;
    bool collected;

    void Start()
    {
        startPosition = transform.position;
        FindPlayer();
        KeepWhite();
    }

    void Update()
    {
        if (collected)
        {
            return;
        }

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);

        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
        {
            return;
        }

        Vector3 difference = player.position - transform.position;
        difference.y = 0f;
        if (difference.sqrMagnitude <= pickupDistance * pickupDistance)
        {
            TryCollect();
        }
    }

    void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            return;
        }

        player = playerObject.transform;
        playerHealth = playerObject.GetComponent<Health>();
    }

    void TryCollect()
    {
        if (collected || playerHealth == null || playerHealth.IsDead ||
            playerHealth.CurrentHealth >= playerHealth.MaximumHealth)
        {
            return;
        }

        collected = true;
        playerHealth.Heal(healAmount);

        AudioClip healClip = Resources.Load<AudioClip>("Audio/health1");
        if (healClip != null)
        {
            AudioSource.PlayClipAtPoint(healClip, transform.position, 0.8f);
        }

        Destroy(gameObject);
    }

    void KeepWhite()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material material = renderer.material;
        material.name = "Medikit White";
        material.color = Color.white;
    }
}
