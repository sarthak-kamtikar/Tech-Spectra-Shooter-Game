using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthChangedEvent : UnityEvent<float> { } // normalized 0..1

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    // normalized value (0..1) - UI listens to this
    public HealthChangedEvent onHealthChanged;

    // called when player reaches zero
    public UnityEvent onDied;

    // extra helpers
    public float healthDelta { get; private set; } = 0f;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;

        if (onHealthChanged == null) onHealthChanged = new HealthChangedEvent();
        if (onDied == null) onDied = new UnityEvent();

        Debug.Log($"[PlayerHealth] Awake() on '{gameObject.name}' max:{maxHealth}");

        // initial broadcast so UI shows correct value at startup
        onHealthChanged.Invoke(currentHealth / maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            Debug.Log("[PlayerHealth] TakeDamage called but player is already dead - ignoring.");
            return;
        }

        float old = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        healthDelta = currentHealth - old; // negative on damage

        Debug.Log($"[PlayerHealth] Took {amount} damage. old:{old} new:{currentHealth}");

        // notify listeners with normalized value
        onHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0f && !isDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return; // optional: do not heal dead players
        float old = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        healthDelta = currentHealth - old; // positive on heal

        Debug.Log($"[PlayerHealth] Healed {amount}. old:{old} new:{currentHealth}");
        onHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[PlayerHealth] Player died! Invoking onDied.");
        onDied?.Invoke();
    }

    // optional convenience for other systems:
    public bool IsDead() => isDead;
}
