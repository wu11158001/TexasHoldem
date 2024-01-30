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
        private static FX_BaseView thisView;

        private static DateTime startTime;
        private static TimeSpan elapsedTime;

        private static ViewType showView;
        private static bool isShowNext;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);
        }

        private static void Update()
        {
            elapsedTime = DateTime.Now - startTime;
            if (elapsedTime.TotalSeconds > 2 && !isShowNext)
            {
                isShowNext = true;
                UIManager.Instance.ShowView(showView);
            }
        }

        /// <summary>
        /// 開啟載入
        /// </summary>
        /// <param name="nextView"></param>
        private static void OpenLoading(ViewType nextView)
        {
            isShowNext = false;
            startTime = DateTime.Now;
            showView = nextView;
        }
    }
}
