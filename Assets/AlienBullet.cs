using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    // BALANCE: was .1, bullets come down faster now
    static float speed = .15f;

    // FIX: removed empty Start() and Update()

    void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.y -= speed;
        transform.position = position;
        if (position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        // did we hit the ship? (look on the object and its parents)
        Spaceship ship = coll.gameObject.GetComponentInParent<Spaceship>();
        if (ship != null)
        {
            ship.Hit();  // take a life, don't destroy the ship
        }
        else
        {
            Destroy(coll.gameObject);
        }
        Destroy(gameObject);
    }
}