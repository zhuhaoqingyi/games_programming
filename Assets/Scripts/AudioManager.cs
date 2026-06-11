using UnityEngine;

[System.Serializable]
public class SoundEffectSettings
{
    public AudioClip clip;
    [Tooltip("音效播放时长（秒），0表示使用原始音频长度")]
    public float duration = 0f;
    [Tooltip("是否循环播放")]
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音效设置")]
    public SoundEffectSettings oreCollision;
    public SoundEffectSettings buildingPlace;
    public SoundEffectSettings thruster;
    public SoundEffectSettings mining;

    [Header("背景音乐")]
    public AudioClip backgroundMusicClip;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource thrusterSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 创建音乐音源
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        // 创建音效音源
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;
        sfxSource.playOnAwake = false;

        // 创建推进器专用音源（支持立即停止）
        thrusterSource = gameObject.AddComponent<AudioSource>();
        thrusterSource.loop = true;
        thrusterSource.volume = sfxVolume;
        thrusterSource.playOnAwake = false;
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusicClip != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusicClip;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlayOreCollision()
    {
        PlaySoundEffect(oreCollision);
    }

    public void PlayBuildingPlace()
    {
        PlaySoundEffect(buildingPlace);
    }

    public void PlayThruster()
    {
        if (thruster == null || thruster.clip == null) return;
        
        // 推进器音效使用专用音源循环播放
        if (!thrusterSource.isPlaying)
        {
            thrusterSource.clip = thruster.clip;
            thrusterSource.loop = true;
            thrusterSource.volume = sfxVolume;
            thrusterSource.Play();
        }
    }

    public void StopThruster()
    {
        if (thrusterSource != null && thrusterSource.isPlaying)
        {
            thrusterSource.Stop();
        }
    }

    public void PlayMining()
    {
        PlaySoundEffect(mining);
    }

    private void PlaySoundEffect(SoundEffectSettings settings)
    {
        if (settings == null || settings.clip == null) return;

        if (settings.loop)
        {
            // 循环播放：使用 AudioSource 直接播放
            sfxSource.clip = settings.clip;
            sfxSource.loop = true;
            sfxSource.volume = sfxVolume;
            sfxSource.Play();
        }
        else
        {
            // 非循环播放
            if (settings.duration > 0f)
            {
                // 指定时长：播放后在指定时间停止
                sfxSource.PlayOneShot(settings.clip, sfxVolume);
                Invoke(nameof(StopSfxSource), settings.duration);
            }
            else
            {
                // 使用原始音频长度
                sfxSource.PlayOneShot(settings.clip, sfxVolume);
            }
        }
    }

    private void StopSfxSource()
    {
        if (sfxSource != null && sfxSource.isPlaying)
        {
            sfxSource.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }
}
