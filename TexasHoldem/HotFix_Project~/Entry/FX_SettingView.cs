using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix_Project
{
    class FX_SettingView : FX_BaseView
    {
        private static FX_BaseView thisView;

        private static Button Mask_Btn, Close_Btn, Music_Btn, Sound_Btn;
        private static Image Music_Img, Sound_Img;
        private static Slider Music_Sl, Sound_Sl;
        private static Text Music_Txt, Sound_Txt;

        private static Sprite[] soundList;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Mask_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Mask_Btn");
            Close_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Close_Btn");
            Music_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Music_Btn");
            Sound_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Sound_Btn");
            Music_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Music_Btn");
            Sound_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Sound_Btn");
            Music_Sl = FindConponent.FindObj<Slider>(thisView.view.transform, "Music_Sl");
            Sound_Sl = FindConponent.FindObj<Slider>(thisView.view.transform, "Sound_Sl");
            Music_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Music_Txt");
            Sound_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Sound_Txt");

            ABManager.Instance.LoadSprite("entry", "Sound", (sound) =>
            {
                soundList = sound;                
            });
        }

        private static void OnEnable()
        {
            SetShow();
        }

        private static void Start()
        {
            Mask_Btn.onClick.AddListener(() =>
            {
                Close();
            });

            Close_Btn.onClick.AddListener(() =>
            {
                Close();
            });

            Music_Btn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance.MusicVolume > 0)
                {
                    AudioManager.Instance.TempMusic = AudioManager.Instance.MusicVolume;
                    AudioManager.Instance.MusicVolume = 0;
                    Music_Img.sprite = soundList[1];
                }
                else
                {
                    AudioManager.Instance.MusicVolume = AudioManager.Instance.TempMusic;
                    Music_Img.sprite = soundList[0];
                }

                SetShow();
            });

            Sound_Btn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance.SoundVolume > 0)
                {
                    AudioManager.Instance.TempSound = AudioManager.Instance.SoundVolume;
                    AudioManager.Instance.SoundVolume = 0;
                    Sound_Img.sprite = soundList[3];
                }
                else
                {
                    AudioManager.Instance.SoundVolume = AudioManager.Instance.TempSound;
                    Sound_Img.sprite = soundList[2];
                }

                SetShow();
            });

            Music_Sl.onValueChanged.AddListener((volume) =>
            {
                AudioManager.Instance.MusicVolume = volume;
                SetShow();
            });

            Sound_Sl.onValueChanged.AddListener((volume) =>
            {
                AudioManager.Instance.SoundVolume = volume;
                SetShow();
            });
        }

        //設置顯示
        private static void SetShow()
        {
            Music_Sl.value = AudioManager.Instance.MusicVolume;
            Sound_Sl.value = AudioManager.Instance.SoundVolume;

            Music_Txt.text = $"{(AudioManager.Instance.MusicVolume * 100).ToString("F0")}%";
            Sound_Txt.text = $"{(AudioManager.Instance.SoundVolume * 100).ToString("F0")}%";

            if (soundList != null && soundList.Length > 0)
            {
                Music_Img.sprite = AudioManager.Instance.MusicVolume == 0 ? soundList[1] : soundList[0];
                Sound_Img.sprite = AudioManager.Instance.SoundVolume == 0 ? soundList[3] : soundList[2];
            }
        }

        /// <summary>
        /// 關閉View
        /// </summary>
        private static void Close()
        {
            thisView.obj.SetActive(false);
        }
    }
}
