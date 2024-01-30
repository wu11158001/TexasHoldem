using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConfirmView : BaseView
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// 設定確認視窗
    /// </summary>
    /// <param name="confirmCallBack"></param>
    /// <param name="str"></param>
    /// <param name="isHaveCancel"></param>
    public void SetConfirmView(UnityAction confirmCallBack, string str, bool isHaveCancel = true)
    {
        appdomain.Invoke($"{hotFixName}{this.GetType().Name}", "SetConfirmView", null, confirmCallBack, str, isHaveCancel);
    }
}
