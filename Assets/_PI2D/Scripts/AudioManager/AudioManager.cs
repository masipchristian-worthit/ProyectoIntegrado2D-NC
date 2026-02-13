using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // ===== SINGLETON =====
    public static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                Debug.Log("No hay AudioManager");
            return instance;
        }
    }
    // ===== FIN SINGLETON =====


    [Header("Galería de Sonidos")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip[] musicLibrary;
    public AudioClip[] sfxLibrary;


    [HideInInspector] 
    public bool isPriorityPlaying = false; 
    // =========================================================

    void Awake()
    {
        // Si no hay AudioManager, lo referenciamos
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(int musicToPlay)
    {
        if (musicToPlay < 0 || musicToPlay >= musicLibrary.Length) return;

        musicSource.clip = musicLibrary[musicToPlay];
        musicSource.Play();
    }

    public void PlaySFX(int sfxToPlay)
    {
        if (sfxToPlay < 0 || sfxToPlay >= sfxLibrary.Length) return;

        sfxSource.PlayOneShot(sfxLibrary[sfxToPlay]);
    }

    // ===== NUEVO: PlaySFX con volumen personalizado =====
    public void PlaySFX(int sfxToPlay, float volume)
    {
        if (sfxToPlay < 0 || sfxToPlay >= sfxLibrary.Length) return;

        sfxSource.PlayOneShot(sfxLibrary[sfxToPlay], volume);
    }
    // ====================================================

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void PauseSFX()
    {
        sfxSource.Pause();
    }

    public void UnPauseMusic()
    {
        musicSource.UnPause();
    }

    public void UnPauseSFX()
    {
        sfxSource.UnPause();
    }

    public void ResumeMusic()
    {
        if (musicSource != null) musicSource.UnPause();
    }

}