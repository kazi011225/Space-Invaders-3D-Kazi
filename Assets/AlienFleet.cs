using UnityEngine;
using System.Collections.Generic;

public class AlienFleet : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject alienPrefab;
    public AlienCounter alienCounter;

    [Header("Speed")]
    public float startSpeed = .12f;  // BALANCE: speed when all aliens are alive
    public float maxSpeed = .45f;    // BALANCE: speed when only 1 alien is left
    public float dropSpeed = .1f;    // how fast they go down at the edge

    [Header("Levels")]
    public int columns = 10;              // aliens in each row
    public float levelSpeedBoost = .2f;   // each level is 20% faster

    // alien color for each level (level 1 keeps the normal green)
    static readonly Color[] levelColors =
    {
        Color.green,
        new Color(1f, .8f, .1f),    // level 2: yellow
        new Color(1f, .25f, .25f)   // level 3: red
    };

    [Header("Dynamic")]
    public List<GameObject> aliens = new List<GameObject>();

    float speed;            // + means moving right, - means moving left
    float levelStartSpeed;  // start speed after the level boost
    float levelMaxSpeed;    // max speed after the level boost
    int startCount;         // how many aliens we started with
    float leftBound = -16;
    float rightBound = 16;
    float downTime = .1f;
    bool movingDown = false;
    bool gameOver = false;
    float loseLine;         // if any alien gets this low, player can't win
    GameScreens screens;

    void Start()
    {
        // add the start/end screens (this also pauses the game until SPACE)
        screens = gameObject.AddComponent<GameScreens>();

        // LEVELS: more rows and more speed on each level
        int level = GameScreens.level;
        int rows = 2 + level;                               // 3, 4, 5 rows
        float boost = 1 + levelSpeedBoost * (level - 1);    // 1.0, 1.2, 1.4
        levelStartSpeed = startSpeed * boost;
        levelMaxSpeed = maxSpeed * boost;
        speed = levelStartSpeed;

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                GameObject alien = Instantiate(alienPrefab);
                alien.transform.position = new Vector3(2 * i - 12, 10 - 2 * j, 0);
                aliens.Add(alien);
                alien.GetComponent<Alien>().fleet = this;

                // LEVELS: new color so each level looks different
                if (level > 1)
                {
                    int c = Mathf.Min(level, levelColors.Length) - 1;
                    alien.GetComponent<Renderer>().material.color = levelColors[c];
                }
            }
        }
        startCount = aliens.Count;

        // lose line is a little above the ship
        Spaceship ship = FindFirstObjectByType<Spaceship>();
        if (ship != null)
        {
            loseLine = ship.transform.position.y + 1f;
        }
        else
        {
            loseLine = -6f; // backup value if no ship found
        }

        // FIX: show the count one time here, not every frame in Update
        alienCounter.SetRemaining(aliens.Count);
    }

    // Alien calls this when it gets hit
    public void AlienDestroyed(GameObject alien)
    {
        // if 2 darts hit the same alien at once, only count it one time
        if (!aliens.Remove(alien)) return;
        alienCounter.SetRemaining(aliens.Count);

        if (aliens.Count == 0)
        {
            EndGame("YOU WIN!");
            return;
        }

        // REQUIRED: less aliens = faster aliens
        // ratio goes from 1 (all alive) down to 0 (almost all dead)
        float ratio = startCount > 1 ? (aliens.Count - 1) / (float)(startCount - 1) : 0;
        float newSpeed = Mathf.Lerp(levelMaxSpeed, levelStartSpeed, ratio);

        // keep the same direction, only change how fast
        speed = Mathf.Sign(speed) * newSpeed;
    }

    void FixedUpdate()
    {
        if (gameOver) return;

        // FIX: start left/right from huge numbers, not 0.
        // Before, if all aliens were on one side of 0, the edge check was wrong.
        float left = float.MaxValue;
        float right = float.MinValue;
        float lowest = float.MaxValue;

        // move all aliens AND find the edges in the same loop
        // (before it was 2 loops over the same list)
        foreach (GameObject alien in aliens)
        {
            Vector3 position = alien.transform.position;
            if (!movingDown)
            {
                position.x += speed;
            }
            else
            {
                position.y -= dropSpeed;
            }
            alien.transform.position = position;

            if (position.x < left) left = position.x;
            if (position.x > right) right = position.x;
            if (position.y < lowest) lowest = position.y;
        }

        // REQUIRED: aliens are too low, game can't be won anymore
        if (lowest <= loseLine)
        {
            EndGame("GAME OVER");
            return;
        }

        // change direction at the edges
        if ((speed > 0 && right >= rightBound) || (speed < 0 && left <= leftBound))
        {
            speed = -speed;
            movingDown = true;
            // FIX: nameof() instead of a string, so renaming won't break it
            Invoke(nameof(StartSidewaysMotion), downTime);
        }
    }

    void StartSidewaysMotion()
    {
        movingDown = false;
    }

    public void EndGame(string message)
    {
        if (gameOver) return;   // only end the game one time
        gameOver = true;
        Time.timeScale = 0;     // freeze the game

        bool won = aliens.Count == 0;
        screens.ShowEnd(message, won, startCount - aliens.Count, startCount);
    }
}