using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Editor tool (only runs inside Unity, not in the game).
// Adds two menu items:
//   Tools > Build Level UI       -> HUD, pause, win and game over screens in the open level
//   Tools > Build Main Menu Scene -> a title screen scene with Play and Quit
// Everything is plain on purpose: change the fonts, colors and text afterwards.
public static class UIBuilder
{
    const string HpFramePath = "Assets/Assets/GandalfHardcore Hp bar/GandalfHardcore Hp bar/Hp bar.png";
    const string HpFillPath = "Assets/Assets/GandalfHardcore Hp bar/GandalfHardcore Hp bar/red bar.png";
    const string MenuBackgroundPath = "Assets/Assets/GandalfHardcore FREE Platformer Assets/GandalfHardcore FREE Platformer Assets/GandalfHardcore Background layers/Normal BG/GandalfHardcore Background layers_layer 1.png";
    const string MenuScenePath = "Assets/Scenes/MainMenu.unity";

    static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.75f);
    static readonly Color ButtonColor = new Color(0.25f, 0.18f, 0.35f, 1f);

    // =====================================================================
    // LEVEL UI
    // =====================================================================

    [MenuItem("Tools/Build Level UI")]
    static void BuildLevelUI()
    {
        if (!TextMeshProReady())
        {
            return;
        }

        GameObject old = GameObject.Find("GameUI");
        if (old != null)
        {
            if (!EditorUtility.DisplayDialog("Build Level UI", "This scene already has a GameUI. Replace it?", "Replace", "Cancel"))
            {
                return;
            }
            Undo.DestroyObjectImmediate(old);
        }

        PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (player == null)
        {
            EditorUtility.DisplayDialog("Build Level UI", "Open your level scene first (the one with the player).", "OK");
            return;
        }

        GameManager gameManager = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        if (gameManager == null)
        {
            GameObject gmObject = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gmObject, "Build Level UI");
            gameManager = gmObject.AddComponent<GameManager>();
        }

        EnsureEventSystem();

        // ----- canvas -----
        GameObject canvasObject = MakeCanvas("GameUI");
        Transform canvas = canvasObject.transform;

        // ----- HUD (top left) -----
        HUD hud = canvasObject.AddComponent<HUD>();
        hud.player = player;

        GameObject hpGroup = MakeUI("HealthOrb", canvas);
        Place(hpGroup, new Vector2(0, 1), new Vector2(30, -30), new Vector2(348, 192));

        Sprite fillSprite = LoadUISprite(HpFillPath);
        Sprite frameSprite = LoadUISprite(HpFramePath);

        // red orb (drains from top to bottom)
        GameObject fillObject = MakeUI("HealthFill", hpGroup.transform);
        Place(fillObject, new Vector2(0, 1), new Vector2(6, -6), new Vector2(168, 162));
        Image fill = fillObject.AddComponent<Image>();
        fill.sprite = fillSprite;
        fill.color = fillSprite != null ? Color.white : Color.red;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Vertical;
        fill.fillOrigin = (int)Image.OriginVertical.Bottom;
        hud.healthFill = fill;

        // frame on top of the orb
        if (frameSprite != null)
        {
            GameObject frameObject = MakeUI("HealthFrame", hpGroup.transform);
            Stretch(frameObject);
            Image frame = frameObject.AddComponent<Image>();
            frame.sprite = frameSprite;
            frame.raycastTarget = false;
        }

        hud.herbText = MakeText(canvas, "HerbText", "Herbs 0 / 0", 48,
            new Vector2(0, 1), new Vector2(400, -60), new Vector2(400, 70), TextAlignmentOptions.Left);
        hud.livesText = MakeText(canvas, "LivesText", "x3", 48,
            new Vector2(0, 1), new Vector2(400, -130), new Vector2(400, 70), TextAlignmentOptions.Left);
        AddBoxBehind(hud.herbText);
        AddBoxBehind(hud.livesText);

        // ----- pause panel -----
        PauseMenu pauseMenu = canvasObject.AddComponent<PauseMenu>();
        GameObject pausePanel = MakePanel(canvas, "PausePanel");
        MakeText(pausePanel.transform, "Title", "Paused", 96,
            new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(900, 150), TextAlignmentOptions.Center);
        UnityEventTools.AddPersistentListener(MakeButton(pausePanel.transform, "Resume", new Vector2(0, 60)).onClick, new UnityAction(pauseMenu.Resume));
        UnityEventTools.AddPersistentListener(MakeButton(pausePanel.transform, "Restart", new Vector2(0, -70)).onClick, new UnityAction(pauseMenu.Restart));
        UnityEventTools.AddPersistentListener(MakeButton(pausePanel.transform, "Quit", new Vector2(0, -200)).onClick, new UnityAction(pauseMenu.QuitToMenu));
        pauseMenu.pausePanel = pausePanel;
        AddVolumeSlider(pausePanel.transform);

        // ----- win panel -----
        GameObject winPanel = MakePanel(canvas, "WinPanel");
        MakeText(winPanel.transform, "Title", "Level Complete!", 110,
            new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1400, 160), TextAlignmentOptions.Center);
        TMP_Text winHerbs = MakeText(winPanel.transform, "HerbsText", "Herbs: 0 / 0", 60,
            new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(900, 100), TextAlignmentOptions.Center);
        UnityEventTools.AddPersistentListener(MakeButton(winPanel.transform, "Play Again", new Vector2(0, -70)).onClick, new UnityAction(gameManager.RestartLevel));
        UnityEventTools.AddPersistentListener(MakeButton(winPanel.transform, "Main Menu", new Vector2(0, -200)).onClick, new UnityAction(gameManager.LoadMainMenu));

        // ----- game over panel -----
        GameObject gameOverPanel = MakePanel(canvas, "GameOverPanel");
        MakeText(gameOverPanel.transform, "Title", "Game Over", 110,
            new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(1200, 160), TextAlignmentOptions.Center);
        UnityEventTools.AddPersistentListener(MakeButton(gameOverPanel.transform, "Try Again", new Vector2(0, -20)).onClick, new UnityAction(gameManager.RestartLevel));
        UnityEventTools.AddPersistentListener(MakeButton(gameOverPanel.transform, "Main Menu", new Vector2(0, -150)).onClick, new UnityAction(gameManager.LoadMainMenu));

        // ----- connect GameManager -----
        Undo.RecordObject(gameManager, "Build Level UI");
        gameManager.winScreen = winPanel;
        gameManager.gameOverScreen = gameOverPanel;
        gameManager.winHerbsText = winHerbs;
        EditorUtility.SetDirty(gameManager);

        // hide the panels in the editor (the scripts also hide them when the game starts)
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(canvasObject.scene);
        Selection.activeGameObject = canvasObject;
        EditorUtility.DisplayDialog("Build Level UI", "Done! Press Ctrl+S to save the scene.\n\nTo edit a hidden panel, tick it on at the top of the Inspector, then tick it off again.", "OK");
    }

    // =====================================================================
    // AUDIO
    // =====================================================================

    const string AudioFolder = "Assets/Audio/";

    // adds an AudioManager with all the sounds from Assets/Audio to the open scene
    [MenuItem("Tools/Setup Audio In This Scene")]
    static void SetupAudio()
    {
        AudioManager manager = Object.FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
        if (manager == null)
        {
            GameObject go = new GameObject("AudioManager");
            Undo.RegisterCreatedObjectUndo(go, "Setup Audio");
            manager = go.AddComponent<AudioManager>();
        }

        Undo.RecordObject(manager, "Setup Audio");
        manager.jump = Clip("Jump");
        manager.land = Clip("Land");
        manager.collect = Clip("Collect");
        manager.hurt = Clip("Hurt");
        manager.swing = Clip("Swing");
        manager.enemyHit = Clip("EnemyHit");
        manager.win = Clip("Win");
        manager.checkpoint = Clip("Checkpoint");
        manager.death = Clip("Death");
        manager.music = Clip("ForestMusic");
        EditorUtility.SetDirty(manager);

        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        EditorUtility.DisplayDialog("Setup Audio", "AudioManager is ready in this scene with all the sounds.\nPress Ctrl+S.\n\nDo this in BOTH the MainMenu and the level scene.", "OK");
    }

    static AudioClip Clip(string name)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + name + ".wav");
        if (clip == null)
        {
            Debug.LogWarning("Setup Audio: missing " + AudioFolder + name + ".wav");
        }
        return clip;
    }

    // adds a volume slider to the pause panel that is already in the scene
    [MenuItem("Tools/Add Volume Slider To Pause Menu")]
    static void AddVolumeSliderMenu()
    {
        PauseMenu pause = Object.FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include);
        if (pause == null || pause.pausePanel == null)
        {
            EditorUtility.DisplayDialog("Volume slider", "No pause menu found. Open your level scene first.", "OK");
            return;
        }
        if (pause.pausePanel.transform.Find("VolumeSlider") != null)
        {
            EditorUtility.DisplayDialog("Volume slider", "The pause menu already has a volume slider.", "OK");
            return;
        }

        AddVolumeSlider(pause.pausePanel.transform);
        EditorSceneManager.MarkSceneDirty(pause.gameObject.scene);
        EditorUtility.DisplayDialog("Volume slider", "Added a volume slider to the pause menu. Press Ctrl+S.", "OK");
    }

    static void AddVolumeSlider(Transform panel)
    {
        MakeText(panel, "VolumeLabel", "Volume   (M = mute)", 40,
            new Vector2(0.5f, 0.5f), new Vector2(0, -300), new Vector2(600, 60), TextAlignmentOptions.Center);

        GameObject sliderObject = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderObject.name = "VolumeSlider";
        sliderObject.transform.SetParent(panel, false);
        Undo.RegisterCreatedObjectUndo(sliderObject, "Add volume slider");
        Place(sliderObject, new Vector2(0.5f, 0.5f), new Vector2(0, -370), new Vector2(500, 40));

        // make the bar easy to see on the dark panel
        Image background = sliderObject.transform.Find("Background").GetComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.25f);
        Image fill = sliderObject.transform.Find("Fill Area/Fill").GetComponent<Image>();
        fill.color = new Color(1f, 0.85f, 0.4f);
        Image handle = sliderObject.transform.Find("Handle Slide Area/Handle").GetComponent<Image>();
        handle.color = Color.white;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.value = 1f;
        sliderObject.AddComponent<VolumeSlider>();
    }

    // adds dark boxes behind the herb and lives text of the UI that is already in the scene
    // (does not touch anything else)
    [MenuItem("Tools/Add Dark Boxes Behind HUD Text")]
    static void AddHudBoxes()
    {
        HUD hud = Object.FindFirstObjectByType<HUD>(FindObjectsInactive.Include);
        if (hud == null)
        {
            EditorUtility.DisplayDialog("HUD boxes", "No HUD found. Open your level scene first.", "OK");
            return;
        }

        int added = 0;
        if (hud.herbText != null && AddBoxBehind(hud.herbText))
        {
            added++;
        }
        if (hud.livesText != null && AddBoxBehind(hud.livesText))
        {
            added++;
        }

        EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
        EditorUtility.DisplayDialog("HUD boxes", "Added " + added + " dark boxes. Press Ctrl+S to save.\nTo change how dark they are, select a box and change the Image color's alpha (A).", "OK");
    }

    // a dark box with the same position as the text, drawn just behind it
    static bool AddBoxBehind(TMP_Text text)
    {
        Transform parent = text.transform.parent;
        string boxName = text.name + "Box";
        if (parent.Find(boxName) != null)
        {
            return false; // already has one
        }

        GameObject box = MakeUI(boxName, parent);
        Undo.RegisterCreatedObjectUndo(box, "Add HUD box");

        RectTransform textRect = text.rectTransform;
        RectTransform boxRect = box.GetComponent<RectTransform>();
        boxRect.anchorMin = textRect.anchorMin;
        boxRect.anchorMax = textRect.anchorMax;
        boxRect.pivot = textRect.pivot;
        boxRect.anchoredPosition = textRect.anchoredPosition + new Vector2(-15f, 5f);
        boxRect.sizeDelta = new Vector2(320f, textRect.sizeDelta.y + 10f);

        Image image = box.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.6f);
        image.raycastTarget = false;

        // put the box right before the text so the text is drawn on top
        box.transform.SetSiblingIndex(text.transform.GetSiblingIndex());
        return true;
    }

    // =====================================================================
    // MAIN MENU SCENE
    // =====================================================================

    [MenuItem("Tools/Build Main Menu Scene")]
    static void BuildMainMenuScene()
    {
        if (!TextMeshProReady())
        {
            return;
        }

        // the level is the scene that is open right now
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
        var levelScene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(levelScene.path) || levelScene.path == MenuScenePath)
        {
            EditorUtility.DisplayDialog("Build Main Menu", "Open your level scene first, then run this again.", "OK");
            return;
        }
        string levelPath = levelScene.path;
        string levelName = levelScene.name;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuScenePath) != null)
        {
            if (!EditorUtility.DisplayDialog("Build Main Menu", "MainMenu scene already exists. Replace it?", "Replace", "Cancel"))
            {
                return;
            }
        }

        var menuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EnsureEventSystem();

        GameObject canvasObject = MakeCanvas("MenuUI");
        Transform canvas = canvasObject.transform;

        // background picture
        Sprite background = AssetDatabase.LoadAllAssetsAtPath(MenuBackgroundPath).OfType<Sprite>().FirstOrDefault();
        GameObject bgObject = MakeUI("Background", canvas);
        Stretch(bgObject);
        Image bgImage = bgObject.AddComponent<Image>();
        if (background != null)
        {
            bgImage.sprite = background;
        }
        else
        {
            bgImage.color = new Color(0.15f, 0.25f, 0.2f);
        }

        MainMenu menu = canvasObject.AddComponent<MainMenu>();
        menu.levelScene = levelName;

        MakeText(canvas, "Title", "Your Game Title", 130,
            new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1600, 200), TextAlignmentOptions.Center);
        UnityEventTools.AddPersistentListener(MakeButton(canvas, "Play", new Vector2(0, -20)).onClick, new UnityAction(menu.PlayGame));
        UnityEventTools.AddPersistentListener(MakeButton(canvas, "Quit", new Vector2(0, -150)).onClick, new UnityAction(menu.QuitGame));

        EditorSceneManager.SaveScene(menuScene, MenuScenePath);

        // build list: MainMenu first, then the level, then anything else that was there
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(levelPath, true)
        };
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (s.path != MenuScenePath && s.path != levelPath)
            {
                scenes.Add(s);
            }
        }
        EditorBuildSettings.scenes = scenes.ToArray();

        EditorUtility.DisplayDialog("Build Main Menu", "Done! The MainMenu scene is open.\nChange \"Your Game Title\" to your game's name, then Ctrl+S.\n\nMainMenu and " + levelName + " were added to the build list.", "OK");
    }

    // =====================================================================
    // HELPERS
    // =====================================================================

    static bool TextMeshProReady()
    {
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            EditorUtility.DisplayDialog("TextMeshPro needed",
                "First import TextMeshPro:\nWindow > TextMeshPro > Import TMP Essential Resources\n\nThen run this again.", "OK");
            return false;
        }
        return true;
    }

    static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (eventSystem == null)
        {
            GameObject esObject = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(esObject, "Build UI");
            eventSystem = esObject.AddComponent<EventSystem>();
        }

        // this project uses the new Input System, so the old input module doesn't work
        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            Undo.DestroyObjectImmediate(oldModule);
        }
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
        }
    }

    static GameObject MakeCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Build UI");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvasObject;
    }

    static GameObject MakeUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    // anchor + pivot at the same corner, then move by "position"
    static void Place(GameObject go, Vector2 anchor, Vector2 position, Vector2 size)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    static void Stretch(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject MakePanel(Transform parent, string name)
    {
        GameObject panel = MakeUI(name, parent);
        Stretch(panel);
        Image image = panel.AddComponent<Image>();
        image.color = PanelColor;
        return panel;
    }

    static TMP_Text MakeText(Transform parent, string name, string text, float fontSize,
        Vector2 anchor, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
    {
        GameObject go = MakeUI(name, parent);
        Place(go, anchor, position, size);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button MakeButton(Transform parent, string label, Vector2 position)
    {
        GameObject go = MakeUI(label + "Button", parent);
        Place(go, new Vector2(0.5f, 0.5f), position, new Vector2(420, 100));

        Image image = go.AddComponent<Image>();
        image.color = ButtonColor;
        Button button = go.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        button.colors = colors;

        MakeText(go.transform, "Text", label, 52,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 100), TextAlignmentOptions.Center);

        return button;
    }

    // loads a picture as a UI sprite. If it was imported in a way the UI can't use,
    // it is switched to a single pixel-art sprite first.
    static Sprite LoadUISprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (sprite != null)
        {
            return sprite;
        }

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("UI Builder: picture not found at " + path);
            return null;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
