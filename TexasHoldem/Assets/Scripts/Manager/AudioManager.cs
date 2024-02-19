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

    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
                Debug.Log($"添加音效:{clip.name}");
                clipList.Add(clip);
            }
        }
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
                return;
            }
        }

        Debug.LogError($"未找到BGM");
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clipName"></param>
    public void PlaySound(string clipName)
    {
        for (int i = 0; i < clipList.Count; i++)
        {
            if (clipList[i].name == clipName)
            {
                GameObject obj = new GameObject();
                obj.transform.SetParent(gameObject.transform);
                AudioSource source = obj.AddComponent<AudioSource>();
                source.clip = clipList.ToList().Where(x => x.name == clipName).First();
                source.volume = 0.6f;
                source.Play();

                RemoveSound(source);
                return;
            }
        }

        Debug.LogError($"未找到音效:{clipName}");
    }

    /// <summary>
    /// 播放按鈕音效
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySound("ButtonClick");
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
