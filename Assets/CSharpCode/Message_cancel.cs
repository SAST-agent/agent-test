using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Message_cancel : MonoBehaviour
{
    //获取物品信息框
    public GameObject messagePanel;
    //player
    public GameObject player;
    //监测玩家player是否在交互中
    private isInteraction i;
    void Start()
    {
        i = player.GetComponent<isInteraction>();
    }

    void Update()
    {
        
    }
    public void messagePanelCancel()
    {
        i.changeIsPaused(false);
        messagePanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

}
