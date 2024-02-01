using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using UnityEngine.Networking;
using UnityEditor;

public class ILRuntimeManager : UnitySingleton<ILRuntimeManager>
{
    private string DllUrl 
    {
        get
        {
#if DEBUG && UNITY_EDITOR
            return "file:///" + Application.streamingAssetsPath + "/HotFix/HotFix_Project.dll";
#else
            return "http://192.168.1.172:8080//TexasHoldem/HotFix/HotFix_Project.dll";
#endif
        }
    }

    private string PdbUrl
    {
        get
        {
#if DEBUG && UNITY_EDITOR
            return "file:///" + Application.streamingAssetsPath + "/HotFix/HotFix_Project.pdb";
#else
            return "http://192.168.1.172:8080//TexasHoldem/HotFix/HotFix_Project.pdb";
#endif
        }
    }

    public AppDomain appdomain;
    private MemoryStream msDll;
    private MemoryStream msPdb;

    public override void Awake()
    {
        base.Awake();

        appdomain = new AppDomain();

        LoadHotHif();
    }

    /// <summary>
    /// 載入熱更
    /// </summary>
    public void LoadHotHif()
    {
        StartCoroutine(ILoadingHotUpdate());
    }

    /// <summary>
    /// 下載熱更
    /// </summary>
    /// <returns></returns>
    private IEnumerator ILoadingHotUpdate()
    {
        byte[] dll = null;
        byte[] pdb = null;
        
        UnityWebRequest webRequest = UnityWebRequest.Get(DllUrl);
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
        webRequest = UnityWebRequest.Get(PdbUrl);
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
#if  UNITY_EDITOR
        msPdb = new MemoryStream(pdb);
#endif

        try
        {
            appdomain.LoadAssembly(msDll, msPdb, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        catch
        {
            Debug.LogError("加载热更DLL失败!!!，確保編譯過熱更DLL");
            return;
        }

        HotFixAdapter();


#if DEBUG && UNITY_EDITOR
        appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif

        appdomain.Invoke("HotFix_Project.Main", "Init", null, null);
        Entry.Instance.StartConnect();
    }

    /// <summary>
    /// 添加熱更適配器
    /// </summary>
    private void HotFixAdapter()
    {
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
        {
            return new UnityEngine.Events.UnityAction(() =>
            {
                ((System.Action)act)();
            });
        });

        appdomain.DelegateManager.RegisterMethodDelegate<TexasHoldemProtobuf.MainPack>();
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<TexasHoldemProtobuf.MainPack>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<TexasHoldemProtobuf.MainPack>((arg0) =>
            {
                ((System.Action<TexasHoldemProtobuf.MainPack>)act)(arg0);
            });
        });

        appdomain.DelegateManager.RegisterDelegateConvertor<System.Action<BaseView, GameObject>>((act) =>
        {
            return new System.Action<BaseView, GameObject>((arg1, arg2) =>
            {
                ((System.Action<BaseView, GameObject>)act)(arg1, arg2);
            });
        });

        appdomain.DelegateManager.RegisterMethodDelegate<System.Int64>();
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.Int64>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<System.Int64>((arg0) =>
            {
                ((System.Action<System.Int64>)act)(arg0);
            });
        });

        appdomain.DelegateManager.RegisterMethodDelegate<System.Single>();
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.Single>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<System.Single>((arg0) =>
            {
                ((System.Action<System.Single>)act)(arg0);
            });
        });

        appdomain.DelegateManager.RegisterMethodDelegate<System.Object>();
        appdomain.DelegateManager.RegisterDelegateConvertor<System.Threading.TimerCallback>((act) =>
        {
            return new System.Threading.TimerCallback((state) =>
            {
                ((System.Action<System.Object>)act)(state);
            });
        });

    }
}
