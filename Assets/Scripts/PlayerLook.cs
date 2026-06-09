using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] float mouseSensitivityX = 4f;
    [SerializeField] float mouseSensitivityY = 3f;
    [SerializeField] float minimumPitch = -45f;
    [SerializeField] float maximumPitch = 60f;

    Health health;
    float pitch;

    void Awake()
    {
        health = GetComponent<Health>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        LockCursor();
    }

    void Update()
    {
        if (MainMenuGui.IsMenuOpen)
        {
            UnlockCursor();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        if (Input.GetButtonDown("Fire1"))
        {
            LockCursor();
        }

        if (GameManager.HasPlayerWon || (health != null && health.IsDead))
        {
            UnlockCursor();
            return;
        }

        float yaw = Input.GetAxis("Mouse X") * mouseSensitivityX;
        transform.Rotate(0f, yaw, 0f);

        if (playerCamera != null)
        {
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
            pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }
    }

    static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
