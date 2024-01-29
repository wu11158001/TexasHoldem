using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using TexasHoldemProtobuf;

public class BaseView :MonoBehaviour
{
    protected string hotFixName = "HotFix_Project.View.FX_";

    protected AppDomain appdomain { get { return ILRuntimeManager.Instance.appdomain; } }
    protected MainPack pack;

    public virtual void Init(BaseView baseView, GameObject obj)
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "Init", null, baseView, obj);
    }

    public virtual void Awake()
    {
        Init(this, gameObject);
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "Awake", null, null);
    }

    public virtual void OnEnable()
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "OnEnable", null, null);
    }

    public virtual void Start()
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "Start", null, null);
    }

    public virtual void Update()
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "Update", null, null);

        if(pack != null)
        {
            HandleRequest(pack);
            pack = null;
        }
    }

    public virtual void OnDisable()
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "OnDisable", null, null);
    }

    public virtual void OnDestroy()
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "OnDestroy", null, null);
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
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "HandleRequest", null, pack);
    }
}
