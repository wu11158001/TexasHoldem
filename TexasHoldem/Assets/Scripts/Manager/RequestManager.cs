using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TexasHoldemProtobuf;
using UnityEngine.Events;

public class RequestManager : UnitySingleton<RequestManager>
{
    private Dictionary<ActionCode, UnityAction<MainPack>> requsetDic = new Dictionary<ActionCode, UnityAction<MainPack>>();
    private Dictionary<ActionCode, UnityAction<MainPack>> broadcastDic = new Dictionary<ActionCode, UnityAction<MainPack>>();

    /// <summary>
    /// 註冊廣播事件
    /// </summary>
    /// <param name="actionCode"></param>
    /// <param name="callback"></param>
    public void RegisterBroadcast(ActionCode actionCode, UnityAction<MainPack> callback)
    {
        broadcastDic.Add(actionCode, callback);
    }

    /// <summary>
    /// 發送協議
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="callback"></param>
    public void Send(MainPack pack, UnityAction<MainPack> callback)
    {
        ClientManager.Instance.Send(pack);

        if (pack.SendModeCode != SendModeCode.RoomBroadcast)
        {
            if (!requsetDic.ContainsKey(pack.ActionCode))
            {
                requsetDic.Add(pack.ActionCode, callback);
            }            
        }
    }

    /// <summary>
    /// 處理回覆
    /// </summary>
    /// <param name="pack"></param>
    public void HandleResponse(MainPack pack)
    {
        if (pack.SendModeCode == SendModeCode.RoomBroadcast)
        {
            //房間廣播
            if (broadcastDic.ContainsKey(pack.ActionCode))
            {
                broadcastDic[pack.ActionCode](pack);

                if (requsetDic.ContainsKey(pack.ActionCode))
                {
                    requsetDic.Remove(pack.ActionCode);
                }
            }
            else
            {
                Debug.LogWarning("沒有相關房間廣播協議:" + pack.ActionCode);
            }
        }
        else
        {
            //一般協議
            if (requsetDic.ContainsKey(pack.ActionCode))
            {
                requsetDic[pack.ActionCode](pack);
                requsetDic.Remove(pack.ActionCode);
            }
            else
            {
                Debug.LogWarning("沒有相關協議:" + pack.ActionCode);
            }
        }        
    }

    /// <summary>
    /// 註冊處理
    /// </summary>
    /// <param name=""></param>
    public void LogonHandle(MainPack pack)
    {
        Debug.Log("註冊處理:" + pack.ReturnCode);
    }

    /// <summary>
    /// 登入處理
    /// </summary>
    /// <param name="pack"></param>
    public void LoginHandle(MainPack pack)
    {
        Debug.Log("登入處理:" + pack.ReturnCode);
    }
}
