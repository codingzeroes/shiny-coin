using UnityEngine;

public class Health: MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    public float currentHealth { get; private set; }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Object {gameObject.name} has taken {damage} damage.");
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void Die()
    {
        // play death animation, disable player controls, activate pause screen, etc.
        Debug.Log("Player has died.");
        return;
    }
}
