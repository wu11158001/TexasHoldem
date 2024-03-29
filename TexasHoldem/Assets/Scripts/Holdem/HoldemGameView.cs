using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TexasHoldemProtobuf;

public class HoldemGameView : BaseView
{
    public override void Awake()
    {
        RequestManager.Instance.RegisterBroadcast(ActionCode.UpdateRoomUserInfo, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.OtherUserExitRoom, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.GameStage, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.ShowUserAction, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.ActionerOrder, ReciveBroadcast);
        RequestManager.Instance.RegisterBroadcast(ActionCode.SidePotResult, ReciveBroadcast);

        base.Awake();
    }

    public override void OnEnable()
    {
        base.OnEnable();

        RequestManager.Instance.RegisterRequest(ActionCode.ForcedExit, ReciveRequest);
        RequestManager.Instance.RegisterRequest(ActionCode.ExitRoom, ReciveRequest);
    }

    public override void Start()
    {
        base.Start();
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

    public override void HandleBroadcast(MainPack pack)
    {
        base.HandleBroadcast(pack);
    }
}
