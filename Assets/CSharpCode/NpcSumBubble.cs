using System;
using System.Collections.Generic;
using UnityEngine;

public class NpcSumBubble : MonoBehaviour
{
    // ======================
    // Backend (legacy, unused in Saiblo/WS mode)
    // 保留字段：避免 Inspector 引用丢失、减少项目改动
    // ======================
    [Header("Backend (Legacy / Unused)")]
    public string baseUrl = "http://localhost:8082";
    public float timeoutSeconds = 10f;

    // 当前应该显示问号的 NPC（npcId 集合）
    private HashSet<string> npcWithQuestionMark = new HashSet<string>();

    private void Start()
    {
        // ✅ 方案A：不再 Start 时 HTTP 拉取
        // 等待 FrameDispatcher 推送 result_state 后再更新
        // 如果你担心“第一帧没来之前要不要隐藏”，可以在 Awake/Start 主动全关一次
        ApplyMarksToNpcs(); // 初始按空集合：全隐藏
    }

    // =================================================
    // ✅ 方案A核心：由 FrameDispatcher 每帧调用
    // 从 result_state.npc_marks 里解析哪些 npcId 需要显示问号
    // =================================================
    public void ApplyResultState(FrameDispatcher.ResultState state)
    {
        if (state == null) return;

        npcWithQuestionMark.Clear();

        // npc_marks 是 List<string>：里面就是需要显示问号的 npcId
        if (state.npc_marks != null)
        {
            foreach (var npcId in state.npc_marks)
            {
                if (!string.IsNullOrWhiteSpace(npcId))
                    npcWithQuestionMark.Add(npcId);
            }
        }

        ApplyMarksToNpcs();
    }


    // =================================================
    // 对外接口：刷新所有 NPC 问号
    // 方案A下不再网络拉取，只是“按当前缓存状态重新应用”
    // =================================================
    public void RefreshNpcMark()
    {
        ApplyMarksToNpcs();
    }

    // =================================================
    // 分发给每个 NPC
    // （保持你原来的逻辑：遍历 transform 子物体，找 NpcIdentity + NPCDialogueTrigger）
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

            // Debug.Log($"[NpcSumBubble] NPC {npcId} bubble = {visible}");
        }
    }
}
