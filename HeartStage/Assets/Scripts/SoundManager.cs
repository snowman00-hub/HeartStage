using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } 

    [Header("Field")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();

    private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();
    private float defaultCooldownTime = 0.1f; // 기본 쿨타임 0.1초

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            sfxSource.outputAudioMixerGroup = sfxMixerGroup; // 믹서 그룹 할당

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
            bgmSource.outputAudioMixerGroup = bgmMixerGroup; // 믹서 그룹 할당

        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(string clipName, float volume = 1f, float cooldownTime = -1f)
    {
        // 쿨타임 확인
        if (soundCooldowns.ContainsKey(clipName))
        {
            if (Time.time < soundCooldowns[clipName])
                return;
        }

        var clip = GetSFXClip(clipName);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);

            float actualCooldown = cooldownTime > 0 ? cooldownTime : defaultCooldownTime;
            soundCooldowns[clipName] = Time.time + actualCooldown;
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

    public void SetSFXVolume(float volume) // 개인 오디오 볼륨 조절
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void PlayRandomSFX(string[] clipNames) // 여러개 클립 중 랜덤으로 재생
    {
        if (clipNames == null || clipNames.Length == 0)
        {
            Debug.LogWarning("[SoundManager] 재생할 사운드 클립이 없습니다.");
            return;
        }

        int randomIndex = Random.Range(0, clipNames.Length);
        string selectedClip = clipNames[randomIndex];

        PlaySFX(selectedClip);
    }

    public void PlayMonsterHitSound()
    {
        string[] hitSounds = 
        {
            "mon_hit_01",
            "mon_hit_02",
            "mon_hit_03"
        };

        PlayRandomSFX(hitSounds);
    }

    public void SetSFXVolumeByMixer(float volume)
    {
        // 0~1 값을 dB로 변환 (-80dB ~ 0dB)
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("SFXVolume", dB);
    }
}
