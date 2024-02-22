using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;
using System.Threading;

public class ABManager : UnitySingleton<ABManager>
{
    private const string downloadUrl = "http://127.0.0.1:8080/TexasHoldem/AB/";

    private AssetBundle mainAB = null;
    private AssetBundleManifest manifest;

    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
    private Dictionary<string, SemaphoreSlim> downloadLocks = new Dictionary<string, SemaphoreSlim>();

    /// <summary>
    /// 獲取AB資源字典
    /// </summary>
    public Dictionary<string, AssetBundle> GetABDic { get { return abDic; } }
    
    public override void Awake()
    {
        base.Awake();
    }

    async public Task Init()
    {
        mainAB = await LoadAB(downloadUrl + "AB");
        manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        string resourceDirectory = Path.Combine(Application.streamingAssetsPath, "AB");

        CreateDirectory(Path.Combine(Application.streamingAssetsPath, resourceDirectory));

        // 獲取資源目錄下所有的 AssetBundle 文件
        string[] assetBundleFiles = Directory.GetFiles(resourceDirectory, "*", SearchOption.AllDirectories)
                                             .Where(file => !file.EndsWith(".meta"))
                                             .ToArray();

        // 加載本地資源到 AssetBundles 字典中
        foreach (string filePath in assetBundleFiles)
        {
            string abName = Path.GetFileNameWithoutExtension(filePath);
            Debug.Log($"加載本地資源:{abName}");

            if (!abDic.ContainsKey(filePath))
            {
                AssetBundle ab = AssetBundle.LoadFromFile(filePath);
                if (ab != null)
                {
                    abDic.Add(abName, ab);
                    AudioManager.Instance.GetClips();
                }
                else
                {
                    Debug.LogError($"添加本地資源失敗:{abName}");
                }
            }
        }

        Debug.Log("AB初始化完成");
    }

    /// <summary>
    /// 創建目錄
    /// </summary>
    /// <param name="resourceDirectory"></param>
    public void CreateDirectory(string resourceDirectory)
    {
        if (!Directory.Exists(resourceDirectory))
        {
            Directory.CreateDirectory(resourceDirectory);
        }        
    }

    /// <summary>
    /// 下載AB資源
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    async public Task<AssetBundle> LoadAB(string path)
    {
        using (UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(path))
        {
            var asyncOperation = webRequest.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerAssetBundle.GetContent(webRequest); 
            }
            else
            {
                Debug.LogError("下載AB資源失敗:" + webRequest.error);
                return null;
            }
        }
    }

    /// <summary>
    /// 下載AB資源到本地
    /// </summary>
    /// <param name="url"></param>
    /// <param name="abName"></param>
    /// <param name="savePath"></param>
    /// <param name="progressCallBack"></param>
    /// <returns></returns>
    async public Task<byte[]> DownloadAB(string abName, UnityAction<float> progressCallBack = null, string savePath = "AB", string url = downloadUrl)
    {
        Debug.Log($"下載AB資源到本地{abName}");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url + abName))
        {
            var asyncOperation = webRequest.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                if (progressCallBack != null)
                {
                    float progress = Mathf.Clamp01(webRequest.downloadProgress);
                    progressCallBack(progress * 100);
                }

                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                CreateDirectory(Path.Combine(Application.streamingAssetsPath, savePath));

                byte[] data = webRequest.downloadHandler.data;
                using (FileStream fs = File.Create(Path.Combine(Application.streamingAssetsPath, savePath, abName)))
                {
                    await fs.WriteAsync(data, 0, webRequest.downloadHandler.data.Length);
                    await fs.FlushAsync();
                }

                //回調進度
                if (progressCallBack != null)
                {
                    progressCallBack(100);
                }
               
                return data;
            }
            else
            {
                Debug.LogError("下載AB資源到本地失敗!!!");
                return null;
            }              
        }
    }

    /// <summary>
    /// 檢查AB資源
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    async public Task<bool> CheckAB(string abName, UnityAction<bool> callback = null)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "AB", abName);
        if (File.Exists(path))
        {
            string localHash = CalculateFileHash(path);
            string RemoteHash = await CalculateRemoteFileHash(downloadUrl + abName);

            if (callback != null)
            {
                callback(localHash == RemoteHash);
            }

            return localHash == RemoteHash;
        }
        else
        {
            if (callback != null)
            {
                callback(false);
            }

            return false;
        }
    }

    /// <summary>
    /// 計算檔案哈希值
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private string CalculateFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    /// <summary>
    /// 計算遠端文件哈希
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    async private Task<string> CalculateRemoteFileHash(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            var asyncOperation = webRequest.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                byte[] data = webRequest.downloadHandler.data;
                using (var md5 = MD5.Create())
                {
                    byte[] hashBytes = md5.ComputeHash(data);
                    return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            else
            {
                Debug.LogError("計算遠端文件哈希失敗!!!");
                return "";
            }            
        }        
    }

    /// <summary>
    /// 獲取AB包大小
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="callBack"></param>
    async public void GetABSize(string abName, UnityAction<long> callBack)
    {
        long localABSize = 0;
        UnityWebRequest webRequest;
        UnityWebRequestAsyncOperation asyncOperation;

        //本地資源大小
        string path = Path.Combine(Application.streamingAssetsPath, "AB", abName);
        if (File.Exists(path))
        {
            webRequest = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, "AB", abName));
            asyncOperation = webRequest.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                localABSize = (long)webRequest.downloadedBytes;
                Debug.Log($"本地資源大小:{localABSize}");
            }
            else
            {
                Debug.LogError($"{abName} 檢查本地AB包大小失敗: {webRequest.error}");
            }
        }

        //遠端資源大小
        webRequest = UnityWebRequest.Get(downloadUrl + abName);
        asyncOperation = webRequest.SendWebRequest();
        while (!asyncOperation.isDone)
        {
            await Task.Yield();
        }

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            long abSize = (long)webRequest.downloadedBytes;
            Debug.Log($"遠端資源大小:{abSize}");
            callBack(System.Math.Abs(abSize - localABSize));
        }
        else
        {
            Debug.LogError($"{abName} 檢查遠端AB包大小失敗: {webRequest.error}");
        }
    }

    /// <summary>
    /// 獲取AB資源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callBack"></param>
    async public Task GetAB<T>(string abName, string resName, UnityAction<T> callBack) where T: Object
    {
        SemaphoreSlim downloadLock;
        lock (downloadLocks)
        {
            if (!downloadLocks.TryGetValue(resName, out downloadLock))
            {
                downloadLock = new SemaphoreSlim(1, 1);
                downloadLocks.Add(resName, downloadLock);
            }
        }

        await downloadLock.WaitAsync();

        try
        {
            bool isDownload = await CheckAB(abName);
            if (isDownload)
            {
                if (abDic.ContainsKey(abName))
                {
                    callBack(abDic[abName].LoadAsset<T>(resName));
                }
                else
                {
                    await DownloadAB(abName);
                    await LoadLoaclAB<T>(abName, resName, callBack);
                }
            }
            else
            {
                if (abDic.ContainsKey(abName))
                {
                    Debug.Log($"移除本地舊的AB資源:{abName}");
                    abDic[abName].Unload(true);
                    abDic.Remove(abName);
                }

                await DownloadAB(abName);
                await LoadLoaclAB<T>(abName, resName, callBack);
            }
        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            downloadLock.Release();
        }
    }

    /// <summary>
    /// 載入圖集ScriptableObject
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    async public Task LoadSprite(string abName, string resName, UnityAction<Sprite[]> callBack)
    {
        await ABManager.Instance.GetAB<SpriteScriptableObject>(abName, resName, (spriteList) =>
        {
            if (spriteList != null)
            {
                Sprites sprites = spriteList.sprites;
                if (sprites != null)
                {
                    Sprite[] spriteArray = sprites.spritesList;

                    if (spriteArray != null)
                    {
                        callBack(spriteArray);
                    }
                    else
                    {
                        Debug.LogError($"{resName}:圖像合集內容為空");
                    }
                }
                else
                {
                    Debug.LogError($"{resName}:圖像合集 null !!!");
                }
            }
            else
            {
                Debug.LogError($"沒有找到圖像合集:{resName}");
            }
        });
    }

    /// <summary>
    /// 載入本地資源
    /// </summary>
    private async Task LoadLoaclAB<T>(string abName, string resName, UnityAction<T> callBack) where T : Object
    {
        Debug.Log($"載入本地資源;{abName}");
        string fullPath = Path.Combine(Application.streamingAssetsPath, "AB", abName);

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        AssetBundleCreateRequest asyncRequest = AssetBundle.LoadFromFileAsync(fullPath);
        asyncRequest.completed += operation =>
        {
            tcs.SetResult(true);
        };

        await tcs.Task;

        AssetBundle ab = asyncRequest.assetBundle;

        if (ab != null)
        {
            Debug.Log($"載入本地資源:{resName}");
            T asset = ab.LoadAsset<T>(resName);
            callBack(asset);

            if (!abDic.ContainsKey(abName))
            {
                abDic.Add(abName, ab);
                AudioManager.Instance.GetClips();
            }
        }
        else
        {
            Debug.LogError($"{abName}:載入本地資源失敗");
        }
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (Entry.Instance.isDeleteAssetBundle)
        {
            foreach (var abName in abDic.Keys)
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, "AB", abName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"移除本地資源:{abName}");
                }
                else
                {
                    Debug.LogWarning($"移除本地資源失敗:{abName}");
                }
            }
        }            
#endif
    }
}
