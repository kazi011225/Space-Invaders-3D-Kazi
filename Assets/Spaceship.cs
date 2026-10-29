using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject dartPrefab;
    public float speed = 10f;
    public float edgePadding = 1f;      // space between ship and screen edge
    public int lives = 3;
    public float safeTime = 1.5f;       // can't get hit again right after a hit

    // BALANCE: was .05, no more spamming the space bar
    public static float shotCooldown = .4f;
    float lastShot = 0;
    float lastHit = -100;

    AlienCounter counter;
    AlienFleet fleet;

    void Start()
    {
        counter = FindFirstObjectByType<AlienCounter>();
        fleet = FindFirstObjectByType<AlienFleet>();
        counter.SetLives(lives);
    }

    void Update()
    {
        // game is frozen (win or lose), so no moving or shooting
        if (Time.timeScale == 0) return;

        // left = -1, right = +1, both or none = 0
        float move = 0;
        if (Keyboard.current.rightArrowKey.isPressed) move += 1;
        if (Keyboard.current.leftArrowKey.isPressed) move -= 1;

        Vector3 position = transform.position;
        position.x += move * speed * Time.deltaTime;

        // REQUIRED: keep ship on screen
        Camera cam = Camera.main;
        float halfWidth = cam.orthographicSize * cam.aspect;
        float minX = cam.transform.position.x - halfWidth + edgePadding;
        float maxX = cam.transform.position.x + halfWidth - edgePadding;
        position.x = Mathf.Clamp(position.x, minX, maxX);

        transform.position = position;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time - lastShot > shotCooldown)
        {
            GameObject bullet = Instantiate(dartPrefab);
            bullet.transform.position = transform.position;
            lastShot = Time.time;
        }
    }

    // AlienBullet calls this when it hits the ship
    public void Hit()
    {
        // still safe from the last hit, ignore this one
        if (Time.time - lastHit < safeTime) return;
        lastHit = Time.time;

        lives--;
        counter.SetLives(lives);

        if (lives <= 0)
        {
            gameObject.SetActive(false);  // hide the ship
            fleet.EndGame("GAME OVER");
        }
    }
}