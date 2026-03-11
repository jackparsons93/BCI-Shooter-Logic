using UnityEngine;

public class BCIBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;

    // This is called by the Player the moment the bullet is fired
    public void PowerUp(float focusScore)
    {
        // 1. Calculate Damage: Base 10 damage + up to 40 extra damage based on focus
        damage = 10 + Mathf.RoundToInt(40f * focusScore);

        // 2. Calculate Size: Scale from 0.5x up to 2.0x based on focus
        float size = Mathf.Lerp(0.5f, 2.0f, focusScore);
        transform.localScale = new Vector3(size, size, 1f);

        // 3. Change Color: White when low focus, bright glowing yellow/orange when high
        GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, new Color(1f, 0.8f, 0f), focusScore);
    }

    void Update()
    {
        // Fly straight up
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Destroy the bullet if it flies off the top of the screen
        if (transform.position.y > 10f)
        {
            Destroy(gameObject);
        }
    }

    // When the bullet hits an enemy
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Destroy bullet after hitting
        }
    }
}