using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class inDialogue : MonoBehaviour
{
    //player
    public GameObject player;
    //监测玩家player是否在交互中
    private isInteraction i;
    //控制输入弹窗
    public GameObject playerDialogue;
    //控制物品提交弹窗
    public GameObject evidencePanel;
    //检测是否仅提供线索
    public bool isClueOnly;
    //npc
    private string npcName;
    void Start()
    {
        GameObject inputPlace = playerDialogue.transform.GetChild(0).GetChild(0).GetChild(1).gameObject;
        i = player.GetComponent<isInteraction>();
    }

    void Update()
    {
       //若正处于应暂停的交互中
       if(i.getIsPaused() || evidencePanel.activeSelf || playerDialogue.activeSelf)
        {
            return;
        }
        //按下空格，退出对话
        if(Input.GetKeyUp(KeyCode.Space))
        {
            Button exitInteraction = transform.GetChild(2).GetComponent<Button>();
            exitInteraction.onClick.Invoke();
        }
        //按下enter，继续对话
        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (playerDialogue.activeSelf || evidencePanel.activeSelf)
                return;
            continueDialogue(); 
        }
    }

    void continueDialogue()
    {
        i.changeIsPaused(true);
        i.changeIsTalk(true);
        playerDialogue.SetActive(true);

        GameObject inputField = playerDialogue.transform.GetChild(0).GetChild(0).GetChild(1).gameObject;
        TextMeshProUGUI inputContent = inputField.GetComponent<TextMeshProUGUI>();
        inputContent.text = "Please submit your dialogue";
    }

    public void setNpcName(string Input)
    {
        npcName = Input;
    }
    public string getNpcName() => npcName;
}
