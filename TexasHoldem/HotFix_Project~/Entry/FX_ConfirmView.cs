using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace HotFix_Project
{
    class FX_ConfirmView :FX_BaseView
    {
        private static FX_BaseView thisView;

        private static Text Content_Txt;
        private static Button Cancel_Btn, Confirm_Btn;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Content_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Content_Txt");
            Cancel_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Cancel_Btn");
            Confirm_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Confirm_Btn");
        }

        private static void Start()
        {
            Cancel_Btn.onClick.AddListener(() =>
            {
                thisView.obj.SetActive(false);
            });
        }

        /// <summary>
        /// 設定確認視窗
        /// </summary>
        /// <param name="cimfirmCallBack"></param>
        /// <param name="str"></param>
        /// <param name="isHaveCancel"></param>
        private static void SetConfirmView(UnityAction cimfirmCallBack, string str, bool isHaveCancel = true)
        {
            Content_Txt.text = str;
            Cancel_Btn.gameObject.SetActive(isHaveCancel);
            Confirm_Btn.onClick.AddListener(() =>
            {
                cimfirmCallBack();
                thisView.obj.SetActive(false);
            });
        }
    }
}
