using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingView : BaseView
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 開啟載入
    /// </summary>
    /// <param name="nextView"></param>
    public void OpenLoading(ViewType nextView)
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "OpenLoading", null, nextView);
    }
}
