using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float damage = 25f;
    public GameObject explosionVfx;
    public AudioClip explosionSfx;
    private bool exploded = false;

    private void Awake()
    {
        StartCoroutine(DestroyBomb());
    }

    // called by shooter
    public void OnShot()
    {
        Pop();
    }

    // main explosion logic
    public void Pop()
    {
        if (exploded) return;
        exploded = true;

        PlayerHealth ph = Object.FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
        {
            Debug.Log($"[Bomb] Damaging player by {damage}");
            ph.TakeDamage(damage);
        }

        if (explosionVfx != null) Instantiate(explosionVfx, transform.position, Quaternion.identity);
        if (explosionSfx != null) AudioSource.PlayClipAtPoint(explosionSfx, transform.position);

        Destroy(gameObject);
    }

    IEnumerator DestroyBomb()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
