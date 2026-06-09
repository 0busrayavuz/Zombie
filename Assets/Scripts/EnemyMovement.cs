using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3.5f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float stoppingDistance = 1.3f;
    [SerializeField] float gravity = -24f;

    CharacterController controller;
    Transform player;
    Health ownHealth;
    Health playerHealth;
    float verticalVelocity;

    public float NormalizedSpeed { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        ownHealth = GetComponent<Health>();
    }

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (GameManager.HasPlayerWon || (ownHealth != null && ownHealth.IsDead))
        {
            NormalizedSpeed = 0f;
            return;
        }

        if (player == null)
        {
            FindPlayer();
            if (player == null)
            {
                NormalizedSpeed = 0f;
                return;
            }
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            NormalizedSpeed = 0f;
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        Vector3 direction = distance > stoppingDistance ? toPlayer.normalized : Vector3.zero;

        RotateToward(direction);
        Move(direction);
        NormalizedSpeed = direction.sqrMagnitude > 0f ? 1f : 0f;
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

    void RotateToward(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void Move(Vector3 direction)
    {
        if (controller != null && controller.enabled)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = direction * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}
