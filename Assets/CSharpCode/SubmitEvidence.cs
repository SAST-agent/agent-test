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
        public string token;
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
        i = player.GetComponent<isInteraction>();
        exitBtn = dialoguePanel.transform.GetChild(2).GetComponent<exitInteraction>();
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
        if (!WsClient.Instance.IsConnected)
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

        // 立刻更新本地状态（与原逻辑一致）
        exitBtn.refreshAll();
        i.changeIsSubmit(false);
        i.changeIsPaused(false);
    }

    // ======================
    // 收集证据
    // ======================
    private void CollectSelectedEvidences()
    {
        evidences.Clear();

        for (int i = 1; i < 3; i++)
        {
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
        var submitDlg = InputPanel.GetComponent<submitDialogue>();
        if (submitDlg != null)
            question = submitDlg.getQuestion();

        var dlg = dialoguePanel.GetComponent<inDialogue>();
        if (dlg != null)
            npc = dlg.getNpcName();
    }

    // ======================
    // WS 发送
    // ======================
    private void SendChatRequest()
    {
        waitingResponse = true;

        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new ChatActionContent
            {
                action = "chat",
                question = question,
                npc = npc,
                evidences = new List<string>(evidences)
            }
        };

        WsClient.Instance.ExpectNextMessage(OnChatResponse);
        WsClient.Instance.Send(JsonUtility.ToJson(req));
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

        // 写到对话框
        var chat =
            dialoguePanel.transform.GetChild(1)
                .GetComponent<TextMeshProUGUI>();

        chat.text = resp.response;

        // 关闭提交面板（与原 PostChatAndClose 行为一致）
        transform.parent.gameObject.SetActive(false);
    }

    // ======================
    // 外部调用接口（保持）
    // ======================
    public void submitEvidenceExternal(List<string> submitEvidenceIds)
    {
        evidences.Clear();
        evidences.AddRange(submitEvidenceIds);

        CollectQuestionAndNpc();
        SendChatRequest();

        exitBtn.refreshAll();
        i.changeIsSubmit(false);
        i.changeIsPaused(false);
    }
}
