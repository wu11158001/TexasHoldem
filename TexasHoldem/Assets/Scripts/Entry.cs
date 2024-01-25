using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TexasHoldemProtobuf;

public class Entry : UnitySingleton<Entry>
{
    ClientManager clientManager = null;
    RequestManager requestManager = null;

    public override void Awake()
    {
        base.Awake();

        /*gameObject.AddComponent<ILRuntimeManager>();
        gameObject.AddComponent<ABManager>();
        gameObject.AddComponent<UIManager>();*/
        clientManager = gameObject.AddComponent<ClientManager>();
        requestManager = gameObject.AddComponent<RequestManager>();
    }

    /// <summary>
    /// 顯示提示文本
    /// </summary>
    /// <param name="str">文本內容</param>
    public void ShowTip(string str)
    {
        Debug.Log($"提示:{str}");
        //uiManager.ShowTip(str);
    }

    private void OnDestroy()
    {
        clientManager.OnDestroy();
    }
}
