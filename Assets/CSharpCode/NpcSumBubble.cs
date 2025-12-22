using System;
using System.Collections.Generic;
using UnityEngine;

public class NpcSumBubble : MonoBehaviour
{
    // 当前应该显示问号的 NPC（npcId 集合）
    private readonly HashSet<string> npcWithQuestionMark = new();

    private bool waitingResponse = false;

    void Start()
    {
        if (WsClient.Instance != null && WsClient.Instance.IsConnected)
            RefreshNpcMark();
        else
            WsClient.Instance.OnConnected += RefreshNpcMark;
    }

    // =================================================
    // 对外接口：刷新所有 NPC 问号
    // =================================================
    public void RefreshNpcMark()
    {
        if (!WsClient.Instance.IsConnected)
        {
            Debug.LogError("[NpcSumBubble] WS not connected");
            return;
        }

        if (waitingResponse)
        {
            Debug.Log("[NpcSumBubble] Already waiting marks response");
            return;
        }

        waitingResponse = true;

        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new MarksActionContent
            {
                action = "marks"
            }
        };

        WsClient.Instance.ExpectNextMessage(OnMarksResponse);
        WsClient.Instance.Send(JsonUtility.ToJson(req));
    }

    // =================================================
    // WS 回包处理
    // =================================================
    private void OnMarksResponse(string json)
    {
        waitingResponse = false;

        // ⭐ 清理 BOM / 不可见字符（你之前踩过的坑）
        json = json.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');

        Debug.Log($"[NpcSumBubble] marks raw json = {json}");

        // =================================================
        // 解析所有字符串 key
        // =================================================
        HashSet<string> allStrings =
            SimpleJsonArrayParser.ParseStringArray(json);

        npcWithQuestionMark.Clear();

        foreach (string npcId in allStrings)
        {
            if (json.Contains($"\"{npcId}\":true"))
            {
                npcWithQuestionMark.Add(npcId);
            }
        }

        Debug.Log(
            $"[NpcSumBubble] NPCs with mark = {string.Join(", ", npcWithQuestionMark)}"
        );

        ApplyMarksToNpcs();
    }

    // =================================================
    // 分发给每个 NPC
    // =================================================
    private void ApplyMarksToNpcs()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform npcChild = transform.GetChild(i);

            NpcIdentity identity = npcChild.GetComponent<NpcIdentity>();
            NPCDialogueTrigger trigger = npcChild.GetComponent<NPCDialogueTrigger>();

            if (identity == null || trigger == null)
                continue;

            string npcId = identity.npcId;

            bool visible = npcWithQuestionMark.Contains(npcId);
            trigger.RefreshBubbleVisibility(visible);

            Debug.Log($"[NpcSumBubble] NPC {npcId} bubble = {visible}");
        }
    }

    // =================================================
    // WS JSON 映射
    // =================================================

    [Serializable]
    private class WsActionRequest
    {
        public string request;
        public string token;
        public MarksActionContent content;
    }

    [Serializable]
    private class MarksActionContent
    {
        public string action;
    }
}
