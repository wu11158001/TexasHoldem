using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class ILRuntimeManager : UnitySingleton<ILRuntimeManager>
{
    private const string downloadUrl = "http://127.0.0.1:8080//TexasHoldem/HotFix/";

    public AppDomain appdomain;
    private MemoryStream msDll;
    private MemoryStream msPdb;

    public override void Awake()
    {
        base.Awake();
    }

    async public Task Init()
    {
        appdomain = new AppDomain();

        byte[] dll = await ABManager.Instance.DownloadAB("HotFix_Project.dll", null, "HotFix", downloadUrl);
        byte[] pdb = await ABManager.Instance.DownloadAB("HotFix_Project.pdb", null, "HotFix", downloadUrl);

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

        appdomain.DelegateManager.RegisterMethodDelegate<System.Boolean>();
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.Boolean>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<System.Boolean>((arg0) =>
            {
                ((System.Action<System.Boolean>)act)(arg0);
            });
        });

        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Sprite>();
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<UnityEngine.Sprite>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<UnityEngine.Sprite>((arg0) =>
            {
                ((System.Action<UnityEngine.Sprite>)act)(arg0);
            });
        });

        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Sprite[]>();
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<UnityEngine.Sprite[]>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<UnityEngine.Sprite[]>((arg0) =>
            {
                ((System.Action<UnityEngine.Sprite[]>)act)(arg0);
            });
        });

        appdomain.DelegateManager.RegisterFunctionDelegate<System.Int32, System.Boolean>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Int32, System.Int32>();
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (Entry.Instance.isDeleteAssetBundle)
        {
            File.Delete(Path.Combine(Application.streamingAssetsPath, "HotFix", "HotFix_Project.dll"));
            File.Delete(Path.Combine(Application.streamingAssetsPath, "HotFix", "HotFix_Project.pdb"));
        }    
#endif
    }
}
