using UnityEngine;
using UnityEngine.SceneManagement;

// GOBLIN BLOCK — player health, damage flash, death/respawn.
// Put on the player (tag the player "Player").
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health = 100f;
    public CanvasGroup hurtFlash;     // optional red full-screen CanvasGroup
    public float hurtFadeSpeed = 2f;

    bool dead;
    float flash;

    void Update()
    {
        if (flash > 0f) flash -= Time.deltaTime * hurtFadeSpeed;
        if (hurtFlash) hurtFlash.alpha = Mathf.Clamp01(flash);
    }

    public void Damage(float amount)
    {
        if (dead) return;
        health -= amount;
        flash = 1f;
        if (health <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (dead) return;
        health = Mathf.Min(maxHealth, health + amount);
    }

    void Die()
    {
        dead = true;
        Cursor.lockState = CursorLockMode.None;
        // simplest restart; swap for a proper game-over UI
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsFull => health >= maxHealth;
}
