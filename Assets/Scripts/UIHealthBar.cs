using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [Tooltip("Image with Image.Type = Filled")]
    public Image fillImage;

    [Tooltip("Smooth factor. 0 for instant")]
    public float smoothSpeed = 8f;

    private float targetFill = 1f;

    void Start()
    {
        var ph = Object.FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.onHealthChanged.AddListener(OnHealthChanged);
            ph.onDied.AddListener(OnPlayerDied); // optional if you want to react here
            // immediately set
            OnHealthChanged(ph.currentHealth / ph.maxHealth);
        }
        else
        {
            Debug.LogWarning("[UIHealthBar] No PlayerHealth found in scene.");
        }
    }

    void Update()
    {
        if (fillImage == null) return;
        if (smoothSpeed <= 0f) fillImage.fillAmount = targetFill;
        else fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
    }

    void OnHealthChanged(float normalized)
    {
        targetFill = Mathf.Clamp01(normalized);
    }

    void OnPlayerDied()
    {
        // optional: flash, or set fill to 0 instantly
        targetFill = 0f;
        fillImage.fillAmount = 0f;
    }
}
