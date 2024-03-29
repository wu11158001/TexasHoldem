using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetChipsAction : BaseView
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
    /// 設定籌碼值
    /// </summary>
    /// <param name="value"></param>
    public void SetChipsValue(string value, Transform pointTarget)
    {
        appdomain.Invoke($"{hotFixPath}", "SetChipsValue", null, value, pointTarget);
    }
}
