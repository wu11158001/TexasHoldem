using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewABName : UnitySingleton<ViewABName>
{
    private Dictionary<ViewType, string> abPathDic = new Dictionary<ViewType, string>();

    private void Awake()
    {
        abPathDic.Add(ViewType.TipView, "view");
        abPathDic.Add(ViewType.LoadingView, "view");
        abPathDic.Add(ViewType.WaitView, "view");
        abPathDic.Add(ViewType.ConfirmView, "view");
        abPathDic.Add(ViewType.LoginView, "view");
        abPathDic.Add(ViewType.ModeView, "view");

        abPathDic.Add(ViewType.LobbyView, "holdem");
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
            Debug.LogError($"沒有找到 {viewType} AB資源");
            return "";
        }
    }
}