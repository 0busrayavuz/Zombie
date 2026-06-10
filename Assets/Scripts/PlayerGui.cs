using UnityEngine;

public class PlayerGui : MonoBehaviour
{
    [SerializeField] Texture2D crosshairTexture;

    Health playerHealth;
    PlayerStats playerStats;
    WeaponSwitcher weaponSwitcher;
    GUIStyle smallStyle;
    GUIStyle rightStyle;
    GUIStyle captionStyle;
    GUIStyle valueStyle;
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
        float panelWidth = Mathf.Min(310f, Screen.width * 0.42f);
        Rect panel = new Rect(22f, Screen.height - 104f, panelWidth, 76f);
        DrawPanel(panel, new Color(0.025f, 0.025f, 0.03f, 0.9f), new Color(0.72f, 0.06f, 0.05f, 0.95f));

        GUI.Label(new Rect(panel.x + 14f, panel.y + 8f, 90f, 20f), "HEALTH", captionStyle);
        GUI.Label(
            new Rect(panel.xMax - 112f, panel.y + 7f, 98f, 22f),
            playerHealth.CurrentHealth + " / " + playerHealth.MaximumHealth,
            valueStyle
        );

        int segmentCount = 10;
        float gap = 4f;
        float barX = panel.x + 14f;
        float barY = panel.y + 38f;
        float barWidth = panel.width - 28f;
        float segmentWidth = (barWidth - gap * (segmentCount - 1)) / segmentCount;
        int filledSegments = Mathf.CeilToInt(ratio * segmentCount);
        Color healthColor = Color.Lerp(
            new Color(0.65f, 0.025f, 0.02f),
            new Color(0.95f, 0.16f, 0.08f),
            ratio
        );

        for (int i = 0; i < segmentCount; i++)
        {
            Rect segment = new Rect(barX + i * (segmentWidth + gap), barY, segmentWidth, 22f);
            DrawColorRect(
                segment,
                i < filledSegments ? healthColor : new Color(0.18f, 0.18f, 0.2f, 0.8f)
            );
        }
    }

    void DrawMissionInfo()
    {
        int kills = playerStats == null ? 0 : playerStats.ZombiesKilled;
        string rescue = GameManager.RescueHasArrived
            ? "HELICOPTER READY"
            : "HELICOPTER ARRIVES IN  " + Mathf.CeilToInt(GameManager.RescueTimeRemaining) + "s";

        float missionWidth = Mathf.Min(330f, Screen.width * 0.42f);
        Rect missionPanel = new Rect((Screen.width - missionWidth) * 0.5f, 18f, missionWidth, 38f);
        DrawPanel(
            missionPanel,
            new Color(0.025f, 0.025f, 0.03f, 0.86f),
            GameManager.RescueHasArrived
                ? new Color(0.15f, 0.72f, 0.32f, 0.95f)
                : new Color(0.82f, 0.48f, 0.05f, 0.95f)
        );
        GUI.Label(missionPanel, rescue, valueStyle);

        float statWidth = 132f;
        Rect killsPanel = new Rect(Screen.width - statWidth - 22f, 18f, statWidth, 52f);
        Rect activePanel = new Rect(Screen.width - statWidth - 22f, 78f, statWidth, 52f);
        DrawStatPanel(killsPanel, "KILLS", kills, new Color(0.72f, 0.06f, 0.05f));
        DrawStatPanel(
            activePanel,
            "ACTIVE ZOMBIES",
            EnemySpawnManager.LivingZombies,
            new Color(0.82f, 0.48f, 0.05f)
        );

        if (weaponSwitcher != null)
        {
            Rect weaponPanel = new Rect(Screen.width - 184f, Screen.height - 68f, 162f, 42f);
            DrawPanel(
                weaponPanel,
                new Color(0.025f, 0.025f, 0.03f, 0.88f),
                new Color(0.5f, 0.5f, 0.54f, 0.9f)
            );
            GUI.Label(weaponPanel, weaponSwitcher.CurrentWeaponName.ToUpperInvariant(), valueStyle);
        }
    }

    void DrawStatPanel(Rect rect, string caption, int value, Color accent)
    {
        DrawPanel(rect, new Color(0.025f, 0.025f, 0.03f, 0.88f), accent);
        GUI.Label(new Rect(rect.x + 8f, rect.y + 5f, rect.width - 16f, 17f), caption, captionStyle);
        GUI.Label(new Rect(rect.x + 8f, rect.y + 21f, rect.width - 16f, 26f), value.ToString(), valueStyle);
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

        captionStyle = new GUIStyle(smallStyle);
        captionStyle.fontSize = 11;
        captionStyle.alignment = TextAnchor.MiddleLeft;
        captionStyle.normal.textColor = new Color(0.72f, 0.72f, 0.76f);

        valueStyle = new GUIStyle(smallStyle);
        valueStyle.fontSize = 16;
        valueStyle.alignment = TextAnchor.MiddleCenter;
        valueStyle.normal.textColor = Color.white;
    }

    static void DrawPanel(Rect rect, Color background, Color accent)
    {
        DrawColorRect(rect, background);
        DrawColorRect(new Rect(rect.x, rect.y, 4f, rect.height), accent);
        DrawColorRect(new Rect(rect.x, rect.y, rect.width, 2f), new Color(accent.r, accent.g, accent.b, 0.55f));
    }

    static void DrawColorRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
