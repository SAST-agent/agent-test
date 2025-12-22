using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isInteraction : MonoBehaviour
{
    //监测玩家是否在与npc的交互中
    private bool isTalk = false;
    //交互时禁用移动
    private Cainos.PixelArtTopDown_Basic.TopDownCharacterController move;
    //监测玩家是否处于游戏应处于暂停状态的交互中
    private bool isPaused = false;
    //监测玩家是否处于查看物品信息的交互中
    private bool isSubmit = false;

    void Start()
    {
        move = gameObject.GetComponent<Cainos.PixelArtTopDown_Basic.TopDownCharacterController>();
    }

    void Update()
    {
        if(isTalk || isPaused)
        {
            move.enabled = false;
        }
        else
        {
            move.enabled = true;
        }
    }

    //设置对外接口
    public bool getIsTalk()
    {
        return isTalk;
    }

    public void changeIsTalk(bool inPut)
    {
        isTalk = inPut;
    }

    public bool getIsPaused()
    {
        return isPaused;
    }

    public void changeIsPaused(bool inPut)
    {
        isPaused = inPut || isSubmit;
    }

    public bool getIsSubmit()
    {
        return isSubmit;
    }

    public void changeIsSubmit(bool inPut)
    {
        isSubmit = inPut;
    }
}
