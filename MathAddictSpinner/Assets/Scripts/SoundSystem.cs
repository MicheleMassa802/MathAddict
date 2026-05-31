using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.Reflection;
using UnityEngine.Scripting;
using Debug = UnityEngine.Debug;


/*
 * Manages the sounds played triggered based on actions from the player or other events.
 */
public class SoundSystem : MonoBehaviour
{
    #region SFX Properties set on the GameObject on the scene
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [SerializeField] private AudioClip spinSound;
    [SerializeField] private AudioClip bigWinSound;
    [SerializeField] private AudioClip smallWinSound;
    [SerializeField] private AudioClip bigLoseSound;
    [SerializeField] private AudioClip smallLoseSound;

    [SerializeField] private AudioClip comboHitSound;
    [SerializeField] private AudioClip backgroundMusic;
    #endregion
    
    #region Game Components related to the audio system set on the scene
    [SerializeField] private Image audioButtonImage;
    [SerializeField] private Sprite audioOn;
    [SerializeField] private Sprite audioOff;
    #endregion
    
    private const float WinVolume = 0.8f;
    private const float JackpotVolume = 1f;
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
        if (!audioButtonImage || !audioOn || !audioOff)
        {
            Debug.LogError($"UI properties are null. Check the GameObject {this.name}!");
        }

        // start out the game on Mute
        soundOn = true;
        ToggleSoundInternal("false");
    }

    private IEnumerator PlaySfxCoroutine(AudioClip audioClip, float clipVolume) {
        // lower bgm volume, play then resume
        // bgmSource.volume = MusicSecondaryVolume;
        sfxSource.volume = clipVolume;
        sfxSource.PlayOneShot(audioClip);
        yield return null;
        // yield return new WaitForSeconds(audioClip.length);
        // bgmSource.volume = MusicVolume;
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
        PlaySfx(spinSound, SpinVolume);
    }

    public void PlayBigWinSound(bool jackpot)
    {
        PlaySfx(bigWinSound, jackpot ? JackpotVolume : WinVolume);
    }
    
    public void PlaySmallWinSound()
    {
        PlaySfx(smallWinSound, WinVolume);
    }

    public void PlayBigLoseSound()
    {
        PlaySfx(bigLoseSound, LoseVolume);
    }
    
    public void PlaySmallLoseSound()
    {
        PlaySfx(smallLoseSound, LoseVolume);
    }
    
    public void PlayComboHitSound()
    {
        PlaySfx(comboHitSound, SpinVolume);
    }
    #endregion
    
    public void ToggleSoundInternal(string contactControllerString)
    {
        bool contactController = contactControllerString == "true";

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
            if (contactController)
            {
                // if music comes from unity button, then turn on music, else, leave off
                ToggleMusic(backgroundMusic);
            }
            soundOn = true;
            audioButtonImage.sprite = audioOn;
            sfxSource.volume = prevSFXVolume;
        }
        
        // send message to JS layer to toggle sound in second unity instance
        if (contactController)
        {
            MAUnityManager.Instance.ParseAndToggleSound();
        }
    }
}
