using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpSpeed = 6f;
    [SerializeField] float gravity = -24f;

    CharacterController controller;
    Health health;
    float verticalVelocity;

    public Vector2 LastMoveInput { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (MainMenuGui.IsMenuOpen || GameManager.HasPlayerWon || (health != null && health.IsDead))
        {
            LastMoveInput = Vector2.zero;
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        LastMoveInput = new Vector2(horizontal, vertical);

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = jumpSpeed;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move = move * moveSpeed;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}
