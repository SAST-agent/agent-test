using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class inDialogue : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
    private isInteraction i;

    [Header("UI Panels")]
    public GameObject playerDialogue;   // 输入弹窗
    public GameObject evidencePanel;    // 物品提交弹窗

    [Header("Mode")]
    public bool isClueOnly;

    // npc
    private string npcName;

    // ===== 新增：缓存本次 interaction =====
    private string lastNpcId;
    private string lastAskContent;
    private List<string> lastSubmitEvidenceIds;
    private string lastNpcReply;

    // ===== 新增：UI 引用（尽量自动找，找不到也不报错）=====
    private TextMeshProUGUI inputContentTmp; // 你原来那个 inputPlace
    [Header("Optional: NPC reply text (拖一个 TextMeshProUGUI 进来更稳)")]
    public TextMeshProUGUI replyText;

    private void Start()
    {
        i = player != null ? player.GetComponent<isInteraction>() : null;

        // 你原来定位 inputField 的路径：
        // playerDialogue -> (0) -> (0) -> (1)
        // 我保留，但加安全判断
        inputContentTmp = FindInputText(playerDialogue);

        // replyText 如果没拖，尝试在 playerDialogue 下找一个名字含 "reply" 的 TMP
        if (replyText == null && playerDialogue != null)
        {
            var tmps = playerDialogue.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                var n = t.gameObject.name.ToLowerInvariant();
                if (n.Contains("reply") || n.Contains("npc") || n.Contains("answer"))
                {
                    replyText = t;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (i == null) return;

        // 若正处于应暂停的交互中
        if (i.getIsPaused() || (evidencePanel != null && evidencePanel.activeSelf) || (playerDialogue != null && playerDialogue.activeSelf))
            return;

        // 按下空格，退出对话（沿用你原逻辑：点第2个子物体按钮）
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Button exitInteraction = transform.childCount > 2 ? transform.GetChild(2).GetComponent<Button>() : null;
            if (exitInteraction != null) exitInteraction.onClick.Invoke();
        }

        // 按下enter，继续对话（沿用你原逻辑）
        if (Input.GetKeyUp(KeyCode.Return))
        {
            if ((playerDialogue != null && playerDialogue.activeSelf) || (evidencePanel != null && evidencePanel.activeSelf))
                return;

            continueDialogue();
        }
    }

    private void continueDialogue()
    {
        // 进入对话 UI 状态
        i.changeIsPaused(true);
        i.changeIsTalk(true);

        if (playerDialogue != null)
            playerDialogue.SetActive(true);

        // 如果上一帧已经给了 ask_content，就显示它；否则保留默认提示
        if (inputContentTmp == null)
            inputContentTmp = FindInputText(playerDialogue);

        if (inputContentTmp != null)
        {
            if (!string.IsNullOrWhiteSpace(lastAskContent))
                inputContentTmp.text = lastAskContent;
            else
                inputContentTmp.text = "Please submit your dialogue";
        }

        // 同时显示 npc_reply（可选）
        if (replyText != null)
        {
            if (!string.IsNullOrWhiteSpace(lastNpcReply))
            {
                replyText.text = lastNpcReply;
                replyText.gameObject.SetActive(true);
            }
            else
            {
                // 没有 reply 就隐藏（避免显示上一次残留）
                replyText.gameObject.SetActive(false);
            }
        }
    }

    // =========================================================
    // ✅ 方案A关键：给 FrameDispatcher 调用的入口
    // =========================================================
    public void PlayInteraction(string npcId, string askContent, List<string> submitEvidenceIds, string npcReply)
    {
        lastNpcId = npcId;
        lastAskContent = askContent;
        lastSubmitEvidenceIds = submitEvidenceIds;
        lastNpcReply = npcReply;

        // npcName：你原来用 setNpcName 存，保持兼容
        setNpcName(npcId);

        // 如果当前已经开着对话面板，就即时刷新内容
        if (playerDialogue != null && playerDialogue.activeSelf)
        {
            if (inputContentTmp == null)
                inputContentTmp = FindInputText(playerDialogue);

            if (inputContentTmp != null && !string.IsNullOrWhiteSpace(lastAskContent))
                inputContentTmp.text = lastAskContent;

            if (replyText != null)
            {
                if (!string.IsNullOrWhiteSpace(lastNpcReply))
                {
                    replyText.text = lastNpcReply;
                    replyText.gameObject.SetActive(true);
                }
                else
                {
                    replyText.gameObject.SetActive(false);
                }
            }
        }

        // 如果你希望“收到 interaction 就自动弹出对话框”，取消下面注释：
        // if (playerDialogue != null && !playerDialogue.activeSelf) continueDialogue();
    }

    public void setNpcName(string Input)
    {
        npcName = Input;
    }

    public string getNpcName() => npcName;

    // =========================================================
    // 工具：安全找到输入框 TextMeshProUGUI
    // =========================================================
    private TextMeshProUGUI FindInputText(GameObject dialoguePanel)
    {
        if (dialoguePanel == null) return null;

        // 保留你原来的路径：GetChild(0)->GetChild(0)->GetChild(1)
        try
        {
            var t = dialoguePanel.transform;
            if (t.childCount > 0 &&
                t.GetChild(0).childCount > 0 &&
                t.GetChild(0).GetChild(0).childCount > 1)
            {
                var go = t.GetChild(0).GetChild(0).GetChild(1).gameObject;
                return go.GetComponent<TextMeshProUGUI>();
            }
        }
        catch { /* ignore */ }

        // 路径不匹配就兜底：找第一个 TMP 文本
        var tmp = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>(true);
        return tmp;
    }
}
