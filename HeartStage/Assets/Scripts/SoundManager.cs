using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } // 싱글톤

    [Header("Field")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
            if (bgmSource == null)
                bgmSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(string clipName)
    {
        var clip = GetSFXClip(clipName);
        if(clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    public void PlayBGM(string clipName, bool loop = true)
    {
        AudioClip clip = GetBGMClip(clipName);
        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }

    private AudioClip GetSFXClip(string clipName)
    {
        if(sfxDictionary.TryGetValue(clipName, out AudioClip cachedClip))
        {
            return cachedClip;  
        }

        var clip = ResourceManager.Instance.Get<AudioClip>(clipName);
        if (clip != null)
        {
            sfxDictionary[clipName] = clip;
            return clip;
        }

        Debug.LogWarning($"[SoundManager] SFX {clipName}를 찾을 수 없습니다.");
        return null;
    }

    private AudioClip GetBGMClip(string clipName)
    {
        if (bgmDictionary.TryGetValue(clipName, out AudioClip cachedClip))
            return cachedClip;

        AudioClip clip = ResourceManager.Instance.Get<AudioClip>(clipName);
        if (clip != null)
        {
            bgmDictionary[clipName] = clip;
            return clip;
        }

        Debug.LogWarning($"[SoundManager] BGM {clipName}를 찾을 수 없습니다.");
        return null;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }
}
