using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix_Project.View
{
    class FX_ModeView :FX_BaseView
    {
        private static FX_BaseView thisView;

        private static Button Holdem_Btn;
        private static GameObject Download_Obj, DownLoadProgress_Obj;
        private static Image Progress_Img;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Holdem_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Holdem_Btn");
            DownLoadProgress_Obj = FindConponent.FindObj<Transform>(thisView.view.transform, "DownLoadProgress_Obj").gameObject;
            DownLoadProgress_Obj.SetActive(false);
            Download_Obj = FindConponent.FindObj<Transform>(thisView.view.transform, "Download_Obj").gameObject;
            Download_Obj.SetActive(!ABManager.Instance.CheckAb("holdem"));
            Progress_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Progress_Img");            
        }

        private static void Start()
        {
            Holdem_Btn.onClick.AddListener(() =>
            {
                string abName = "holdem";
                bool isDownLoad = ABManager.Instance.CheckAb(abName);
                Download_Obj.SetActive(isDownLoad);

                if (isDownLoad)
                {
                    UIManager.Instance.ShowView(ViewType.LobbyView);
                }
                else
                {
                    ABManager.Instance.GetABSize(abName, CheckResources);
                }                
            });
        }

        /// <summary>
        /// 檢測資源
        /// </summary>
        /// <param name="size"></param>
        private static void CheckResources(long size)
        {
            Debug.Log($"下載資源大小:" + size);
            DownLoadProgress_Obj.SetActive(true);

            UIManager.Instance.ShowConfirmView(() =>
            {
                ABManager.Instance.LoadAb("holdem", DownloadProgress);
            },
            $"下載熱更資源: {(size / 1000.0f).ToString("F1")} M"
            );
        }

        /// <summary>
        /// 下載進度
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="finishedCallBack"></param>
        private static void DownloadProgress(float progress)
        {
            Debug.Log($"下載進度:{progress}%");
            Progress_Img.fillAmount = progress;
            if (progress == 100)
            {
                DownLoadProgress_Obj.SetActive(false);
            }
        }
    }
}
