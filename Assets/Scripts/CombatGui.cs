using UnityEngine;

public class CombatGui : MonoBehaviour
{
    Health playerHealth;
    PlayerStats playerStats;
    GUIStyle statStyle;
    Texture2D gameOverTexture;
    Texture2D winTexture;

    void Awake()
    {
        gameOverTexture = Resources.Load<Texture2D>("Gui/GameOverScreen");
        winTexture = Resources.Load<Texture2D>("Gui/WinScreen");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    void OnGUI()
    {
        if (MainMenuGui.IsMenuOpen)
        {
            return;
        }

        EnsureStyles();

        if (playerHealth != null && playerHealth.IsDead)
        {
            DrawEndScreen(gameOverTexture, 420f);
        }
        else if (GameManager.HasPlayerWon)
        {
            DrawEndScreen(winTexture, 760f);
        }
    }

    void DrawEndScreen(Texture2D texture, float preferredWidth)
    {
        if (texture == null)
        {
            return;
        }

        float maxWidth = Mathf.Min(preferredWidth, Screen.width * 0.78f);
        float maxHeight = Screen.height * 0.62f;
        float aspect = (float)texture.width / texture.height;
        float width = maxWidth;
        float height = width / aspect;

        if (height > maxHeight)
        {
            height = maxHeight;
            width = height * aspect;
        }

        Rect imageRect = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f - 18f,
            width,
            height
        );
        GUI.DrawTexture(imageRect, texture, ScaleMode.ScaleToFit, true);

        int kills = playerStats == null ? 0 : playerStats.ZombiesKilled;
        GUI.Label(
            new Rect(imageRect.x, imageRect.yMax + 8f, imageRect.width, 30f),
            "Zombies killed: " + kills,
            statStyle
        );
    }

    void EnsureStyles()
    {
        if (statStyle != null)
        {
            return;
        }

        statStyle = new GUIStyle(GUI.skin.label);
        statStyle.alignment = TextAnchor.MiddleCenter;
        statStyle.fontSize = 17;
        statStyle.fontStyle = FontStyle.Bold;
        statStyle.normal.textColor = Color.white;
    }
}
