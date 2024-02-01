using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.IO;

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
    /// 監測是否已有AB資源
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public bool IsDownloadAB(string abName)
    {
        //return abDic.ContainsKey(abName);
        AssetBundle[] loadedAssetBundles = (AssetBundle[])AssetBundle.GetAllLoadedAssetBundles();
        foreach (var bundle in loadedAssetBundles)
        {
            if (bundle.name == abName)
            {
                return true;
            }
        }
        return false;
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
        StartCoroutine(IGetABAsync<T>(abName, resName, callBack));
    }

    /// <summary>
    /// 獲取AB資源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    private IEnumerator IGetABAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : Object
    {
        yield return ILoadAB(abName);

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(resName);
        yield return abr;

        callBack(abr.asset as T);
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
            string[] assetsPath = manifest.GetAllDependencies(abName);
            for (int i = 0; i < assetsPath.Length; i++)
            {
                if (!abDic.ContainsKey(assetsPath[i]))
                {
                    webRequest = UnityWebRequest.Get(resUrl + assetsPath[i]);
                    yield return webRequest.SendWebRequest();

                    /*webRequest = UnityWebRequestAssetBundle.GetAssetBundle(resUrl + assetsPath[i]);
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(webRequest.error);
                    }
                    else
                    {
                        ab = DownloadHandlerAssetBundle.GetContent(webRequest);
                        abDic.Add(assetsPath[i], ab);
                    }*/
                }
            }

            //資源未加載
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

                    //回調進度
                    if (progressCallBack != null)
                    {
                        progressCallBack(100);
                    }

                    ab.Unload(false);
                }
                else
                {
                    Debug.LogError("加載資源失敗:" + abName);
                }
            }
        }
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        /*Debug.Log("AB資源卸載");
        foreach (var ab in abDic.Values)
        {
            ab.Unload(true);
        }*/
#endif
    }
}
