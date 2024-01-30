using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class ABManager : UnitySingleton<ABManager>
{
    private string resUrl = "http://127.0.0.1:8080/TexasHoldem/AB/";

    //紀錄AB資源
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
    private AssetBundle mainAB = null;
    private AssetBundleManifest manifest;

    public override void Awake()
    {
        base.Awake();
    }
    
    /// <summary>
    /// 監測是否有AB資源
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public bool CheckAb(string abName)
    {
        return abDic.ContainsKey(abName);
    }

    /// <summary>
    /// 獲取AB包大小
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="callBack"></param>
    public void GetABSize(string abName, UnityAction<long> callBack)
    {
        StartCoroutine(ICheckABSize(abName, callBack));
    }

    /// <summary>
    /// 檢查AB包大小
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    private IEnumerator ICheckABSize(string abName, UnityAction<long> callBack)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(resUrl + abName);

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            long abSize = (long)webRequest.downloadedBytes;
            callBack(abSize);
        }
        else
        {
            Debug.LogError($"{abName} 檢查AB包大小失敗: {webRequest.error}");
        }
    }

    /// <summary>
    /// 獲取AB資源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callBack"></param>
    public void GetABRes<T>(string abName, string resName, UnityAction<T> callBack) where T: Object
    {
        StartCoroutine(ILoadResAsync<T>(abName, resName, callBack));
    }

    /// <summary>
    /// 加載資源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    private IEnumerator ILoadResAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : Object
    {
        yield return ILoadAB(abName);

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(resName);
        yield return abr;

        callBack(abr.asset as T);
    }

    /// <summary>
    /// 載入AB包
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="progressCallBack"></param>
    /// <param name="finishedCallBack"></param>
    public void LoadAb(string abName, UnityAction<float> progressCallBack = null)
    {
        StartCoroutine(ILoadAB(abName, progressCallBack));
    }

    /// <summary>
    /// 加載AB資源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="progressCallBack"></param>
    /// <returns></returns>
    private IEnumerator ILoadAB(string abName, UnityAction<float> progressCallBack = null)
    {
        UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(resUrl + "AB");
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(webRequest.error);
        }
        else
        {
            if (mainAB == null)
            {
                //加載主包
                mainAB = DownloadHandlerAssetBundle.GetContent(webRequest);
                manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }            

            //獲取依賴包
            AssetBundle ab = null;
            string[] strs = manifest.GetAllDependencies(abName);
            for (int i = 0; i < strs.Length; i++)
            {
                if (!abDic.ContainsKey(strs[i]))
                {
                    webRequest = UnityWebRequestAssetBundle.GetAssetBundle(resUrl + strs[i]);
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(webRequest.error);
                    }
                    else
                    {
                        ab = DownloadHandlerAssetBundle.GetContent(webRequest);
                        abDic.Add(strs[i], ab);
                    }
                }
            }

            //資源未加載過
            if (!abDic.ContainsKey(abName))
            {
                webRequest = UnityWebRequestAssetBundle.GetAssetBundle(resUrl + abName);
                webRequest.SendWebRequest();

                while (!webRequest.isDone)
                {
                    if (progressCallBack != null)
                    {
                        float progress = Mathf.Clamp01(webRequest.downloadProgress);
                        progressCallBack(progress * 100);
                    }

                    yield return null;
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    ab = DownloadHandlerAssetBundle.GetContent(webRequest);
                    abDic.Add(abName, ab);

                    if (progressCallBack != null)
                    {
                        progressCallBack(100);
                    }
                }
                else
                {
                    Debug.LogError("加載資源失敗:" + abName);
                }
            }
        }
    }
}
