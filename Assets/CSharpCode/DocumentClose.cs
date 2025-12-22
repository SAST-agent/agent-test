using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DocumentClose : MonoBehaviour
{
    private GameObject document;
    private eventBoardFold fold;
    //player
    public GameObject player;
    //监测玩家player是否在交互中
    private isInteraction i;
    void Start()
    {
        document = transform.parent.gameObject;
        fold = GameObject.Find("Fold2").GetComponent<eventBoardFold>();
        i = player.GetComponent<isInteraction>();
    }

    void Update()
    {
        
    }

    //关闭记事板
    public void close()
    {
        i.changeIsPaused(false);
        fold.changeFold();
        document.SetActive(false);
    }
}

