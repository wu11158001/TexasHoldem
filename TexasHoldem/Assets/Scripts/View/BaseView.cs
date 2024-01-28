using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;

public class BaseView :MonoBehaviour
{
    protected string hotFixName = "HotFix_Project.View.FX_";
    protected AppDomain appdomain { get { return ILRuntimeManager.Instance.appdomain; } }

    public virtual void Init(BaseView baseView, GameObject obj)
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "Init", null, baseView, obj);
    }

    public virtual void Awake()
    {
        Init(this, gameObject);
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "Awake", null, null);
    }

    public virtual void Start()
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "Start", null, null);
    }

    public virtual void Update()
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "Update", null, null);
    }
}
