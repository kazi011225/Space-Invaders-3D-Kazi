using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

// Shows the start screen, the level clear screen, and the end screen.
// AlienFleet adds this script by itself, so no setup in Unity is needed.
public class GameScreens : MonoBehaviour
{
    // LEVELS: static so it stays the same when the scene reloads for the next level
    public static int level = 1;
    public const int maxLevel = 3;

    // This project keeps static values between Play sessions in the editor.
    // So we reset the level every time Play is pressed.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetLevel()
    {
        level = 1;
    }

    GameObject panel;           // dark box that covers the screen
    TextMeshProUGUI title;      // big text
    TextMeshProUGUI info;       // small text under it

    bool waitingToStart = true;
    bool gameEnded = false;
    bool canGoNext = false;     // true when a level is cleared
    float endTime;              // when the end screen showed up

    void Awake()
    {
        BuildUI();
        ShowStart();
    }

    void Update()
    {
        // start screen: press SPACE to begin
        if (waitingToStart && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            waitingToStart = false;
            panel.SetActive(false);
            Time.timeScale = 1;   // unfreeze the game
        }
        // end screen: wait half a second so a held SPACE doesn't skip it
        else if (gameEnded && Time.unscaledTime - endTime > .5f)
        {
            if (canGoNext && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                level++;          // go to the next level
                Reload();
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                level = 1;        // start over from level 1
                Reload();
            }
        }
    }

    void Reload()
    {
        Time.timeScale = 1;   // must unfreeze before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ShowStart()
    {
        Time.timeScale = 0;   // freeze everything until the player is ready
        title.color = Color.white;

        if (level == 1)
        {
            title.text = "SPACE INVADERS";
            info.text = "Arrow keys to move, SPACE to shoot. Beat " + maxLevel + " levels to win!\nPress SPACE to start";
        }
        else
        {
            title.text = "LEVEL " + level;
            info.text = "More aliens, and they are faster!\nPress SPACE to start";
        }
        panel.SetActive(true);
    }

    // AlienFleet calls this when the game or level ends
    public void ShowEnd(string message, bool won, int destroyed, int total)
    {
        gameEnded = true;
        endTime = Time.unscaledTime;
        string score = "Aliens destroyed: " + destroyed + " / " + total;

        if (won && level < maxLevel)
        {
            // level cleared, more levels left
            canGoNext = true;
            title.text = "LEVEL " + level + " CLEAR!";
            title.color = Color.green;
            info.text = score + "\nPress SPACE for level " + (level + 1) + ", or R to start over";
        }
        else if (won)
        {
            // beat the last level
            title.text = "YOU WIN!";
            title.color = Color.green;
            info.text = "You beat all " + maxLevel + " levels!\nPress R to play again";
        }
        else
        {
            title.text = message;
            title.color = Color.red;
            info.text = "You made it to level " + level + ". " + score + "\nPress R to play again";
        }
        panel.SetActive(true);
    }

    // ---------- build the UI with code ----------

    void BuildUI()
    {
        // new canvas that draws on top of everything else
        GameObject canvasObj = new GameObject("ScreensCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        // makes the UI the same size on any screen
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // dark see-through box over the whole screen
        panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(canvasObj.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.75f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;   // stretch to all 4 corners
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        title = MakeText("Title", 120, new Vector2(0, 100));
        info = MakeText("Info", 48, new Vector2(0, -100));
    }

    TextMeshProUGUI MakeText(string name, float size, Vector2 offset)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(panel.transform, false);

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        RectTransform rt = text.rectTransform;
        rt.sizeDelta = new Vector2(1700, size * 3);
        rt.anchoredPosition = offset;
        return text;
    }
}