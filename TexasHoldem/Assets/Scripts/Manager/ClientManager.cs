using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TexasHoldemProtobuf;
using System.Net.Sockets;
using System;

public class ClientManager : UnitySingleton<ClientManager>
{
    private string ip = "127.0.0.1";
    private const int port = 5555;

    private Socket socket = null;
    private Message message = null;
    
    public override void Awake()
    {
        base.Awake();

        message = new Message();
        InitSocket();
    }

    /// <summary>
    /// 初始化連接
    /// </summary>
    void InitSocket()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(ip, port);
            //開始接收訊息
            StartReceive();
            Entry.Instance.ShowTip("連接成功");
        }
        catch (Exception e)
        {
            Debug.LogWarning("連接失敗:" + e);
        }
    }

    /// <summary>
    /// 開始接收訊息
    /// </summary>
    void StartReceive()
    {
        socket.BeginReceive(message.GetBuffer, message.GetStartIndex, message.GetRemSize, SocketFlags.None, ReceiveCallBack, null);
    }

    /// <summary>
    /// 接收訊息CallBack
    /// </summary>
    /// <param name="iar"></param>
    void ReceiveCallBack(IAsyncResult iar)
    {
        try
        {
            if (socket == null || !socket.Connected) return;

            int len = socket.EndReceive(iar);
            if (len == 0)
            {
                //關閉連接                
                CloseSocket();
                return;
            }

            message.ReadBuffer(len, HandleResponse);
            //重新開始接收訊息
            StartReceive();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 處理回覆
    /// </summary>
    /// <param name="pack"></param>
    void HandleResponse(MainPack pack)
    {
        RequestManager.Instance.HandleResponse(pack);
    }

    /// <summary>
    /// 發送訊息
    /// </summary>
    /// <param name="pack"></param>
    public void Send(MainPack pack)
    {
        socket.Send(Message.PackData(pack));
    }

    /// <summary>
    /// 關閉連接
    /// </summary>
    void CloseSocket()
    {
        if (socket.Connected && socket != null)
        {
            Debug.Log("關閉連接");
            socket.Close();
        }
    }

    public void OnDestroy()
    {
        message = null;

        //關閉連接        
        CloseSocket();
    }
}
