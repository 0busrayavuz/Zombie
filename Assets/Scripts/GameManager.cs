using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static bool hasPlayerWon;

    [SerializeField] float secondsToLand = 180f;
    [SerializeField] bool winWhenTimerExpiresWithoutHelicopter = false;
    [SerializeField] Helicopter helicopter;

    Health playerHealth;
    float landingTime;

    public static event Action PlayerWon;
    public static bool HasPlayerWon { get { return hasPlayerWon; } }
    public static float RescueTimeRemaining { get; private set; }
    public static bool RescueHasArrived { get; private set; }

    public void Configure(float landingDelay, Helicopter rescueHelicopter)
    {
        secondsToLand = landingDelay;
        helicopter = rescueHelicopter;
    }

    void Awake()
    {
        hasPlayerWon = false;
        RescueHasArrived = false;
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }

        if (helicopter == null)
        {
            helicopter = FindFirstObjectByType<Helicopter>();
        }

        if (secondsToLand > 0f)
        {
            landingTime = Time.time + secondsToLand;
            RescueTimeRemaining = secondsToLand;
            Invoke("LandOrWin", secondsToLand);
        }
    }

    void Update()
    {
        if (!RescueHasArrived)
        {
            RescueTimeRemaining = Mathf.Max(0f, landingTime - Time.time);
        }
    }

    void LandOrWin()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        if (helicopter != null)
        {
            RescueHasArrived = true;
            RescueTimeRemaining = 0f;
            helicopter.Land();
        }
        else if (winWhenTimerExpiresWithoutHelicopter)
        {
            WinPlayer();
        }
    }

    public static void WinPlayer()
    {
        if (hasPlayerWon)
        {
            return;
        }

        hasPlayerWon = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerWon?.Invoke();
    }
}
