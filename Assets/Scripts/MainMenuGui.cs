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
    Texture2D optionButtonBackground;
    Texture2D optionButtonPressed;
    Texture2D sliderBackground;
    Texture2D sliderThumb;
    Texture2D optionsTitle;
    Texture2D difficultyTitle;
    Texture2D soundTitle;
    Texture2D easyText;
    Texture2D normalText;
    Texture2D hardText;
    AudioSource menuAudio;
    AudioClip menuClick;
    GUIStyle imageButtonStyle;
    GUIStyle optionButtonStyle;
    GUIStyle sliderStyle;
    GUIStyle sliderThumbStyle;
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
        optionButtonBackground = Resources.Load<Texture2D>("Gui/ButtonBg");
        optionButtonPressed = Resources.Load<Texture2D>("Gui/ButtonBgPressed");
        sliderBackground = Resources.Load<Texture2D>("Gui/SliderBg");
        sliderThumb = Resources.Load<Texture2D>("Gui/SliderThumb");
        optionsTitle = Resources.Load<Texture2D>("Gui/TitleOptions");
        difficultyTitle = Resources.Load<Texture2D>("Gui/TitleDifficulty");
        soundTitle = Resources.Load<Texture2D>("Gui/TitleSound");
        easyText = Resources.Load<Texture2D>("Gui/TextEasy");
        normalText = Resources.Load<Texture2D>("Gui/TextNormal");
        hardText = Resources.Load<Texture2D>("Gui/TextHard");

        menuClick = Resources.Load<AudioClip>("Audio/MenuClick");
        menuAudio = gameObject.AddComponent<AudioSource>();
        menuAudio.playOnAwake = false;
        menuAudio.spatialBlend = 0f;

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

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ApplyDifficulty(GameSettings.Difficulty);
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
            PlayMenuClick();
            startRequested = true;
        }

        y += 78f + gap;
        if (DrawImageButton(new Rect(x, y, width, 78f), optionsButton, "OPTIONS"))
        {
            PlayMenuClick();
            optionsOpen = true;
        }

        y += 78f + gap;
        if (DrawImageButton(new Rect(x, y, width, 78f), quitButton, "QUIT"))
        {
            PlayMenuClick();
            Application.Quit();
        }
    }

    void DrawOptions()
    {
        float width = Mathf.Min(760f, Screen.width - 36f);
        float height = Mathf.Min(510f, Screen.height - 36f);
        Rect panel = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );

        DrawColorRect(panel, new Color(0.03f, 0.03f, 0.035f, 0.94f));
        GUI.Box(panel, GUIContent.none);

        Rect titleRect = new Rect(panel.x + panel.width * 0.2f, panel.y + 18f, panel.width * 0.6f, 74f);
        DrawTextureOrLabel(titleRect, optionsTitle, "OPTIONS", headingStyle);

        Rect difficultyTitleRect = new Rect(panel.x + 38f, panel.y + 105f, 210f, 55f);
        DrawTextureOrLabel(difficultyTitleRect, difficultyTitle, "DIFFICULTY", labelStyle);

        float buttonAreaX = panel.x + 38f;
        float buttonAreaWidth = panel.width - 76f;
        float buttonGap = 14f;
        float difficultyButtonWidth = (buttonAreaWidth - buttonGap * 2f) / 3f;
        float difficultyButtonHeight = Mathf.Clamp(difficultyButtonWidth * 0.32f, 58f, 82f);
        float difficultyY = panel.y + 165f;
        Texture2D[] difficultyLabels = { easyText, normalText, hardText };
        string[] difficultyFallbacks = { "EASY", "NORMAL", "HARD" };

        for (int i = 0; i < difficultyLabels.Length; i++)
        {
            Rect buttonRect = new Rect(
                buttonAreaX + i * (difficultyButtonWidth + buttonGap),
                difficultyY,
                difficultyButtonWidth,
                difficultyButtonHeight
            );

            if (selectedDifficulty == i)
            {
                DrawBorder(ExpandRect(buttonRect, 4f), new Color(1f, 0.92f, 0.12f), 3f);
            }

            if (GUI.Button(buttonRect, GUIContent.none, optionButtonStyle))
            {
                selectedDifficulty = i;
                PlayMenuClick();
            }

            Rect textRect = new Rect(
                buttonRect.x + buttonRect.width * 0.16f,
                buttonRect.y + buttonRect.height * 0.18f,
                buttonRect.width * 0.68f,
                buttonRect.height * 0.64f
            );
            DrawTextureOrLabel(textRect, difficultyLabels[i], difficultyFallbacks[i], labelStyle);
        }

        Rect soundTitleRect = new Rect(panel.x + 38f, panel.y + 285f, 170f, 54f);
        DrawTextureOrLabel(soundTitleRect, soundTitle, "SOUND", labelStyle);

        float sliderX = panel.x + 220f;
        float sliderWidth = panel.width - 265f;
        soundVolume = GUI.HorizontalSlider(
            new Rect(sliderX, panel.y + 302f, sliderWidth, 32f),
            soundVolume,
            0f,
            1f,
            sliderStyle,
            sliderThumbStyle
        );
        AudioListener.volume = soundVolume;

        GUI.Label(
            new Rect(sliderX, panel.y + 342f, sliderWidth, 28f),
            Mathf.RoundToInt(soundVolume * 100f) + "%",
            labelStyle
        );

        float okWidth = Mathf.Min(190f, panel.width * 0.3f);
        Rect okRect = new Rect(
            panel.x + (panel.width - okWidth) * 0.5f,
            panel.yMax - 82f,
            okWidth,
            56f
        );
        if (DrawImageButton(okRect, okButton, "OK"))
        {
            PlayMenuClick();
            GameSettings.Difficulty = selectedDifficulty;
            GameSettings.SoundVolume = soundVolume;
            optionsOpen = false;
        }
    }

    void PlayMenuClick()
    {
        if (menuAudio != null && menuClick != null)
        {
            menuAudio.PlayOneShot(menuClick, 0.7f);
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

        optionButtonStyle = new GUIStyle(GUI.skin.button);
        optionButtonStyle.normal.background = optionButtonBackground;
        optionButtonStyle.hover.background = optionButtonPressed != null
            ? optionButtonPressed
            : optionButtonBackground;
        optionButtonStyle.active.background = optionButtonPressed != null
            ? optionButtonPressed
            : optionButtonBackground;
        optionButtonStyle.border = new RectOffset(12, 12, 12, 12);

        sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
        sliderStyle.normal.background = sliderBackground;
        sliderStyle.fixedHeight = 18f;
        sliderStyle.margin = new RectOffset(0, 0, 7, 7);

        sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        sliderThumbStyle.normal.background = sliderThumb;
        sliderThumbStyle.hover.background = sliderThumb;
        sliderThumbStyle.active.background = sliderThumb;
        sliderThumbStyle.fixedWidth = 34f;
        sliderThumbStyle.fixedHeight = 34f;

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

    static void DrawTextureOrLabel(Rect rect, Texture2D texture, string fallback, GUIStyle style)
    {
        if (texture != null)
        {
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
        }
        else
        {
            GUI.Label(rect, fallback, style);
        }
    }

    static Rect ExpandRect(Rect rect, float amount)
    {
        return new Rect(
            rect.x - amount,
            rect.y - amount,
            rect.width + amount * 2f,
            rect.height + amount * 2f
        );
    }

    static void DrawBorder(Rect rect, Color color, float thickness)
    {
        DrawColorRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        DrawColorRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        DrawColorRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        DrawColorRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    static void DrawColorRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
