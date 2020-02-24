using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoodController : ItemBaseController
{
    private Transform canvas;
    private Button add;
    private Button sub;
    /// <summary>
    /// 重写
    /// </summary>
    public override void Init()
    {
        InitCount = 100;
        Perfab = Resources.Load<GameObject>("item");
        PerRect = Perfab.GetComponent<RectTransform>().rect;
        column = 3;
        scrollRect = transform.parent.GetComponent<ScrollRect>();
        PanelSize = scrollRect.GetComponent<RectTransform>().sizeDelta;
        spacing = new Vector2(10f, 10f);
        VHState = VertHori.Vertical;
        setData = InitImageItem;//更新时会做的事
        scrollRect.inertia = false;//惯性去掉

    }

    private void Start()
    {
        canvas = GameObject.Find("Canvas").transform;
        add = canvas.Find("Add").GetComponent<Button>();
        sub = canvas.Find("Sub").GetComponent<Button>();
        add.onClick.AddListener(OnAdd);
        sub.onClick.AddListener(OnSub);
    }

    private void OnAdd()
    {
        AddItem();
    }

    private void OnSub()
    {
        SubItem();
    }

    public void InitImageItem(GameObject item)
    {
        if(item.GetComponent<Button>()==null)
        {
            item.AddComponent<Button>();
            item.GetComponent<Button>().onClick.AddListener(delegate { OnRun(item); });
        }

      
    }

    private void OnRun(GameObject item)
    {
        Debug.Log(item.GetComponent<Item>().ID);
    }

 

}
