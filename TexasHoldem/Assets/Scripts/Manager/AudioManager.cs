using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : UnitySingleton<AudioManager>
{
    private AudioSource audioSource;

    private List<string> loadedABName = new List<string>();
    private List<AudioClip> clipList = new List<AudioClip>();

    private string localMusic = "Holdem_MusicVolume";
    public float TempMusic { get; set; }
    private float musicVolume;
    public float MusicVolume 
    {
        get
        { 
            return musicVolume;
        }
        set
        {
            musicVolume = value;
            audioSource.volume = value;
            PlayerPrefs.SetFloat(localMusic, value);
        }
    }

    private string localSound = "Holdem_SoundVolume";
    public float TempSound { get; set; }
    private float soundVolume;
    public float SoundVolume
    {
        get
        {
            return soundVolume;
        }
        set
        {
            soundVolume = value;
            PlayerPrefs.SetFloat(localSound, value);
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        //載入本地音量       
        if (PlayerPrefs.HasKey(localMusic))
        {
            MusicVolume = PlayerPrefs.GetFloat(localMusic);
        }
        else
        {
            MusicVolume = 1.0f;
            PlayerPrefs.SetFloat(localMusic, 1);
        }

        if (PlayerPrefs.HasKey(localSound))
        {
            SoundVolume = PlayerPrefs.GetFloat(localSound);
        }
        else
        {
            SoundVolume = 1.0f;
            PlayerPrefs.SetFloat(localSound, 1);
        }
    }

    /// <summary>
    /// 添加音效資源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="ab"></param>
    public void AddClip(string abName, AssetBundle ab)
    {
        if (loadedABName.Contains(abName)) return;

        loadedABName.Add(abName);
        List<AudioClip> downloadClips = ab.LoadAllAssets<AudioClip>().ToList();

        foreach (var clip in downloadClips)
        {
            if (!clipList.Contains(clip))
            {
                clipList.Add(clip);
            }
        }
    }

    /// <summary>
    /// 播放按鈕音效
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySound("ButtonClick");
    }

    /// <summary>
    /// 播放BGM
    /// </summary>
    public void PlayBGM()
    {
        for (int i = 0; i < clipList.Count; i++)
        {
            if (clipList[i].name == "BGM")
            {
                audioSource.clip = clipList[i];
                audioSource.Play();
                audioSource.loop = true;
                audioSource.volume = MusicVolume;
                return;
            }
        }

        Debug.LogError($"未找到BGM");
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="rate"></param>
    public void PlaySound(string clipName, float rate = 1)
    {
        if (SoundVolume == 0) return;

        for (int i = 0; i < clipList.Count; i++)
        {
            if (clipList[i].name == clipName)
            {
                GameObject obj = new GameObject();
                obj.transform.SetParent(gameObject.transform);
                AudioSource source = obj.AddComponent<AudioSource>();
                source.clip = clipList[i];
                source.volume = SoundVolume * rate;
                source.Play();

                RemoveSound(source);
                return;
            }
        }

        Debug.LogError($"未找到音效:{clipName}");
    }

    /// <summary>
    /// 移除音效
    /// </summary>
    /// <param name="source"></param>
    private async void RemoveSound(AudioSource source)
    {
        await Task.Delay((int)(source.clip.length * 1000));
        GameObject.Destroy(source.gameObject);
    }
}
