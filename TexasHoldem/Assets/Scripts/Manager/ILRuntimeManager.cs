using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using UnityEngine.Networking;

public class ILRuntimeManager : UnitySingleton<ILRuntimeManager>
{
    private const string dllUrl = "http://127.0.0.1:8080//TexasHoldem/HotFix/HotFix_Project.dll";
    private const string pdbUrl = "http://127.0.0.1:8080//TexasHoldem/HotFix/HotFix_Project.pdb";

    private AppDomain appdomain;
    private MemoryStream msDll;
    private MemoryStream msPdb;

    public override void Awake()
    {
        base.Awake();

        appdomain = new AppDomain();

        StartCoroutine(ICheckHotUpdate());
    }

    /// <summary>
    /// 檢查熱更
    /// </summary>
    /// <returns></returns>
    private IEnumerator ICheckHotUpdate()
    {
        byte[] dll = null;
        byte[] pdb = null;

        UnityWebRequest webRequest = UnityWebRequest.Get(dllUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("dll熱更錯誤:" + webRequest.error);
        }
        else
        {
            dll = webRequest.downloadHandler.data;
        }

#if DEBUG && UNITY_EDITOR
        webRequest = UnityWebRequest.Get(pdbUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("pdb熱更錯誤:" + webRequest.error);
        }
        else
        {
            pdb = webRequest.downloadHandler.data;
        }
#endif

        LoadHotFixAssembly(dll, pdb);

        yield return null;
    }

    /// <summary>
    /// 載入熱更資源
    /// </summary>
    /// <param name="dll"></param>
    /// <param name="pdb"></param>
    private void LoadHotFixAssembly(byte[] dll, byte[] pdb)
    {
        msDll = new MemoryStream(dll);
        msPdb = new MemoryStream(pdb);

        try
        {
            appdomain.LoadAssembly(msDll, msPdb, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        catch
        {
            Debug.LogError("加载热更DLL失败!!!，確保編譯過熱更DLL");
            return;
        }

#if DEBUG && UNITY_EDITOR
        appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif

        appdomain.Invoke("HotFix_Project.Main", "Init", null, null);
    }
}
