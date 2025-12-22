using UnityEngine;
using UnityEngine.EventSystems;

public class MenuActivate : MonoBehaviour
{
    public GameObject MenuPanel;
    public GameObject player;

    private isInteraction i;
    private bool isOpen = false;

    private void Start()
    {
        i = player.GetComponent<isInteraction>();
    }

    /// <summary>
    /// 用于 ESC / 菜单键：切换菜单
    /// </summary>
    public void MenuStart()
    {
        isOpen = !isOpen;
        ApplyState();
    }

    /// <summary>
    /// 用于 Continue：明确继续游戏
    /// </summary>
    public void ContinueGame()
    {
        isOpen = false;
        ApplyState();
    }

    private void ApplyState()
    {
        i.changeIsPaused(isOpen);
        MenuPanel.SetActive(isOpen);
        EventSystem.current.SetSelectedGameObject(null);
    }
}
