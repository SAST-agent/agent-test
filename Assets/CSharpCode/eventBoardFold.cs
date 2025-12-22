using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eventBoardFold : MonoBehaviour
{
    //记事板的收起、折叠
    public GameObject documentPanel;
    private bool isFold = true;
    //player
    public GameObject player;
    //监测玩家player是否在交互中
    private isInteraction i;
    void Start()
    {
        i = player.GetComponent<isInteraction>();
    }

    public void documentBoard()
    {
        if(isFold)
        {
            isFold = false;
            documentPanel.SetActive(true);
            i.changeIsPaused(true);
        }
    }

    public void changeFold()
    {
        isFold = true;
    }
}
