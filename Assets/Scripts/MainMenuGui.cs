using UnityEngine;

public static class GameSettings
{
    public static int Difficulty
    {
        get { return PlayerPrefs.GetInt("Difficulty", 1); }
        set
        {
            PlayerPrefs.SetInt("Difficulty", Mathf.Clamp(value, 0, 2));
            PlayerPrefs.Save();
        }
    }

    public static float SoundVolume
    {
        get { return PlayerPrefs.GetFloat("SoundVolume", 0.8f); }
        set
        {
            PlayerPrefs.SetFloat("SoundVolume", Mathf.Clamp01(value));
            PlayerPrefs.Save();
            AudioListener.volume = Mathf.Clamp01(value);
        }
    }
}

public class MainMenuGui : MonoBehaviour
{
    public static bool IsMenuOpen { get; private set; }
    static MainMenuGui instance;

    Texture2D background;
    Texture2D title;
    Texture2D newGameButton;
    Texture2D optionsButton;
    Texture2D quitButton;
    Texture2D okButton;
    GUIStyle imageButtonStyle;
    GUIStyle headingStyle;
    GUIStyle labelStyle;
    bool optionsOpen;
    bool gameStarted;
    bool startRequested;
    int selectedDifficulty;
    float soundVolume;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        background = Resources.Load<Texture2D>("Gui/MainMenuBg");
        title = Resources.Load<Texture2D>("Gui/Title");
        newGameButton = Resources.Load<Texture2D>("Gui/ButtonNewGame");
        optionsButton = Resources.Load<Texture2D>("Gui/ButtonOptions");
        quitButton = Resources.Load<Texture2D>("Gui/ButtonQuit");
        okButton = Resources.Load<Texture2D>("Gui/ButtonOk");

        selectedDifficulty = GameSettings.Difficulty;
        soundVolume = GameSettings.SoundVolume;
        AudioListener.volume = soundVolume;
        OpenMenu();
    }

    void Update()
    {
        if (startRequested)
        {
            startRequested = false;
            StartGame();
            return;
        }

        if (gameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        Time.timeScale = 1f;
    }

    void OpenMenu()
    {
        IsMenuOpen = true;
        optionsOpen = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void StartGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            ZombieSurvivalBootstrapper bootstrapper =
                FindFirstObjectByType<ZombieSurvivalBootstrapper>();
            if (bootstrapper != null)
            {
                bootstrapper.BuildScene();
            }
        }

        EnemySpawnManager spawnManager = FindFirstObjectByType<EnemySpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.ApplyDifficulty(GameSettings.Difficulty);
        }

        IsMenuOpen = false;
        optionsOpen = false;
        gameStarted = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void CloseMenu()
    {
        IsMenuOpen = false;
        optionsOpen = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        if (!IsMenuOpen)
        {
            return;
        }

        EnsureStyles();
        DrawBackground();

        if (optionsOpen)
        {
            DrawOptions();
        }
        else
        {
            DrawMainMenu();
        }
    }

    void DrawBackground()
    {
        if (background != null)
        {
            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                background,
                ScaleMode.ScaleAndCrop
            );
        }
        else
        {
            DrawColorRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.08f, 0.01f, 0.01f));
        }

        DrawColorRect(
            new Rect(0f, 0f, Screen.width, Screen.height),
            new Color(0f, 0f, 0f, 0.18f)
        );

        if (title != null)
        {
            float width = Mathf.Min(title.width, Screen.width * 0.55f);
            float height = width * title.height / title.width;
            GUI.DrawTexture(
                new Rect(Screen.width - width - 24f, 22f, width, height),
                title,
                ScaleMode.ScaleToFit,
                true
            );
        }
    }

    void DrawMainMenu()
    {
        float width = Mathf.Min(300f, Screen.width * 0.36f);
        float x = 28f;
        float y = Mathf.Max(150f, Screen.height * 0.28f);
        float gap = 14f;

        if (DrawImageButton(new Rect(x, y, width, 78f), newGameButton, "NEW GAME"))
        {
            startRequested = true;
        }

        y += 78f + gap;
        if (DrawImageButton(new Rect(x, y, width, 78f), optionsButton, "OPTIONS"))
        {
            optionsOpen = true;
        }

        y += 78f + gap;
        if (DrawImageButton(new Rect(x, y, width, 78f), quitButton, "QUIT"))
        {
            Application.Quit();
        }
    }

    void DrawOptions()
    {
        float width = Mathf.Min(620f, Screen.width - 48f);
        float height = Mathf.Min(390f, Screen.height - 70f);
        Rect panel = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );

        DrawColorRect(panel, new Color(0.03f, 0.03f, 0.035f, 0.94f));
        GUI.Box(panel, GUIContent.none);
        GUI.Label(new Rect(panel.x, panel.y + 22f, panel.width, 42f), "OPTIONS", headingStyle);

        GUI.Label(new Rect(panel.x + 45f, panel.y + 95f, 180f, 32f), "DIFFICULTY", labelStyle);
        string[] difficulties = { "EASY", "NORMAL", "HARD" };
        selectedDifficulty = GUI.SelectionGrid(
            new Rect(panel.x + 230f, panel.y + 90f, panel.width - 275f, 42f),
            selectedDifficulty,
            difficulties,
            3
        );

        GUI.Label(new Rect(panel.x + 45f, panel.y + 175f, 180f, 32f), "SOUND", labelStyle);
        soundVolume = GUI.HorizontalSlider(
            new Rect(panel.x + 230f, panel.y + 188f, panel.width - 275f, 28f),
            soundVolume,
            0f,
            1f
        );
        AudioListener.volume = soundVolume;

        GUI.Label(
            new Rect(panel.x + 230f, panel.y + 215f, panel.width - 275f, 28f),
            Mathf.RoundToInt(soundVolume * 100f) + "%",
            labelStyle
        );

        Rect okRect = new Rect(panel.xMax - 190f, panel.yMax - 82f, 150f, 54f);
        if (DrawImageButton(okRect, okButton, "OK"))
        {
            GameSettings.Difficulty = selectedDifficulty;
            GameSettings.SoundVolume = soundVolume;
            optionsOpen = false;
        }
    }

    bool DrawImageButton(Rect rect, Texture2D texture, string fallbackText)
    {
        return texture != null
            ? GUI.Button(rect, texture, imageButtonStyle)
            : GUI.Button(rect, fallbackText);
    }

    void EnsureStyles()
    {
        if (imageButtonStyle != null)
        {
            return;
        }

        imageButtonStyle = new GUIStyle(GUI.skin.button);
        imageButtonStyle.normal.background = null;
        imageButtonStyle.hover.background = null;
        imageButtonStyle.active.background = null;

        headingStyle = new GUIStyle(GUI.skin.label);
        headingStyle.alignment = TextAnchor.MiddleCenter;
        headingStyle.fontSize = 30;
        headingStyle.fontStyle = FontStyle.Bold;
        headingStyle.normal.textColor = new Color(0.85f, 0.08f, 0.06f);

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = 18;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;
    }

    static void DrawColorRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
