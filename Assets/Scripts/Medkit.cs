using UnityEngine;

public class Medkit : MonoBehaviour
{
    public float healAmount = 25f;
    public AudioClip pickupSfx;
    public GameObject pickupVfx;

    public void OnShot()
    {
        PlayerHealth ph = Object.FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.Heal(healAmount);
            Debug.Log($"[Medkit] Healed player by {healAmount}");
        }

        if (pickupVfx != null) Instantiate(pickupVfx, transform.position, Quaternion.identity);
        if (pickupSfx != null) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        Destroy(gameObject);
    }
}
