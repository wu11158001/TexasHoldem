using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class UIManager : UnitySingleton<UIManager>
{
    private Transform canvas_Tr;

    private Dictionary<ViewType, BaseView> viewDic = new Dictionary<ViewType, BaseView>();
    private Stack<BaseView> viewStack = new Stack<BaseView>();

    //提示View
    private BaseView tipView;

    public override void Awake()
    {
        base.Awake();

        canvas_Tr = GameObject.FindObjectOfType<Canvas>().transform;  
    }

    /// <summary>
    /// 顯示提示
    /// </summary>
    /// <param name="str"></param>
    async public void ShowTip(string str)
    {
        if (tipView == null)
        {
            tipView = await CreatePanel(ViewType.TipView);
            InitView(tipView, ViewType.TipView);
            ((TipView)tipView).ShowTip(str);
        }
        else
        {
            ((TipView)tipView).ShowTip(str);
        }        
    }

    /// <summary>
    /// 顯示View
    /// </summary>
    /// <param name="viewType"></param>
    /// <returns></returns>
    async public Task<BaseView> ShowView(ViewType viewType)
    {
        if (viewDic.ContainsKey(viewType))
        {
            ViewStackPeek();
            BaseView view = viewDic[viewType];
            return view;
        }
        else
        {
            ViewStackPeek();
            BaseView view = await CreatePanel(viewType);
            InitView(view, viewType);
            return view;
        }      
    }

    //初始化View
    private void InitView(BaseView view, ViewType viewType)
    {
        viewStack.Push(view);
        view.gameObject.SetActive(true);
        view.name = viewType.ToString();
        RectTransform rt = view.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.eulerAngles = Vector3.zero;
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    /// <summary>
    /// 創建View
    /// </summary>
    /// <param name="viewType"></param>
    /// <returns></returns>
    async private Task<BaseView> CreatePanel(ViewType viewType)
    {
        if (viewDic.ContainsKey(viewType))
        { 
            return viewDic[viewType];
        }
        else
        {
            TaskCompletionSource<BaseView> tcs = new TaskCompletionSource<BaseView>();
            ABManager.Instance.GetABRes<GameObject>("view", viewType.ToString(), (obj) =>
            {
                GameObject viewObj = Instantiate(obj);
                viewObj.transform.SetParent(canvas_Tr);
                viewObj.SetActive(false);
                BaseView baseView = viewObj.GetComponent<BaseView>();
                viewDic.Add(viewType, baseView);

                tcs.SetResult(baseView);
            });

            return await tcs.Task;
        }
    }

    /// <summary>
    /// 關閉前個View
    /// </summary>
    private void ViewStackPeek()
    {
        if (viewStack.Count > 0)
        {
            BaseView topPanel = viewStack.Peek();
            topPanel.gameObject.SetActive(false);
        }
    }
}
