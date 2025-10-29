using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    [Header("Fade Effect")]
    public CanvasGroup gameOverCanvasGroup; // assign the CanvasGroup from GameOverPanel (optional)
    public float panelFadeDuration = 0.6f;

    [Header("UI")]
    public GameObject gameOverPanel;        // assign the panel (initially inactive)
    public Image redFlashImage;             // full-screen red Image (optional)

    [Header("Settings")]
    public float flashDuration = 0.6f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.85f);

    [Header("Audio (Game Over)")]
    public AudioClip gameOverSfx;               // clip that plays on game over
    [Range(0f, 1f)] public float gameOverSfxVolume = 1f;

    [Header("Audio Fade Options")]
    public bool fadeOtherAudio = true;          // if true, other audio will smoothly fade out
    public float otherAudioFadeDuration = 0.5f; // seconds (uses unscaled time)

    [Header("Optional Title Pulse (Polish)")]
    public Transform titleTransform;        // assign the title (e.g., Game_over_title) if you want it to pulse
    public float titlePulseScale = 1.06f;
    public float titlePulseDuration = 0.8f;

    // internal audio lists for fading
    private List<AudioSource> _sourcesToFade = new List<AudioSource>();
    private List<float> _originalVolumes = new List<float>();

    void Awake()
    {
        // ensure panel hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // ensure red flash image is transparent and non-interactable
        if (redFlashImage != null)
        {
            var c = redFlashImage.color;
            c.a = 0f;
            redFlashImage.color = c;
            redFlashImage.raycastTarget = false;
            redFlashImage.enabled = true;
        }

        // subscribe to the player's death event (if a PlayerHealth exists)
        var ph = Object.FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
            ph.onDied.AddListener(HandleGameOver);
    }

    public void HandleGameOver()
    {
        StartCoroutine(DoGameOverSequence());
    }

    IEnumerator DoGameOverSequence()
    {
        // optional: fade other audio so the game over sfx stands out
        if (fadeOtherAudio)
        {
            _sourcesToFade.Clear();
            _originalVolumes.Clear();

            AudioSource[] all = FindObjectsOfType<AudioSource>();
            foreach (var s in all)
            {
                if (s == null) continue;
                // skip sources that already use the same clip (so we don't mute the SFX we're about to play)
                if (s.clip == gameOverSfx) continue;
                if (s.volume <= 0f) continue;

                _sourcesToFade.Add(s);
                _originalVolumes.Add(s.volume);
            }

            if (_sourcesToFade.Count > 0)
                StartCoroutine(FadeOutAudioSources(_sourcesToFade, _originalVolumes, otherAudioFadeDuration));
        }

        // play the game over sound (uses PlayClipAtPoint so it plays regardless of timeScale)
        if (gameOverSfx != null)
            AudioSource.PlayClipAtPoint(gameOverSfx, Camera.main != null ? Camera.main.transform.position : Vector3.zero, gameOverSfxVolume);

        // red flash overlay (unscaled)
        if (redFlashImage != null)
        {
            float timer = 0f;
            while (timer < flashDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / flashDuration);
                Color c = flashColor;
                // fade alpha from flashColor.a -> 0
                c.a = Mathf.Lerp(flashColor.a, 0f, t);
                redFlashImage.transform.SetAsLastSibling(); // ensure on top
                redFlashImage.color = c;
                yield return null;
            }
            // ensure invisible
            var final = redFlashImage.color; final.a = 0f; redFlashImage.color = final;
        }

        // show panel and fade it in (if CanvasGroup assigned)
        if (gameOverPanel != null)
        {
            gameOverPanel.transform.SetAsLastSibling();
            gameOverPanel.SetActive(true);

            if (gameOverCanvasGroup != null)
            {
                // ensure starting alpha is zero
                gameOverCanvasGroup.alpha = 0f;
                yield return StartCoroutine(FadeCanvasGroupIn(gameOverCanvasGroup, panelFadeDuration));
            }
            else
            {
                // small pause so the player can perceive the flash before the panel appears
                yield return new WaitForSecondsRealtime(0.08f);
            }
        }

        // start title pulse if assigned (runs unscaled while paused)
        if (titleTransform != null)
            StartCoroutine(PulseTitleLoop(titleTransform, titlePulseScale, titlePulseDuration));

        // finally pause the game (after visuals & audio started)
        Time.timeScale = 0f;
    }

    IEnumerator FadeOutAudioSources(List<AudioSource> sources, List<float> originals, float duration)
    {
        if (sources == null || sources.Count == 0) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float inv = 1f - t;
            for (int i = 0; i < sources.Count; i++)
            {
                var s = sources[i];
                if (s == null) continue;
                s.volume = originals[i] * inv;
            }
            yield return null;
        }
        // ensure zero at the end
        for (int i = 0; i < sources.Count; i++)
        {
            var s = sources[i];
            if (s == null) continue;
            s.volume = 0f;
        }
    }

    IEnumerator FadeCanvasGroupIn(CanvasGroup cg, float duration)
    {
        cg.alpha = 0f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(timer / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    IEnumerator PulseTitleLoop(Transform t, float maxScale, float cycleDuration)
    {
        Vector3 baseScale = t.localScale;
        float half = Mathf.Max(0.01f, cycleDuration * 0.5f);
        while (true)
        {
            // scale up
            float timer = 0f;
            while (timer < half)
            {
                timer += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(1f, maxScale, Mathf.SmoothStep(0f, 1f, timer / half));
                t.localScale = baseScale * s;
                yield return null;
            }
            // scale down
            timer = 0f;
            while (timer < half)
            {
                timer += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(maxScale, 1f, Mathf.SmoothStep(0f, 1f, timer / half));
                t.localScale = baseScale * s;
                yield return null;
            }
        }
    }

    // UI button hookups
    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
