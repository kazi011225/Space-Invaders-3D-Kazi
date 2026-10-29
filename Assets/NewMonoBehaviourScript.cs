using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

// Shows the start screen and the end screen.
// AlienFleet adds this script by itself, so no setup in Unity is needed.
public class GameScreens : MonoBehaviour
{
    GameObject panel;           // dark box that covers the screen
    TextMeshProUGUI title;      // big text
    TextMeshProUGUI info;       // small text under it

    bool waitingToStart = true;
    bool gameEnded = false;

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
        // end screen: press R to play again
        else if (gameEnded && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1;   // must unfreeze before reloading
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void ShowStart()
    {
        Time.timeScale = 0;   // freeze everything until the player is ready
        title.text = "SPACE INVADERS";
        title.color = Color.white;
        info.text = "Arrow keys to move, SPACE to shoot\nPress SPACE to start";
        panel.SetActive(true);
    }

    // AlienFleet calls this when the game ends
    public void ShowEnd(string message, bool won, int destroyed, int total)
    {
        gameEnded = true;
        title.text = message;
        title.color = won ? Color.green : Color.red;
        info.text = "Aliens destroyed: " + destroyed + " / " + total + "\nPress R to play again";
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
        rt.sizeDelta = new Vector2(1600, size * 3);
        rt.anchoredPosition = offset;
        return text;
    }
}