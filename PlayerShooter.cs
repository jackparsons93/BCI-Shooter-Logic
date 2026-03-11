using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    public BCIReceiver bciReceiver;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Player Settings")]
    public float moveSpeed = 7f;
    public float fireRate = 0.3f;
    public float fixedYPosition = -4f;

    private float nextFireTime = 0f;

    void Update()
    {
        // 1. Movement Logic
        float moveInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput -= 1f;
        }

        float newX = transform.position.x + (moveInput * moveSpeed * Time.deltaTime);
        float clampedX = Mathf.Clamp(newX, -8f, 8f);
        transform.position = new Vector3(clampedX, fixedYPosition, transform.position.z);

        // 2. Shooting Logic
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Safety checks
        if (bciReceiver == null || bulletPrefab == null || firePoint == null) return;

        // Weapon jams if signal is lost (signal_ok is now handled directly by the receiver)
        if (bciReceiver.signal_ok == 0) return;

        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // GRAB THE NEW VARIABLE HERE:
        float currentFocus = bciReceiver.performance_score;

        newBullet.GetComponent<BCIBullet>().PowerUp(currentFocus);
    }
}