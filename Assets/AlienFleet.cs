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

    [Header("Dynamic")]
    public List<GameObject> aliens = new List<GameObject>();

    float speed;            // + means moving right, - means moving left
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

        speed = startSpeed;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameObject alien = Instantiate(alienPrefab);
                alien.transform.position = new Vector3(2 * i - 12, 10 - 2 * j, 0);
                aliens.Add(alien);
                alien.GetComponent<Alien>().fleet = this;
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
        aliens.Remove(alien);
        alienCounter.SetRemaining(aliens.Count);

        if (aliens.Count == 0)
        {
            EndGame("YOU WIN!");
            return;
        }

        // REQUIRED: less aliens = faster aliens
        // ratio goes from 1 (all alive) down to 0 (almost all dead)
        float ratio = (aliens.Count - 1) / (float)(startCount - 1);
        float newSpeed = Mathf.Lerp(maxSpeed, startSpeed, ratio);

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