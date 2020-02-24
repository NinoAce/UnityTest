using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum VertHori
{
    Vertical,
    Horizontal
}
public delegate void SetData(GameObject obj);
/// <summary>
/// 空气格子和物体分开
/// 绑定在gameobject格子上，
/// </summary>
public abstract class ItemBaseController : MonoBehaviour {

    protected int InitCount;//init
    protected GameObject Perfab;//物体的预制体。init
    protected Rect PerRect;//物体的预制体的大小init
    protected VertHori VHState = VertHori.Vertical;//横还是竖
    protected int column;//列数或行数//init
    protected Vector2 PanelSize;//scroll view的大小。
    protected Vector2 spacing;//间隔
    Dictionary<int, Rect> DicItem;//存放格子。【和组件绑定的物体】
    protected HashSet<int> AppearID;//当前的显示的id
    protected List<int> NeedID;//需要显示的id
    protected GameObject[] CurrentObj;
    protected ScrollRect scrollRect;
    protected Rect RectMask;
    protected SetData setData;//每次出现该格子的时候要做的事

    
    int numbers = 0;
    /// <summary>
    /// 一开始创建几个位置InitCount
    /// 初始格子预制体Perfab
    /// 初始格子预制体的Rect PerRect
    /// </summary>
    public abstract void Init();

    private void Awake()
    {
        Init();
        DicItem = new Dictionary<int, Rect>();
        AppearID = new HashSet<int>();
       
        NeedID = new List<int>();
        //根据设置的总数创建空气格子

        scrollRect.onValueChanged.AddListener(OnUpate);
        StartCoroutine(OnInitItem(InitCount));
        
    }
    private void OnEnable()
    {
        this.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;//回归原点。
        OnShowDrag();
    }
    private void OnUpate(Vector2 arg0)
    {
        OnShowDrag();
    }
    //根据
    public void OnShowDrag()
    {
        if (VHState == VertHori.Vertical)
            RectMask.y = -PanelSize.y - GetComponent<RectTransform>().anchoredPosition.y + PerRect.height;//竖着的
        else
        {

            RectMask.x =  - GetComponent<RectTransform>().anchoredPosition.x;
           
        }
      
        UpdateNeedId();
    }
    /// <summary>
    /// 更新当前应该显示的id
    /// </summary>
    public void UpdateNeedId()
    {

        numbers = 0;
       foreach (KeyValuePair<int, Rect> item in DicItem)
     {
                if (item.Value.Overlaps(RectMask))
                {
             
                _UpdateChildTransformPos(CurrentObj[numbers], item.Key);
                if(numbers<CurrentObj.Length-1)
                   numbers++;
                }
           

       }
       

    }
    /// <summary>
    /// 创建空白的Rect格子
    /// </summary>
    /// <returns></returns>
    public IEnumerator OnInitItem(int num)
    {
        yield return null;
        if (VHState == VertHori.Vertical)
        {
            for (int i = 0; i < num; i++)
            {
                int i_row = i / column;
                int i_column = i % column;
                Rect dRect = new Rect(i_column * (spacing.x + PerRect.width) + spacing.x, -(i_row * (spacing.y + PerRect.height)) - spacing.y, PerRect.width, PerRect.height);
                //Debug.Log("空" + i + "：" + dRect);
                if (!DicItem.ContainsKey(i))
                {
                    DicItem.Add(i, dRect);
               
                }
                else
                {
                    DicItem[i] = dRect;
                }
            }
            InitChildWithVertical();
          
        }
        else
        {
            for (int i = 0; i < num; i++)
            {
                int i_row = i / column;
                int i_column = i % column;
                Rect dRect = new Rect(i_row * (spacing.x + PerRect.width) + spacing.x, -(i_column * (spacing.y + PerRect.height) + spacing.y), PerRect.width, PerRect.height);
                //Debug.Log("空" + i + "：" + dRect);
                if (!DicItem.ContainsKey(i))
                {
                    DicItem.Add(i, dRect);
                }
                else
                {
                    DicItem[i] = dRect;
                }

            }
             InitChildWithHorizontal();
        }

    }

    /// <summary>
    /// 初始真正的格子【垂直】
    /// </summary>
    public void InitChildWithVertical()
    {
        //蒙版的大小 wegiht：（间隔x+预制体的width）*列数    heigth：（间隔y+预制体的Hight）*（初始总数/列数）
        GetComponent<RectTransform>().sizeDelta = new Vector2(column * (spacing.x + PerRect.width) + spacing.x, (Mathf.CeilToInt((InitCount * 1.0f / column)) * (spacing.y + PerRect.height)));
        //自动调整scrollRect的大小，不想要可以去掉【不必要】
        scrollRect.GetComponent<RectTransform>().sizeDelta = new Vector2(column * (spacing.x + PerRect.width) + spacing.x, scrollRect.GetComponent<RectTransform>().rect.height);
    
        //舍弃下拉条【需要的话x加上】ScrollRect.verticalScrollbar.GetComponent<RectTransform>().rect.width
        //创建的时候创多一排用来实时循环
        int len = column * (Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().sizeDelta.y / (spacing.y + PerRect.height)+1) );

        CurrentObj = new GameObject[len];
        for (int i = 0; i < len; i++)
        {
            GameObject item = Instantiate(Perfab);
            //创建位置
            item.transform.parent = transform;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;
            if (item.GetComponent<Item>() == null)
            {
                item.AddComponent<Item>();
            }
            _UpdateChildTransformPos(item, i);
            CurrentObj[i] = item;
        }
        PanelSize = scrollRect.GetComponent<RectTransform>().sizeDelta;
        RectMask = new Rect(0, -PanelSize.y, PanelSize.x, PanelSize.y);//因为unity二维坐标向下是负数，所以需要扩大到下方 mMaskSize.y这么长
       //unity所有rect的xy都是左下角。判断的时候要注意
    }



    /// <summary>
    /// 初始化子物体【水平】
    /// </summary>
    private void InitChildWithHorizontal()
    {
        //（间隔x和格子的宽)
        GetComponent<RectTransform>().sizeDelta = new Vector2((Mathf.CeilToInt((InitCount * 1.0f / column)) * (spacing.x + PerRect.width)), column * (spacing.y + PerRect.height) + spacing.y);
        scrollRect.GetComponent<RectTransform>().sizeDelta = new Vector2(scrollRect.GetComponent<RectTransform>().rect.width, column * (spacing.y + PerRect.height) + spacing.y);//舍弃下拉条【需要的话y加上】 + ScrollRect.horizontalScrollbar.GetComponent<RectTransform>().rect.height
        //一个界面同时存在的格子数

        int len = column * (Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().sizeDelta.x / (spacing.x + PerRect.width)+1));
        CurrentObj = new GameObject[len];
        for (int i = 0; i < len; i++)
        {
            GameObject item = Instantiate(Perfab);

            //创建位置
            item.transform.parent = transform;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;

            if (item.GetComponent<Item>() == null)
            {
                item.AddComponent<Item>();
            }
            _UpdateChildTransformPos(item, i);
            CurrentObj[i] = item;
        }
        PanelSize = scrollRect.GetComponent<RectTransform>().sizeDelta;
        RectMask = new Rect(0, -PanelSize.y, PanelSize.x, PanelSize.y);
    }

    

    public void _UpdateChildTransformPos(GameObject obj,int i)
    {
        if(DicItem.ContainsKey(i))
        {
            obj.transform.localPosition = new Vector3( DicItem[i].x, DicItem[i].y,0);
            obj.GetComponent<Item>().ID = i;
            if(setData!=null)
            {
                setData(obj);
            }
        }
      

    }

    public void AddItem()
    {

        int i_row = InitCount / column;
        int i_column = InitCount % column;
        if (VHState == VertHori.Vertical)
        {
            Rect dRect = new Rect(i_column * (spacing.x + PerRect.width) + spacing.x, -(i_row * (spacing.y + PerRect.height)) - spacing.y, PerRect.width, PerRect.height);
            if (!DicItem.ContainsKey(InitCount))
            {
                DicItem.Add(InitCount, dRect);
            }
            else
            {
                DicItem[InitCount] = dRect;
            }
            GetComponent<RectTransform>().sizeDelta = new Vector2(column * (spacing.x + PerRect.width) + spacing.x, (Mathf.CeilToInt((InitCount * 1.0f / column)+1) * (spacing.y + PerRect.height)));
            InitCount++;
        }
        else
        {

            Rect dRect = new Rect(i_row * (spacing.x + PerRect.width) + spacing.x, -(i_column * (spacing.y + PerRect.height) + spacing.y), PerRect.width, PerRect.height);
            if (!DicItem.ContainsKey(InitCount))
            {
                DicItem.Add(InitCount, dRect);
            }
            else
            {
                DicItem[InitCount] = dRect;
            }
            GetComponent<RectTransform>().sizeDelta = new Vector2((Mathf.CeilToInt((InitCount * 1.0f / column)+1) * (spacing.x + PerRect.width)), column * (spacing.y + PerRect.height) + spacing.y);
            InitCount++;
        }
        OnShowDrag();
        
    }
    
    public void SubItem()
    {
        if(DicItem.ContainsKey(InitCount-1))
        {
            DicItem.Remove(InitCount - 1);
            numbers = 0;
            foreach (KeyValuePair<int, Rect> item in DicItem)
            {
                if (item.Value.Overlaps(RectMask))
                {
                    _UpdateChildTransformPos(CurrentObj[numbers], item.Key);
                    if (numbers < CurrentObj.Length - 1)
                        numbers++;
                }
            }
            for (int i = 0; i < CurrentObj.Length; i++)
            {
                if(CurrentObj[i].GetComponent<Item>().ID == InitCount - 1)
                {
                    _UpdateChildTransformPos(CurrentObj[i], InitCount - 2);
                   
                }
            }
             InitCount--;
            if (VHState == VertHori.Vertical)
            {
                GetComponent<RectTransform>().sizeDelta = new Vector2(column * (spacing.x + PerRect.width) + spacing.x, (Mathf.CeilToInt((InitCount * 1.0f / column) + 1) * (spacing.y + PerRect.height)));
            }
            else
            {
                GetComponent<RectTransform>().sizeDelta = new Vector2((Mathf.CeilToInt((InitCount * 1.0f / column) + 1) * (spacing.x + PerRect.width)), column * (spacing.y + PerRect.height) + spacing.y);

            }
           
        }
       
    }



}
