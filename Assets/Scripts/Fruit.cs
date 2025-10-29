using UnityEngine;

public class Fruit : MonoBehaviour
{
    public int scoreValue = 10;
    public AudioClip popSfx;
    public GameObject popVfx;

    // called by shooter
    public void OnShot()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(scoreValue);
        if (popVfx != null) Instantiate(popVfx, transform.position, Quaternion.identity);
        if (popSfx != null) AudioSource.PlayClipAtPoint(popSfx, transform.position);
        Destroy(gameObject);
    }
}
