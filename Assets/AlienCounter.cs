using UnityEngine;
using TMPro;

public class AlienCounter : MonoBehaviour
{
    private TextMeshProUGUI uiText;
    int remaining = 0;
    int lives = 3;

    // Awake runs before any Start, so the text is ready early
    void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
    }

    // FIX: no more Update(). Text only changes when something changes.
    public void SetRemaining(int count)
    {
        remaining = count;
        Refresh();
    }

    public void SetLives(int count)
    {
        lives = count;
        Refresh();
    }

    void Refresh()
    {
        uiText.text = "Remaining Aliens: " + remaining.ToString("#,0") + "     Lives: " + lives;
    }

    public void ShowMessage(string message)
    {
        uiText.text = message;
    }
}