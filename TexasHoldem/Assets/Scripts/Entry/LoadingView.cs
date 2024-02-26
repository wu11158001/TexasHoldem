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
    /// 開啟載入畫面
    /// </summary>
    public void OpenLoading()
    {
        appdomain.Invoke($"{hotFixPath}", "OpenLoading", null, null);
    }

    /// <summary>
    /// 關閉載入畫面
    /// </summary>
    public void CloseLoading(GameObject nextView = null)
    {
        appdomain.Invoke($"{hotFixPath}", "CloseLoading", null, nextView);
    }
}
