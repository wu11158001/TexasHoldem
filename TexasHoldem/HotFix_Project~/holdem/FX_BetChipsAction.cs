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

        private static float distanceX = 350;
        private static float distanceY = 120;

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
                    if ((DateTime.Now - move.Value.Item1).TotalSeconds >= 4)
                    {
                        GameObject.Destroy(move.Key.gameObject);
                        moveDic.Remove(move.Key);
                        return;
                    }

                    //向目標點移動
                    if ((move.Key.position.x > PointTarget.position.x + distanceX || move.Key.position.x < PointTarget.position.x - distanceX) ||
                        (move.Key.position.y > PointTarget.position.y + distanceY || move.Key.position.y < PointTarget.position.y - distanceY))
                    {
                        float progress = (Time.time - move.Value.Item2) / 40;
                        float posX = Mathf.Lerp(move.Key.position.x, PointTarget.position.x, progress);
                        float posY = Mathf.Lerp(move.Key.position.y, PointTarget.position.y, progress);
                        move.Key.position = new Vector2(posX, posY);
                    }
                    else
                    {
                        move.Value.Item3.gameObject.SetActive(true);
                    }
                }
            }            
        }

        /// <summary>
        /// 設定目標
        /// </summary>
        /// <param name="target"></param>
        private static void SetPointTarget(RectTransform target)
        {
            PointTarget = target;           
        }

        /// <summary>
        /// 設定籌碼值
        /// </summary>
        /// <param name="value"></param>
        private static void SetChipsValue(string value, Transform pointTarget)
        {
            Transform BetValue_Tr = FindConponent.FindObj<RectTransform>(thisView.view.transform, "BetValue_Tr");
            BetValue_Tr.gameObject.SetActive(false);

            PointTarget = pointTarget;
            BetValue_Txt.text = value;
            moveDic.Add(thisView.obj.transform, (DateTime.Now, Time.time, BetValue_Tr));
        }
    }
}
