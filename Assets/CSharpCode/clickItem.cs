using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class item_InBar : MonoBehaviour
{
    //获取信息弹窗对象（直接挂载上去）
    public GameObject messagePanel;
    private GameObject itemName;//子物体文字
    private GameObject player;
    //监测玩家player是否在交互中
    private isInteraction i;
    //显示哪个物品的信息
    private int childNum;
    //具体显示信息
    public string message;
    //颜色选择
    private static Color[] selectColor = { Color.white, new Color(1f, 1f, 0.6f, 1f) };
    public bool isSelect = false;
    //点击模式切换
    private int mode = 0;

    void Start()
    {
        //判断玩家是否在交互
        player = GameObject.Find("PF Player");
        i = player.GetComponent<isInteraction>();

        Button button = transform.GetComponent<Button>();
        button.onClick.AddListener(ifClick);

        // 防止 Unity 自带 Transition 干扰颜色控制
        button.transition = Selectable.Transition.None;
    }

    private void ifClick()
    {
        switch(mode)
        {
            case 0:
                showMessagePanel();
                break;
            case 1:
                changeColor();
                break;
        }
    }
    private void showMessagePanel()
    {
        //若物品信息窗已被弹出，直接返回
        if(messagePanel.activeSelf)
        {
            return;
        }
        //当玩家处于应暂停的游戏状态，且此时不需要提供证据时，直接返回
        if(i.getIsPaused()) return;

        i.changeIsPaused(true);

        //初始化物品信息
        TextMeshProUGUI itemMessage = messagePanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        itemMessage.text = message;

        messagePanel.SetActive(true);  //点击按钮后显示弹窗
        
    }

    private void changeColor()
    {
        Debug.Log("should change color");
        isSelect = !isSelect;
        transform.GetComponent<Image>().color = selectColor[isSelect ? 1 : 0];
    }

    public void changeMode(int inputMode)
    {
        mode = inputMode;
    }
}
