using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnswerActivate : MonoBehaviour
{
    public GameObject AnswerSubmit;  // 拖入 MenuPanel
    //player
    public GameObject player;
    //监测玩家player是否在交互中
    private isInteraction i;
    private bool isOpen = false;

    private void Start()
    {
        i = player.GetComponent<isInteraction>();
    }

    public void MenuStart()
    {
        isOpen = !isOpen;

        i.changeIsPaused(isOpen);
        AnswerSubmit.SetActive(isOpen);

        EventSystem.current.SetSelectedGameObject(null);
    }
}