using UnityEngine;

public class PlayerGui : MonoBehaviour
{
    [SerializeField] Texture2D crosshairTexture;

    Health playerHealth;
    PlayerStats playerStats;
    WeaponSwitcher weaponSwitcher;
    GUIStyle smallStyle;
    GUIStyle rightStyle;
    float damageFlash;
    int previousHealth;

    void Awake()
    {
        playerHealth = GetComponent<Health>();
        playerStats = GetComponent<PlayerStats>();
        weaponSwitcher = GetComponent<WeaponSwitcher>();
        crosshairTexture = crosshairTexture != null
            ? crosshairTexture
            : Resources.Load<Texture2D>("Textures/crosshair");

        if (playerHealth != null)
        {
            previousHealth = playerHealth.CurrentHealth;
            playerHealth.HealthChanged += OnHealthChanged;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
        }
    }

    void Update()
    {
        damageFlash = Mathf.MoveTowards(damageFlash, 0f, Time.deltaTime * 1.8f);
    }

    void OnHealthChanged(int current, int maximum)
    {
        if (current < previousHealth)
        {
            damageFlash = Mathf.Max(damageFlash, 0.32f);
        }

        previousHealth = current;
    }

    void OnGUI()
    {
        if (MainMenuGui.IsMenuOpen)
        {
            return;
        }

        EnsureStyles();
        DrawHealth();
        DrawMissionInfo();

        if (!GameManager.HasPlayerWon && (playerHealth == null || !playerHealth.IsDead))
        {
            DrawCrosshair();
        }

        if (damageFlash > 0f)
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.65f, 0f, 0f, damageFlash);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }

    void DrawHealth()
    {
        if (playerHealth == null)
        {
            return;
        }

        float ratio = playerHealth.CurrentHealth / (float)playerHealth.MaximumHealth;
        Rect background = new Rect(18f, 18f, 230f, 24f);
        Rect fill = new Rect(background.x + 3f, background.y + 3f, (background.width - 6f) * ratio, background.height - 6f);

        DrawColorRect(background, new Color(0.03f, 0.04f, 0.05f, 0.88f));
        Color healthColor = Color.Lerp(new Color(0.75f, 0.08f, 0.05f), new Color(0.14f, 0.72f, 0.3f), ratio);
        DrawColorRect(fill, healthColor);
        GUI.Label(new Rect(26f, 19f, 210f, 22f), playerHealth.CurrentHealth + " / " + playerHealth.MaximumHealth, smallStyle);
    }

    void DrawMissionInfo()
    {
        int kills = playerStats == null ? 0 : playerStats.ZombiesKilled;
        string rescue = GameManager.RescueHasArrived
            ? "RESCUE READY"
            : "RESCUE  " + Mathf.CeilToInt(GameManager.RescueTimeRemaining) + "s";

        GUI.Label(new Rect(Screen.width - 230f, 18f, 210f, 24f), rescue, rightStyle);
        GUI.Label(new Rect(Screen.width - 230f, 43f, 210f, 24f), "KILLS  " + kills + "   ACTIVE  " + EnemySpawnManager.LivingZombies, rightStyle);

        if (weaponSwitcher != null)
        {
            GUI.Label(
                new Rect(Screen.width - 230f, Screen.height - 48f, 210f, 24f),
                weaponSwitcher.CurrentWeaponName,
                rightStyle
            );
        }
    }

    void DrawCrosshair()
    {
        if (crosshairTexture == null)
        {
            return;
        }

        float width = crosshairTexture.width;
        float height = crosshairTexture.height;
        GUI.DrawTexture(
            new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height),
            crosshairTexture
        );
    }

    void EnsureStyles()
    {
        if (smallStyle != null)
        {
            return;
        }

        smallStyle = new GUIStyle(GUI.skin.label);
        smallStyle.fontSize = 14;
        smallStyle.fontStyle = FontStyle.Bold;
        smallStyle.alignment = TextAnchor.MiddleCenter;
        smallStyle.normal.textColor = Color.white;

        rightStyle = new GUIStyle(smallStyle);
        rightStyle.alignment = TextAnchor.MiddleRight;
    }

    static void DrawColorRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
