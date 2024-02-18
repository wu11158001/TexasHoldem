using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using TexasHoldemProtobuf;

public class BaseView :MonoBehaviour
{
    protected string hotFixPath;

    protected AppDomain appdomain { get { return ILRuntimeManager.Instance.appdomain; } }
    protected MainPack pack;
    protected Queue<MainPack> coradcastPackList = new Queue<MainPack>();

    public virtual void Init(BaseView baseView, GameObject obj)
    {
        hotFixPath = $"HotFix_Project.FX_{this.GetType().Name}";

        appdomain.Invoke($"{hotFixPath}", "Init", null, baseView, obj);
    }

    public virtual void Awake()
    {
        Init(this, gameObject);
        appdomain.Invoke($"{hotFixPath}", "Awake", null, null);
    }

    public virtual void OnEnable()
    {
        appdomain.Invoke($"{hotFixPath}", "OnEnable", null, null);
    }

    public virtual void Start()
    {
        appdomain.Invoke($"{hotFixPath}", "Start", null, null);
    }

    public virtual void Update()
    {
        appdomain.Invoke($"{hotFixPath}", "Update", null, null);

        if(pack != null)
        {
            HandleRequest(pack);
            pack = null;
        }

        if (coradcastPackList.Count != 0)
        {
            HandleBroadcast(coradcastPackList.Dequeue());
        }
    }

    public virtual void OnDisable()
    {
        appdomain.Invoke($"{hotFixPath}", "OnDisable", null, null);
    }

    public virtual void OnDestroy()
    {
        appdomain.Invoke($"{hotFixPath}", "OnDestroy", null, null);
    }

    /// <summary>
    /// 發送協議
    /// </summary>
    /// <param name="pack"></param>
    public virtual void SendRequest(MainPack pack)
    {
        RequestManager.Instance.Send(pack, ReciveRequest);
    }

    /// <summary>
    /// 接收協議
    /// </summary>
    /// <param name="pack"></param>
    public virtual void ReciveRequest(MainPack pack)
    {
        this.pack = pack;
    }

    /// <summary>
    /// 處理協議
    /// </summary>
    /// <param name="pack"></param>
    public virtual void HandleRequest(MainPack pack)
    {
        appdomain.Invoke($"{hotFixPath}", "HandleRequest", null, pack);
    }

    /// <summary>
    /// 接收廣播訊息
    /// </summary>
    /// <param name="pack"></param>
    public virtual void ReciveBroadcast(MainPack pack)
    {
        coradcastPackList.Enqueue(pack);
    }

    /// <summary>
    /// 處理廣播訊息
    /// </summary>
    /// <param name="pack"></param>
    public virtual void HandleBroadcast(MainPack pack)
    {
        appdomain.Invoke($"{hotFixPath}", "HandleBroadcast", null, pack);
    }
}
