using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix_Project
{
    class FX_LoadingView : FX_BaseView
    {
        private static FX_BaseView thisView;

        private static DateTime startTime;
        private static GameObject showView;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);
        }

        private static void Update()
        {
            if ((DateTime.Now - startTime).TotalSeconds > 1.5f && showView != null)
            {                
                showView.SetActive(true);
                thisView.obj.SetActive(false);
            }
        }

        /// <summary>
        /// 開啟載入畫面
        /// </summary>
        private static void OpenLoading()
        {
            showView = null;
            startTime = DateTime.Now;
        }

        /// <summary>
        /// 關閉載入畫面
        /// </summary>
        private static void CloseLoading(GameObject nextView)
        {
            showView = nextView;
        }
    }
}
