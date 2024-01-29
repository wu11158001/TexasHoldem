using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingView : BaseView
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 設定下個View
    /// </summary>
    /// <param name="nextView"></param>
    public void SetNextView(ViewType nextView)
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "SetNextView", null, nextView);
    }
}
