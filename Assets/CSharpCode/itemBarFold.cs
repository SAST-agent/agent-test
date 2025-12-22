using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class itemBarfold : MonoBehaviour
{
    //显示物品栏时，Fold的位置
    //未显示物品栏时，Fold的位置
    //获取物品信息框
    public GameObject itemPanel;
    private GameObject itemName;//子物体文字
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void foldPanel()
    {
        //初始化itemName并获取文字内容
        itemName = transform.GetChild(0).gameObject;
        TextMeshProUGUI buttonText = itemName.GetComponent<TextMeshProUGUI>();
        //获取 RectTransform
        RectTransform rt = transform.GetComponent<RectTransform>(); 
        //"<"代表显示物品栏
        if (buttonText.text == "<")
        {
            itemPanel.SetActive(false);
            rt.anchoredPosition = new Vector2(-382, -21);
            buttonText.text = ">";
        }
        //未显示物品栏
        else
        {
            itemPanel.SetActive(true);
            rt.anchoredPosition = new Vector2(-335, -21);
            buttonText.text = "<";
        }
        EventSystem.current.SetSelectedGameObject(null);
    }
}
