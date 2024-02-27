using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix_Project
{
    class FX_SideBackAction : FX_BaseView
    {
        private static FX_BaseView thisView;

        /// <summary>
        ///  紀錄移動物件(物件, (移除時間, 移動時間, 文字物件, 初始位置Y))
        /// </summary>
        private static Dictionary<Transform, (DateTime, float, Text, float)> moveDic = new Dictionary<Transform, (DateTime, float, Text, float)>();

        private static Text SideBack_Txt;

        private static float showTime = 2.0f;
        private static float moveHeight = 100.0f;
        private static float initPosY;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            SideBack_Txt = thisView.obj.GetComponent<Text>();
        }

        private static void Update()
        {
            if (moveDic.Count() > 0)
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

                    float progress = (float)((DateTime.Now - move.Value.Item1).TotalSeconds / showTime);
                    float posX = move.Key.localPosition.x;
                    float posY = Mathf.Lerp(move.Value.Item4, move.Value.Item4 + moveHeight, progress);
                    move.Key.localPosition = new Vector2(posX, posY);

                    Color color = move.Value.Item3.color;
                    color.a = Mathf.Lerp(1, 0, progress - 0.65f);
                    move.Value.Item3.color = color;
                }
            }
        }

        /// <summary>
        /// 設定退回邊池訊息
        /// </summary>
        /// <param name="value"></param>
        private static void SetSideBackInfo(string value)
        {
            SideBack_Txt.text = $"籌碼退回:{value}";
            initPosY = thisView.obj.transform.localPosition.y;
            moveDic.Add(thisView.obj.transform, (DateTime.Now, Time.time, SideBack_Txt, initPosY));
        }
    }
}
