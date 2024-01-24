using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : UnitySingleton<UIManager>
{
    private Transform canvas_Tr;

    public override void Awake()
    {
        base.Awake();

        canvas_Tr = GameObject.FindObjectOfType<Canvas>().transform;
    }

    /// <summary>
    /// 創建面板
    /// </summary>
    /// <param name="panelType"></param>
    /// <returns></returns>
    public GameObject CreatePanel(PanelType panelType)
    {
        GameObject panel = null;

        ABManager.Instance.GetABRes<GameObject>("panel", panelType.ToString(), (obj) =>
        {
            panel = Instantiate(obj);
            panel.transform.SetParent(canvas_Tr);
        });

        return panel;
    }
}
