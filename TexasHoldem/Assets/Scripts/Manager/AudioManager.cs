using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : UnitySingleton<AudioManager>
{
    private AudioSource audioSource;

    [SerializeField]
    private List<AudioClip> clipList = new List<AudioClip>();

    private string localMusic = "Holdem_MusicVolume";
    public float TempMusic { get; set; }
    public float MusicVolume 
    {
        get
        { 
            return audioSource.volume;
        }
        set
        {
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

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
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
    /// 獲取音效
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="ab"></param>
    public void GetClips()
    {
        clipList.Clear();

        Dictionary<string, AssetBundle> abDic = ABManager.Instance.GetABDic;

        foreach (var ab in abDic)
        {
            List<AudioClip> downloadClips = ab.Value.LoadAllAssets<AudioClip>().ToList();

            foreach (var clip in downloadClips)
            {
                if (!clipList.Contains(clip))
                {
                    clipList.Add(clip);
                }
            }
        }
        

        Debug.Log($"音效添加完成。");
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
            if (clipList[i] != null && clipList[i].name == "BGM")
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
            if (clipList[i] != null && clipList[i].name == clipName)
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
