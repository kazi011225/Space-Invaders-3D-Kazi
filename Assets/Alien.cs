using UnityEngine;

public class Alien : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject bulletPrefab;
    public AlienFleet fleet;

    // BALANCE: was .002, now aliens shoot 2x more
    static float bulletSpawnRate = .004f;
    public static float shotCooldown = .1f;
    float lastShot = 0;

    // FIX: removed empty Start() and Update(). Unity still calls them
    // every frame for every alien, so they just waste time.

    void FixedUpdate()
    {
        if (Random.value < bulletSpawnRate && Time.time - lastShot > shotCooldown)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.transform.position = transform.position;
            lastShot = Time.time;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        // FIX: fleet is already an AlienFleet, no need for GetComponent
        fleet.AlienDestroyed(gameObject);
        Destroy(gameObject);
        Destroy(coll.gameObject);
    }
}