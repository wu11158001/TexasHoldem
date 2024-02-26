using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TexasHoldemProtobuf;

namespace HotFix_Project
{
    class FX_BetChipsAction : FX_BaseView
    {
        private static FX_BaseView thisView;

        private static Transform PointTarget;
        private static Text BetValue_Txt;

        private static float showTime = 1.5f;
        private static float distanceX = 400;
        private static float distanceY = 210;

        //紀錄移動物件(物件, (移除時間, 移動時間, 文字物件))
        private static Dictionary<Transform, (DateTime, float, Transform)> moveDic = new Dictionary<Transform, (DateTime, float, Transform)>();

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            BetValue_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "BetValue_Txt");
        }

        private static void Update()
        {
            if (moveDic.Count > 0)
            {
                foreach (var move in moveDic)
                {
                    //顯示時間
                    if ((DateTime.Now - move.Value.Item1).TotalSeconds >= showTime)
                    {
                        GameObject.Destroy(move.Key.gameObject);
                        moveDic.Remove(move.Key);
                        return;
                    }

                    //向目標點移動
                    if ((move.Key.localPosition.x > PointTarget.localPosition.x + distanceX || move.Key.localPosition.x < PointTarget.localPosition.x - distanceX) ||
                        (move.Key.localPosition.y > PointTarget.localPosition.y + distanceY || move.Key.localPosition.y < PointTarget.localPosition.y - distanceY))
                    {
                        float progress = (Time.time - move.Value.Item2) / 40;
                        float posX = Mathf.Lerp(move.Key.localPosition.x, PointTarget.localPosition.x, progress);
                        float posY = Mathf.Lerp(move.Key.localPosition.y, PointTarget.localPosition.y, progress);
                        move.Key.localPosition = new Vector2(posX, posY);
                    }
                    else
                    {
                        move.Value.Item3.gameObject.SetActive(true);
                    }
                }
            }            
        }

        /// <summary>
        /// 設定籌碼值
        /// </summary>
        /// <param name="value"></param>
        private static void SetChipsValue(string value, Transform pointTarget)
        {
            PointTarget = pointTarget;
            BetValue_Txt.text = Utils.Instance.SetChipsStr(value);
            BetValue_Txt.gameObject.SetActive(false);
            moveDic.Add(thisView.obj.transform, (DateTime.Now, Time.time, BetValue_Txt.transform));
        }
    }
}
