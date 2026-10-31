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
    public float blinkSpeed = 10f;      // how fast the ship blinks when safe

    // BALANCE: was .05, no more spamming the space bar
    public static float shotCooldown = .4f;
    float lastShot = 0;
    float lastHit = -100;

    AlienCounter counter;
    AlienFleet fleet;
    Renderer[] parts;                   // all the visible pieces of the ship

    void Start()
    {
        counter = FindFirstObjectByType<AlienCounter>();
        fleet = FindFirstObjectByType<AlienFleet>();
        counter.SetLives(lives);

        // the ship is made of a few pieces, get all of them for blinking
        parts = GetComponentsInChildren<Renderer>();

        // FIX: bullets were pushing and turning the ship when it got hit.
        // Kinematic = physics can't move it, only our code moves it.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Update()
    {
        // game is frozen (win or lose), so no moving or shooting
        if (Time.timeScale == 0) return;

        // blink while safe, so the player knows hits don't count right now
        bool safe = Time.time - lastHit < safeTime;
        bool visible = !safe || Mathf.FloorToInt(Time.time * blinkSpeed) % 2 == 0;
        foreach (Renderer part in parts)
        {
            part.enabled = visible;
        }

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
        transform.rotation = Quaternion.identity;  // always stay straight

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

        // ANIMATION: orange boom, a big one on the last life
        Color orange = new Color(1f, .6f, .1f);
        if (lives <= 0)
        {
            Explosion.Spawn(transform.position, orange, 30, 1.6f);
            gameObject.SetActive(false);  // hide the ship
            fleet.EndGame("GAME OVER");
        }
        else
        {
            Explosion.Spawn(transform.position, orange, 10, .8f);
        }
    }
}