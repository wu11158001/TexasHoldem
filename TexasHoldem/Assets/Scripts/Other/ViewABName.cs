using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewABName : UnitySingleton<ViewABName>
{
    private Dictionary<ViewType, string> abPathDic = new Dictionary<ViewType, string>();
    
    public override void Awake()
    {
        base.Awake();

        abPathDic.Add(ViewType.TipView, "entry");
        abPathDic.Add(ViewType.LoadingView, "entry");
        abPathDic.Add(ViewType.WaitView, "entry");
        abPathDic.Add(ViewType.ConfirmView, "entry");
        abPathDic.Add(ViewType.LoginView, "entry");
        abPathDic.Add(ViewType.ModeView, "entry");

        abPathDic.Add(ViewType.LobbyView, "holdem");
        abPathDic.Add(ViewType.HoldemGameView, "holdem");
    }

    /// <summary>
    /// 獲取View AB包名稱
    /// </summary>
    /// <param name="viewType"></param>
    /// <returns></returns>
    public string GetAbName(ViewType viewType)
    {
        if (abPathDic.ContainsKey(viewType))
        {
            return abPathDic[viewType];
        }
        else
        {
            Debug.LogError($"沒有找到 {viewType} AB資源!!!");
            return "";
        }
    }
}
