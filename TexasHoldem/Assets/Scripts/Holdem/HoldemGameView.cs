using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TexasHoldemProtobuf;

public class HoldemGameView : BaseView
{
    public override void Awake()
    {
        RequestManager.Instance.RegisterBroadcast(ActionCode.UpdateRoomUserInfo, ReciveBroadcast);
        base.Awake();
    }

    public override void OnEnable()
    {
        base.OnEnable();
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

    public override void ReciveBroadcast(MainPack pack)
    {
        base.ReciveBroadcast(pack);
    }
}
