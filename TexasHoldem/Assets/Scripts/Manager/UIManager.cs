using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class UIManager : UnitySingleton<UIManager>
{
    private Transform canvas_Tr;

    private Dictionary<ViewType, BaseView> viewDic = new Dictionary<ViewType, BaseView>();
    private Stack<BaseView> viewStack = new Stack<BaseView>();

    //工具類View
    private ViewType[] toolsViewType = new ViewType[] 
    {
        ViewType.TipView, 
        ViewType.LoadingView,
        ViewType.WaitView
    };
    private Dictionary<ViewType, BaseView> toolsViewDic = new Dictionary<ViewType, BaseView>();

    public override void Awake()
    {
        base.Awake();

        canvas_Tr = GameObject.FindObjectOfType<Canvas>().transform;  
    }

    /// <summary>
    /// 創建工具View
    /// </summary>
    async public Task CreateToolsView()
    {
        for (int i = 0; i < toolsViewType.Length; i++)
        {
            BaseView view = await CreatePanel(toolsViewType[i]);
            InitView(view, toolsViewType[i]);
            toolsViewDic.Add(toolsViewType[i], view);

            await Task.Delay(1);
        }
    }

    /// <summary>
    /// 顯示工具View
    /// </summary>
    /// <param name="viewType"></param>
    /// <returns></returns>
    private BaseView ShowToolView(ViewType viewType)
    {
        BaseView view = null;
        if (toolsViewDic.ContainsKey(viewType))
        {
            view = toolsViewDic[viewType];
            view.gameObject.SetActive(true);
            view.gameObject.transform.SetSiblingIndex(100);
        }

        return view;
    }

    /// <summary>
    /// 顯示提示
    /// </summary>
    /// <param name="str"></param>
    public void ShowTip(string str)
    {
        BaseView view = ShowToolView(ViewType.TipView);
        if (view != null)
        {
            ((TipView)view).ShowTip(str);
        }      
    }

    /// <summary>
    /// 顯示載入畫面
    /// </summary>
    /// <param name="nextView"></param>
    public void ShowLoadingView(ViewType nextView)
    {
        BaseView view = ShowToolView(ViewType.LoadingView);
        if (view != null)
        {
            ((LoadingView)view).SetNextView(nextView);
        }
    }

    /// <summary>
    /// 顯示View
    /// </summary>
    /// <param name="viewType"></param>
    /// <returns></returns>
    async public Task<BaseView> ShowView(ViewType viewType)
    {
        BaseView view = null;
        if (viewDic.ContainsKey(viewType))
        {
            ViewStackPop();
            view = viewDic[viewType];
        }
        else
        {
            ViewStackPop();
            view = await CreatePanel(viewType);
            InitView(view, viewType);
        }

        view.gameObject.SetActive(true);
        viewStack.Push(view);
        return view;
    }

    //初始化View
    private void InitView(BaseView view, ViewType viewType)
    {
        view.gameObject.SetActive(false);
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
    private void ViewStackPop()
    {
        if (viewStack.Count > 0)
        {
            BaseView topPanel = viewStack.Pop();
            topPanel.gameObject.SetActive(false);
        }
    }
}
