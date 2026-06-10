using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static bool hasPlayerWon;
    static GameManager instance;

    [SerializeField] float secondsToLand = 180f;
    [SerializeField] bool winWhenTimerExpiresWithoutHelicopter = false;
    [SerializeField] Helicopter helicopter;

    Health playerHealth;
    AudioSource victoryAudio;
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

    public void ApplyDifficulty(int difficulty)
    {
        secondsToLand = difficulty == 0 ? 15f : difficulty == 2 ? 35f : 25f;
        ScheduleHelicopter();
    }

    void Awake()
    {
        instance = this;
        hasPlayerWon = false;
        RescueHasArrived = false;

        AudioClip victoryClip = Resources.Load<AudioClip>("Audio/VictoryMusic");
        if (victoryClip != null)
        {
            victoryAudio = gameObject.AddComponent<AudioSource>();
            victoryAudio.clip = victoryClip;
            victoryAudio.playOnAwake = false;
            victoryAudio.loop = false;
            victoryAudio.spatialBlend = 0f;
            victoryAudio.volume = 0.8f;
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
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

        ApplyDifficulty(GameSettings.Difficulty);
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

    void ScheduleHelicopter()
    {
        CancelInvoke("LandOrWin");
        RescueHasArrived = false;
        landingTime = Time.time + secondsToLand;
        RescueTimeRemaining = secondsToLand;

        if (secondsToLand > 0f)
        {
            Invoke("LandOrWin", secondsToLand);
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
        if (instance != null)
        {
            instance.PlayVictoryAudio();
        }

        PlayerWon?.Invoke();
    }

    void PlayVictoryAudio()
    {
        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (source != victoryAudio && source.loop)
            {
                source.Stop();
            }
        }

        if (victoryAudio != null)
        {
            victoryAudio.time = victoryAudio.clip.length > 60f ? 60f : 0f;
            victoryAudio.Play();
        }
    }
}
