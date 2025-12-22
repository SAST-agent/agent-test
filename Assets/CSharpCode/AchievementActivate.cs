using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AchievementActivate : MonoBehaviour
{
    public GameObject AchievementPanel;  // 拖入 MenuPanel
    //player
    public GameObject player;
    //监测玩家player是否在交互中
    private isInteraction i;
    private bool isOpen = false;

    private void Start()
    {
        i = player.GetComponent<isInteraction>();
    }

    //public void MenuStart()
    //{
    //    isOpen = !isOpen;

    //    i.changeIsPaused(isOpen);
    //    AchievementPanel.SetActive(isOpen);

    //    EventSystem.current.SetSelectedGameObject(null);
    //}

    /// <summary>
    /// 用于“查看成就”按钮：明确打开
    /// </summary>
    public void OpenAchievement()
    {
        isOpen = true;
        ApplyState();
    }

    private void ApplyState()
    {
        i.changeIsPaused(isOpen);
        AchievementPanel.SetActive(isOpen);
        EventSystem.current.SetSelectedGameObject(null);
    }
}
