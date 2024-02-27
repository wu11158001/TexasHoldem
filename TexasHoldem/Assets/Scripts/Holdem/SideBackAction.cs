using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideBackAction : BaseView
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
    /// 設定退回邊池訊息
    /// </summary>
    /// <param name="value"></param>
    public void SetSideBackInfo(string value)
    {
        appdomain.Invoke($"{hotFixPath}", "SetSideBackInfo", null, value);
    }
}
