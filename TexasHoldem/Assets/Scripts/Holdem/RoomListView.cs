using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TexasHoldemProtobuf;

public class RoomListView : BaseView
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void SendRequest(MainPack pack)
    {
        base.SendRequest(pack);
    }

    public override void ReciveRequest(MainPack pack)
    {
        base.ReciveRequest(pack);
    }

    public override void HandleRequest(MainPack pack)
    {
        base.HandleRequest(pack);
    }

    /// <summary>
    /// 設定房間訊息
    /// </summary>
    /// <param name="roomPack"></param>
    public void SetRoomInfo(RoomPack roomPack)
    {
        appdomain.Invoke($"{hotFixPath}", "SetRoomInfo", null, roomPack);
    }
}
