using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TexasHoldemProtobuf;

public class ModeView : BaseView
{
    [SerializeField]
    private UserInfoView userInfoView;

    public override void Awake()
    {
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

    public override void OnDisable()
    {
        base.OnDisable();
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
}
