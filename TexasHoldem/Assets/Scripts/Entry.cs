using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TexasHoldemProtobuf;

public class Entry : UnitySingleton<Entry>
{
    ClientManager clientManager = null;

    public override void Awake()
    {
        base.Awake();

        gameObject.AddComponent<ViewABName>();
        gameObject.AddComponent<ILRuntimeManager>();
        gameObject.AddComponent<ABManager>();
        gameObject.AddComponent<UIManager>();
    }

    /// <summary>
    /// 開始連線
    /// </summary>
    async public void StartConnect()
    {
        Debug.Log("開始連線...");
        clientManager = gameObject.AddComponent<ClientManager>();
        gameObject.AddComponent<RequestManager>();

        await UIManager.Instance.CreateToolsView();
        await UIManager.Instance.ShowView(ViewType.LoginView);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            UIManager.Instance.SwitchWaitView(true);
        }
    }

    private void OnDestroy()
    {
        clientManager.OnDestroy();
    }
}
