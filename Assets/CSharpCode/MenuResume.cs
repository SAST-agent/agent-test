using UnityEngine;

public class MenuResume : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelToClose;

    [Header("Player")]
    public isInteraction playerInteraction; //拖 Player 上的那个组件

    public void Continue()
    {
        // 1. 关闭面板
        if (panelToClose != null)
            panelToClose.SetActive(false);

        // 2. 恢复玩家交互
        if (playerInteraction != null)
            playerInteraction.changeIsSubmit(false);
        else
            Debug.LogWarning("MenuResume: playerInteraction is NULL");
    }
}
