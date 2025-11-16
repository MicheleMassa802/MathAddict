using System.Collections;
using UnityEngine;

/*
 * Manages the sounds played triggered based on actions from the player or other events.
 */
public class SoundSystem : MonoBehaviour
{
    #region SFX Properties set on the GameObject on the scene
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [SerializeField] private AudioClip clickSpinSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip tryAgainSound;
    [SerializeField] private AudioClip backgroundMusic;
    
    #endregion
    
    private const float WinVolume = 0.6f;
    private const float JackpotVolume = 0.9f;
    private const float LoseVolume = 1f;
    private const float TryAgainVolume = 0.5f;
    private const float SpinVolume = 0.7f;
    private const float MusicVolume = 0.2f;
    private const float MusicSecondaryVolume = 0.1f;
    private Coroutine _sfxOneShotPlayer;

    private void Start()
    {
        if (!sfxSource || !bgmSource || !clickSpinSound || !winSound || !loseSound || !backgroundMusic)
        {
            Debug.LogError($"Sound properties are null. Check the GameObject {this.name}!");
        }
    }

    private IEnumerator PlaySfxCoroutine(AudioClip audioClip, float clipVolume) {
        // lower bgm volume, play then resume
        bgmSource.volume = MusicSecondaryVolume;
        sfxSource.volume = clipVolume;
        sfxSource.PlayOneShot(audioClip);
        yield return new WaitForSeconds(audioClip.length);
        bgmSource.volume = MusicVolume;
    }

    private void PlaySfx(AudioClip audioClip, float clipVolume)
    {
        if (_sfxOneShotPlayer != null)
        {
            sfxSource.volume = 0f;
            StopCoroutine(_sfxOneShotPlayer);            
        }
        StartCoroutine(PlaySfxCoroutine(audioClip, clipVolume));
    }
    
    private void ToggleMusic(AudioClip clip = null) {
        // leave audio clip null if stopping music, OTW, we play
        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.volume = MusicVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }
    }
    
    #region SFX interface for MAUnityManager
    public void PlaySpinSound()
    {
        PlaySfx(clickSpinSound, SpinVolume);
    }

    public void PlayWinSound(bool jackpot)
    {
        PlaySfx(winSound, jackpot ? JackpotVolume : WinVolume);
    }

    public void PlayLoseSound()
    {
        PlaySfx(loseSound, LoseVolume);
    }
    
    public void PlayTryAgainSound()
    {
        PlaySfx(tryAgainSound, TryAgainVolume);
    }
    
    public void PlayBgm()
    {
        ToggleMusic(backgroundMusic);
    }
    
    public void StopBgm()
    {
        ToggleMusic();
    }
    #endregion
}
