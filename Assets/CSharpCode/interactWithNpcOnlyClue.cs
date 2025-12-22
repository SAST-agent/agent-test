using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractionOnlyClue : MonoBehaviour
{
    //player
    public GameObject player;
    //交互距离
    public float interactionDistance = 0.75f;
    //对话框 Panel
    public GameObject dialoguePanel;
    //NPC编号
    public int npcID;
    //全局数组，包含所有的npc线索
    private static string[] npcClues = {
        "我看那个女人（崔安彦）好早……不到18：00就来了吧，一开始是和那个戴金丝眼镜的，后来是和那个穿马褂的，难道是有啥奸情？", // 服务生甲 0
        "说起这俩人……我倒觉得有点奇怪，就最近的事，原先范敏敏一直对萧定昂挺好的，我们私底下还猜她是不是对萧定昂有意思。但最近突然忽冷忽热起来，某一天大献殷勤，第二天又反过来不理人家，真的挺奇怪的。", // 服务生乙 1
        "18：30的时候，在舞台右侧看到邓达岭和叶文潇在说悄悄话。", // 服务生丙 2 
        "我突然想起来，18：30在舞台左侧见到过崔安彦来着，当时她是跟哪个男的在一块来着……我只看到了个背影，穿的是一件西服。", // 服务生甲/ 之后可能会改丁
        "虽然Rose平时人不错，但好像对我们这些人没什么兴趣，气势上让人不太好接近，就跟个大小姐一样。但她好像和范敏敏早就认识，两个人同时来的舞厅，那时两人好像就认识，这么说起来仔细一看这俩人眉眼生的也有几分相似，不会……。",// 舞女甲 4
        "就大概18：55左右，我们上台之前，萧定昂突然跑到后台来问我们范敏敏在不在，很奇怪欸有没有。不过我好像能看出来，萧定昂和范敏敏这俩人最近不一般，萧定昂看范敏敏的眼神有的时候不太对劲，有点……温柔？我猜是有情况！嘿嘿嘿……",//舞女乙的证言 5
        "我总感觉邓达岭不大对劲，他好像并不像很多男人一样是为了Rose而来，反而对Rose有点爱答不理的感觉，不过每次邓达岭来了，叶文潇总会开心很长一段时间，我怀疑邓达岭来舞厅的真正目的是为了叶文潇。",//舞女丙的证言 6
        "诶？什么情况？准备室后门怎么被人锁上了？我18：30还从这出去来着？对了，当时我记得",//舞女丁的证言 7
        "我看到范敏敏没在舞台上，于是19：05的时候就出去找她来着。虽然没找到她，但是在准备室门口碰到了白井霆。",//萧定昂的证言 8
        "她还说人家Rose下贱呢，自己不也差不多。哦我说的是崔安彦。听说她的家族最近生意状况不大好，想要找邓达岭帮忙，就一直缠着他。哦对了，她家好像是做药材生意的来着。",//邓达岭朋友的证言 9
        "今天Rose怎么还带了面纱呢，她往常根本不会戴面纱的啊。说起来今天她歌也没唱几首，难道是出什么事了？不知道其他舞女知不知道发生了什么" //某位观众
        // 添加更多线索
    };
    private static string[] clueTitles =
    {
        "服务生甲的证言",//0
        "服务生乙的证言",//1
        "服务生丙的证言",//2
        "服务生甲的证言2",//3
        "舞女甲的证言",//4
        "舞女乙的证言",//5
        "舞女丙的证言",//6
        "舞女丁的证言",//7
        "萧定昂的证言",//8
        "邓达岭朋友的证言",//9
        "某位观众的证言"//10
    };

    //监测玩家player是否在交互中
    private isInteraction i;
    //获取npc名字
    private string npcName;
    //获取npc头像，npcImage为npc同名图片
    Sprite npcImage;
    //监测是否在可交互范围内
    private NPCDialogueTrigger isInRange;
    //添加记事板线索
    public Transform boardContent;
    public GameObject clueEvent;

    private void Start()
    {
        i = player.GetComponent<isInteraction>();
        npcName = gameObject.name;
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
    private void startInteraction()
    {
        //修改玩家交互状态
        i.changeIsTalk(true);

        //初始化对话框所需元素
        Image talkerImage = dialoguePanel.transform.GetChild(0).gameObject.GetComponent<Image>();
        Transform content = dialoguePanel.transform.GetChild(1);

        //修改talkerImage为npcImage
        talkerImage.sprite = npcImage;

        //content问后端
        if (npcClues[npcID] != null)
        {
            string talkContent = npcClues[npcID];
            content.GetComponent<TextMeshProUGUI>().text = talkContent;
        }

        //是否仅提交线索
        inDialogue interaction = dialoguePanel.GetComponent<inDialogue>();
        interaction.isClueOnly = true;
        addEventBoard();

        //显示对话框
        dialoguePanel.SetActive(true);
    }

    private void addEventBoard()
    {
        var newEvent = Instantiate(clueEvent, boardContent, false);
        newEvent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = clueTitles[npcID];
        newEvent.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = npcClues[npcID];
    }
}
