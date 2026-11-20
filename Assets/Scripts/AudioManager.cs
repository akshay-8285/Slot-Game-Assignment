using UnityEngine;

// Simple audio manager that plays sound effects for the slot machine.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Master Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip spinButtonClickSound;
    [SerializeField] private AudioClip leverPullSound;
    [SerializeField] private AudioClip reelSpinLoopSound;
    [SerializeField] private AudioClip reelStopSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip jackpotSound;
    [SerializeField] private AudioClip betChangeSound;
    [SerializeField] private AudioClip insufficientBalanceSound;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;

    private void Awake()
    {
        // Singleton pattern so any script can call AudioManager.Instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Setup audio sources if not already assigned in the Inspector
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D sound for UI

        if (loopSource == null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
        }
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 0f; // 2D sound
        loopSource.loop = true;
    }

    // Allows adjusting SFX volume at runtime
    public float Volume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            if (loopSource != null) loopSource.volume = sfxVolume;
        }
    }

    // Plays a one-shot sound effect through the shared sfxSource
    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    // 1. Plays sound when player clicks the Spin button
    public void PlaySpinButton()
    {
        PlayOneShot(spinButtonClickSound);
    }

    // 2. Plays mechanical ratchet sound when lever moves down
    public void PlayLeverPull()
    {
        PlayOneShot(leverPullSound);
    }

    // 3. Starts continuous reel spinning sound loop
    public void StartReelSpinLoop()
    {
        if (reelSpinLoopSound != null && loopSource != null)
        {
            loopSource.clip = reelSpinLoopSound;
            loopSource.volume = sfxVolume;
            loopSource.Play();
        }
    }

    // Stops continuous reel spinning sound loop
    public void StopReelSpinLoop()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    // 4. Plays short stop sound when an individual reel lands
    public void PlayReelStop()
    {
        PlayOneShot(reelStopSound);
    }

    // 5 & 6. Plays win sound (or jackpot sound for big payout)
    public void PlayWin(bool isJackpot = false)
    {
        if (isJackpot && jackpotSound != null)
        {
            PlayOneShot(jackpotSound);
        }
        else
        {
            PlayOneShot(winSound);
        }
    }

    // 7. Plays click when bet amount changes
    public void PlayBetChange()
    {
        PlayOneShot(betChangeSound);
    }

    // 8. Plays error sound when balance is too low to spin
    public void PlayInsufficientBalance()
    {
        PlayOneShot(insufficientBalanceSound);
    }
}
