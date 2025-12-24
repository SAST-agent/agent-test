using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubmitEvidence : MonoBehaviour
{
    // ======================
    // Player / UI
    // ======================
    public GameObject player;
    private isInteraction i;

    public GameObject dialoguePanel;
    public GameObject InputPanel;
    public GameObject itemBar;

    private exitInteraction exitBtn;

    [Header("WS (drag in Inspector)")]
    public WsClient wsClient;                         // ✅ 直接拖引用
    public WebInteractionController webBridge;         // ✅ 可选：需要 token 时用

    // ======================
    // 证据映射
    // ======================
    private static readonly Dictionary<string, string> evidence = new()
    {
        { "Glass", "111" },
        { "Flower Pot", "112" },
        { "Body", "113" },
        { "Flower", "711" },
        { "Posion", "712" },
        { "Posion (2)", "713" },
        { "Posion (3)", "714" }
    };

    // ======================
    // 提交数据
    // ======================
    private string question;
    private string npc;
    private readonly List<string> evidences = new();

    private bool waitingResponse = false;

    // ======================
    // WS JSON 映射
    // ======================
    [Serializable]
    private class WsActionRequest
    {
        public string request;
        public string token;               // 可空：很多协议不要求 action 再带 token
        public ChatActionContent content;
    }

    [Serializable]
    private class ChatActionContent
    {
        public string action;
        public string question;
        public string npc;
        public List<string> evidences;
    }

    [Serializable]
    private class ChatResponse
    {
        public string response;
    }

    // ======================
    // Unity 生命周期
    // ======================
    private void Start()
    {
        if (player != null)
            i = player.GetComponent<isInteraction>();

        if (dialoguePanel != null && dialoguePanel.transform.childCount > 2)
            exitBtn = dialoguePanel.transform.GetChild(2).GetComponent<exitInteraction>();

        // 兜底：如果没拖 wsClient，就用单例（你新 WsClient 支持 Instance）
        if (wsClient == null)
            wsClient = WsClient.Instance;

        // 兜底：如果没拖 webBridge，尝试场景里找
        if (webBridge == null)
            webBridge = FindObjectOfType<WebInteractionController>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            Submit();
        }
    }

    // ======================
    // 提交入口
    // ======================
    public void Submit()
    {
        if (wsClient == null)
        {
            Debug.LogError("[SubmitEvidence] wsClient is null (drag WsClient in Inspector).");
            return;
        }

        if (!wsClient.IsConnected)
        {
            Debug.LogError("[SubmitEvidence] WS not connected");
            return;
        }

        if (waitingResponse)
        {
            Debug.Log("[SubmitEvidence] Already waiting response");
            return;
        }

        CollectSelectedEvidences();
        CollectQuestionAndNpc();
        SendChatRequest();

        // 本地状态更新（保持原逻辑）
        if (exitBtn != null) exitBtn.refreshAll();
        if (i != null)
        {
            i.changeIsSubmit(false);
            i.changeIsPaused(false);
        }
    }

    // ======================
    // 收集证据
    // ======================
    private void CollectSelectedEvidences()
    {
        evidences.Clear();

        if (itemBar == null) return;

        for (int i = 1; i < 3; i++)
        {
            if (itemBar.transform.childCount <= i) continue;

            Transform evidenceSet = itemBar.transform.GetChild(i);

            for (int j = 0; j < evidenceSet.childCount; j++)
            {
                GameObject button = evidenceSet.GetChild(j).gameObject;

                var item = button.GetComponent<item_InBar>();
                var image = button.GetComponent<Image>();

                if (item == null || image == null) continue;
                if (image.color == Color.white) continue;

                image.color = Color.white;
                item.isSelect = false;
                item.changeMode(0);

                if (evidence.TryGetValue(button.name, out var id))
                {
                    evidences.Add(id);
                }
            }
        }
    }

    // ======================
    // 收集问题 / NPC
    // ======================
    private void CollectQuestionAndNpc()
    {
        var submitDlg = InputPanel != null ? InputPanel.GetComponent<submitDialogue>() : null;
        if (submitDlg != null)
            question = submitDlg.getQuestion();

        var dlg = dialoguePanel != null ? dialoguePanel.GetComponent<inDialogue>() : null;
        if (dlg != null)
            npc = dlg.getNpcName();
    }

    // ======================
    // WS 发送
    // ======================
    private void SendChatRequest()
    {
        waitingResponse = true;

        var req = new WsActionRequest
        {
            request = "action",

            // ✅ 方案A：默认不需要 token（连接时已经带了）
            token = null,

            content = new ChatActionContent
            {
                action = "chat",
                question = question,
                npc = npc,
                evidences = new List<string>(evidences)
            }
        };

        // 如果你的协议强制 action 里必须带 token：
        // 取消下面注释，并确保 WebInteractionController 暴露一个 GetTokenB64()
        // req.token = webBridge != null ? webBridge.GetTokenB64() : null;

        wsClient.ExpectNextMessage(OnChatResponse);
        wsClient.Send(JsonUtility.ToJson(req));
    }

    // ======================
    // WS 回包
    // ======================
    private void OnChatResponse(string json)
    {
        waitingResponse = false;

        json = json.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');
        Debug.Log("[SubmitEvidence] Raw JSON:\n" + json);

        ChatResponse resp;
        try
        {
            resp = JsonUtility.FromJson<ChatResponse>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[SubmitEvidence] JSON parse error: " + e.Message);
            return;
        }

        if (resp == null || string.IsNullOrEmpty(resp.response))
            return;

        // 写到对话框（保持你的路径：dialoguePanel child(1) 是文本）
        if (dialoguePanel != null && dialoguePanel.transform.childCount > 1)
        {
            var chat = dialoguePanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            if (chat != null) chat.text = resp.response;
        }

        // 关闭提交面板（保持原行为）
        if (transform.parent != null)
            transform.parent.gameObject.SetActive(false);
    }

    // ======================
    // 外部调用接口（保持）
    // ======================
    public void submitEvidenceExternal(List<string> submitEvidenceIds)
    {
        if (wsClient == null)
        {
            Debug.LogError("[SubmitEvidence] wsClient is null.");
            return;
        }

        if (!wsClient.IsConnected)
        {
            Debug.LogError("[SubmitEvidence] WS not connected");
            return;
        }

        if (waitingResponse)
        {
            Debug.Log("[SubmitEvidence] Already waiting response");
            return;
        }

        evidences.Clear();
        if (submitEvidenceIds != null)
            evidences.AddRange(submitEvidenceIds);

        CollectQuestionAndNpc();
        SendChatRequest();

        if (i != null)
        {
            i.changeIsSubmit(false);
            i.changeIsPaused(false);
        }
    }
}
