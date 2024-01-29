using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;

public static class AsyncFunc
{
    /// <summary>
    /// 等待執行
    /// </summary>
    /// <param name="delayTime"></param>
    /// <param name="callback"></param>
    async public static void DelayFunc(int delayTime, UnityAction callback)
    {
        await Task.Delay(delayTime);
        callback();
    }
}
