using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipView : BaseView
{
    private GameObject TipSample;
    private List<TipContentView> tipList = new List<TipContentView>();

    public override void Awake()
    {
        base.Awake();

        TipSample = FindConponent.FindObj<Transform>(transform, "TipSample").gameObject;
        TipSample.gameObject.SetActive(false);
    }

    /// <summary>
    /// 顯示提示
    /// </summary>
    /// <param name="str"></param>
    public void ShowTip(string str)
    {
        TipContentView tipContentView = null;
        if (tipList.Count == 0)
        {
            tipContentView = Instantiate(TipSample).GetComponent<TipContentView>();
        }
        else
        {
            for (int i = 0; i < tipList.Count; i++)
            {
                if (!tipList[i].gameObject.activeSelf)
                {
                    tipContentView = tipList[i];
                    break;
                }
            }

            if (tipContentView == null)
            {
                tipContentView = Instantiate(TipSample).GetComponent<TipContentView>();
            } 
        }

        if (tipContentView != null)
        {
            tipContentView.transform.SetParent(transform);
            tipContentView.gameObject.SetActive(true);
            tipContentView.Show(str, this);
            tipList.Add(tipContentView);
        }
    }
}
