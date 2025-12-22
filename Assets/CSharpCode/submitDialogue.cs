using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class submitDialogue : MonoBehaviour
{
    //玩家输入的对话
    private string dialogue;
    //player
    public GameObject player;
    //监测玩家player是否在交互中
    private isInteraction i;
    //npc下一次输出的对话
    //对话框 Panel
    public GameObject dialoguePanel;
    //回答
    private string question;
    //提交窗口
    public GameObject subimitEvidence;
    //退出按钮
    public Transform exitPanel;

    void Start()
    {
        i = player.GetComponent<isInteraction>();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            submit();
        }
    }

    public void submit()
    {
        //获取输入内容
        GameObject text = GameObject.Find("Answer");
        TextMeshProUGUI inputDialogue = text.GetComponent<TextMeshProUGUI>();
        question = inputDialogue.text;

        getResponse();

        EventSystem.current.SetSelectedGameObject(null);
    }

    //将玩家输入发给后端
    void getResponse()
    {
        //玩家对话输入框消失
        GameObject playerDialogue = GameObject.Find("playerDialogue");
        playerDialogue.SetActive(false);

        //物品信息弹窗
        subimitEvidence.SetActive(true);

        //更改玩家交互状态
        i.changeIsSubmit(true);
    }

    public string getQuestion() => question;
}