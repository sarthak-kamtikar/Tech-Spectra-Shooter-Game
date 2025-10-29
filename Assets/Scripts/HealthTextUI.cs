using UnityEngine;
using TMPro;

/// <summary>
/// Updates a TextMeshProUGUI with the player's current health in real time.
/// Listens to PlayerHealth.onHealthChanged if available; falls back to polling currentHealth.
/// </summary>
public class HealthTextUI : MonoBehaviour
{
    public TextMeshProUGUI healthText;      // assign in Inspector
    public float pollInterval = 0.1f;       // used only if events aren't available

    private PlayerHealth playerHealth;
    private float pollTimer = 0f;
    private bool usesEvent = false;

    void Start()
    {
        if (healthText == null)
        {
            Debug.LogWarning("[HealthTextUI] healthText not assigned.");
            enabled = false;
            return;
        }

        // find PlayerHealth instance
        playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("[HealthTextUI] No PlayerHealth found in scene.");
            healthText.text = "HP: -";
            return;
        }

        // try to subscribe to the event if it exists
        try
        {
            playerHealth.onHealthChanged.AddListener(OnHealthChanged);
            usesEvent = true;
            // set initial text
            UpdateText(playerHealth.currentHealth, playerHealth.maxHealth);
        }
        catch (System.Exception)
        {
            // event missing or signature mismatch — fall back to polling
            usesEvent = false;
            UpdateText(playerHealth.currentHealth, playerHealth.maxHealth);
        }
    }

    void Update()
    {
        if (playerHealth == null || usesEvent) return;

        // fallback polling
        pollTimer += Time.deltaTime;
        if (pollTimer >= pollInterval)
        {
            pollTimer = 0f;
            UpdateText(playerHealth.currentHealth, playerHealth.maxHealth);
        }
    }

    void OnHealthChanged(float normalized)
    {
        // normalized is 0..1 — convert to actual values for display
        float current = playerHealth != null ? playerHealth.currentHealth : Mathf.Round(normalized * 100f);
        float max = playerHealth != null ? playerHealth.maxHealth : 100f;
        UpdateText(current, max);
    }

    void UpdateText(float current, float max)
    {
        healthText.text = $"HP: {Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        if (current <= 0f)
            healthText.text += "  (DEAD)";
    }
}
