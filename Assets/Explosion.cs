using UnityEngine;
using UnityEngine.Rendering;

// A little "boom": small spheres fly out from a point and shrink away.
// No prefab needed, it is all made with code.
public class Explosion : MonoBehaviour
{
    Vector3 velocity;
    Vector3 startScale;
    float life;      // how long this piece lives
    float age = 0;   // how long it has lived so far

    // Call this from anywhere: Explosion.Spawn(position, color);
    public static void Spawn(Vector3 position, Color color, int pieces = 12, float size = 1f)
    {
        for (int i = 0; i < pieces; i++)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piece.name = "ExplosionPiece";

            // FIX: pieces must never hit aliens, or they start a chain reaction.
            // We are inside a collision here, and Unity does not allow
            // DestroyImmediate during a collision. So we turn the collider
            // off right away (allowed), and delete it at the end of the frame.
            Collider col = piece.GetComponent<Collider>();
            col.enabled = false;
            Destroy(col);

            piece.transform.position = position;
            piece.transform.localScale = Vector3.one * Random.Range(.15f, .35f) * size;

            Renderer r = piece.GetComponent<Renderer>();
            r.material.color = color;
            r.shadowCastingMode = ShadowCastingMode.Off;

            // send each piece in a random direction
            Explosion e = piece.AddComponent<Explosion>();
            Vector2 dir = Random.insideUnitCircle.normalized;
            e.velocity = new Vector3(dir.x, dir.y, 0) * Random.Range(4f, 9f) * size;
            e.startScale = piece.transform.localScale;
            e.life = Random.Range(.4f, .7f);
        }
    }

    void Update()
    {
        // unscaled time, so the boom still finishes when the game freezes at the end
        float dt = Time.unscaledDeltaTime;
        age += dt;

        transform.position += velocity * dt;
        velocity *= Mathf.Pow(.05f, dt);  // slow down over time

        // shrink to nothing, then delete
        float left = 1 - age / life;
        if (left <= 0)
        {
            Destroy(gameObject);
            return;
        }
        transform.localScale = startScale * left;
    }
}