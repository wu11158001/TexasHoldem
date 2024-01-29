using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix_Project.View
{
    class FX_LoadingView : FX_BaseView
    {
        private static DateTime startTime;
        private static TimeSpan elapsedTime;

        private static ViewType showView;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            new FX_BaseView().SetObj(baseView, viewObj);
        }

        private static void Update()
        {
            elapsedTime = DateTime.Now - startTime;
            if (elapsedTime.TotalSeconds > 2 && obj.activeSelf)
            {
                UIManager.Instance.ShowView(showView);
                obj.SetActive(false);
            }
        }

        /// <summary>
        /// 設定下個View
        /// </summary>
        /// <param name="nextView"></param>
        private static void SetNextView(ViewType nextView)
        {
            startTime = DateTime.Now;
            showView = nextView;
        }
    }
}
