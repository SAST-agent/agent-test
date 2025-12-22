
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    //player
    public GameObject player;
    //交互距离
    public float interactionDistance = 0.75f;
    //对话框 Panel
    public GameObject dialoguePanel;
    //监测玩家player是否在交互中
    private isInteraction i;
    //获取npc名字
    private string npcName;
    //获取npc头像，npcImage为npc同名图片
    Sprite npcImage;
    //监测是否在可交互范围内
    private NPCDialogueTrigger isInRange;

    private void Start()
    {
        i = player.GetComponent<isInteraction>();
        string id = transform.GetComponent<NpcIdentity>().npcId;
        npcName = id;
        npcImage = Resources.Load<Sprite>(npcName);
        isInRange = gameObject.GetComponent<NPCDialogueTrigger>();
    }

    void Update()
    {
        if (isInRange.getPlayerInRange() &&
            Input.GetKeyUp(KeyCode.Q) &&
            !i.getIsTalk() && !i.getIsPaused())
        {
            startInteraction();
        }
    }

    //交互时调用
    public void startInteraction()
    {
        //修改玩家交互状态
        i.changeIsTalk(true);

        //初始化对话框所需元素
        Image talkerImage = dialoguePanel.transform.GetChild(0).gameObject.GetComponent<Image>();
        Transform content = dialoguePanel.transform.GetChild(1);

        //修改talkerImage为npcImage
        talkerImage.sprite = npcImage;

        //content问后端
        string talkContent = "Hello!";
        content.GetComponent<TextMeshProUGUI>().text = talkContent;

        //是否仅提交线索
        inDialogue interaction = dialoguePanel.GetComponent<inDialogue>();
        interaction.isClueOnly = false;

        //显示对话框
        dialoguePanel.SetActive(true);
        dialoguePanel.transform.GetComponent<inDialogue>().setNpcName(npcName);

    }

    public string GetNpcName() => npcName;
}
