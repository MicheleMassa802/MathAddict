using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    
    #region Game Components related to the audio system set on the scene
    [SerializeField] private Image audioButtonImage;
    [SerializeField] private Sprite audioOn;
    [SerializeField] private Sprite audioOff;
    #endregion
    
    private const float WinVolume = 0.6f;
    private const float JackpotVolume = 0.9f;
    private const float LoseVolume = 1f;
    private const float TryAgainVolume = 0.5f;
    private const float SpinVolume = 0.7f;
    private const float MusicVolume = 0.2f;
    private const float MusicSecondaryVolume = 0.1f;
    private Coroutine _sfxOneShotPlayer;
    
    private bool soundOn = false;
    private float prevSFXVolume = 0.0f;

    private void Start()
    {
        if (!sfxSource || !bgmSource || !clickSpinSound || !winSound || !loseSound || !backgroundMusic)
        {
            Debug.LogError($"Sound properties are null. Check the GameObject {this.name}!");
        }

        if (!audioButtonImage || !audioOn || !audioOff)
        {
            Debug.LogError($"UI properties are null. Check the GameObject {this.name}!");
        }

        // start out the game on Mute
        soundOn = true;
        ToggleSound();
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
        if (!soundOn)
        {
            return;
        }
        
        if (_sfxOneShotPlayer != null)
        {
            sfxSource.volume = 0f;
            StopCoroutine(_sfxOneShotPlayer);            
        }
        _sfxOneShotPlayer = StartCoroutine(PlaySfxCoroutine(audioClip, clipVolume));
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
    #endregion
    
    public void ToggleSound()
    {
        if (soundOn)
        {
            soundOn = false;
            ToggleMusic();
            audioButtonImage.sprite = audioOff;
            
            // check for active sfx!
            if (_sfxOneShotPlayer != null)
            {
                prevSFXVolume = sfxSource.volume;
                sfxSource.volume = 0f;
            }
        }
        else
        {
            ToggleMusic(backgroundMusic);
            soundOn = true;
            audioButtonImage.sprite = audioOn;
            sfxSource.volume = prevSFXVolume;
        }
    }
}
