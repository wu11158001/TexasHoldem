using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TexasHoldemProtobuf;
using Google.Protobuf;

public class Message
{
    private byte[] buffer = new byte[1024];
    public byte[] GetBuffer { get { return buffer; } }

    private int startIndex;
    public int GetStartIndex { get { return startIndex; } }

    public int GetRemSize { get { return buffer.Length - startIndex; } }

    /// <summary>
    /// 解析訊息
    /// </summary>
    /// <param name="len">訊息長度</param>
    /// <param name="handleRequest">回傳方法</param>
    public void ReadBuffer(int len, Action<MainPack> HandleResponse)
    {
        startIndex += len;

        while (true)
        {
            //訊息不完整
            if (startIndex <= 4) return;

            int count = BitConverter.ToInt32(buffer, 0);
            if (startIndex >= count + 4)
            {
                MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 4, count);
                //回傳方法
                HandleResponse(pack);

                Array.Copy(buffer, count + 4, buffer, 0, startIndex - count - 4);
                startIndex -= count + 4;
            }
            else break;
        }
    }

    /// <summary>
    /// 打包
    /// </summary>
    /// <param name="pack"></param>
    /// <returns></returns>
    public static byte[] PackData(MainPack pack)
    {
        //包體
        byte[] data = pack.ToByteArray();
        //包頭
        byte[] head = BitConverter.GetBytes(data.Length);

        return head.Concat(data).ToArray();
    }

    /// <summary>
    /// 打包
    /// </summary>
    /// <param name="pack"></param>
    /// <returns></returns>
    public static byte[] PackDataUDP(MainPack pack)
    {
        return pack.ToByteArray();
    }
}
