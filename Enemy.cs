using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 50;
    public float fallSpeed = 2f; // How fast the enemy drops

    void Update()
    {
        // Move downwards every frame
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // If the enemy goes past the bottom of the screen, destroy it so it doesn't cause lag
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        // Flash red when hit
        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke("ResetColor", 0.1f);

        if (health <= 0)
        {
            Die();
        }
    }

    void ResetColor()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}