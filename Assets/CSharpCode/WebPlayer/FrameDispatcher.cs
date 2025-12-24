using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FrameDispatcher
/// ----------------------------
/// Unity 侧唯一的「帧入口」
/// - 吃一帧 Frame
/// - 播 interaction
/// - 应用 result_state
/// - 不做任何逻辑判断
/// </summary>
public class FrameDispatcher : MonoBehaviour
{
    [Header("Interaction / Dialogue")]
    public inDialogue dialogueController;   // 复用你现有的对话脚本

    [Header("ResultState Appliers")]
    public NpcVisibilityManager npcVisibilityManager; // npc 可见
    public NpcSumBubble npcSumBubble; // npc hint提示
    public RefreshEvidence refreshEvidence; // 刷新证据
    public EvidenceService evidenceService; // ？
    public HintManager hintManager; // 当前阶段的提示
    public AchievementSidebarController achievementSidebar; // 管理成就的
    public exitInteraction exitInteraction; // 拖拽绑定


    // =================================================
    // ⭐ 对外唯一入口
    // =================================================
    public void ApplyFrame(Frame frame)
    {
        if (frame == null)
        {
            Debug.LogWarning("[FrameDispatcher] Frame is null");
            return;
        }

        Debug.Log($"[FrameDispatcher] Apply frame {frame.step_id}");

        // 1️⃣ 播放 interaction（对话 / 提交内容）
        ApplyInteraction(frame);

        // 2️⃣ 应用世界状态
        ApplyResultState(frame.result_state);
    }

    // =================================================
    // Interaction
    // =================================================
    private void ApplyInteraction(Frame frame)
    {
        if (dialogueController == null)
            return;

        if (frame.interaction == null)
            return;

        dialogueController.PlayInteraction(
            frame.npc_id,
            frame.interaction.ask_content,
            frame.interaction.submit_evidence_id,
            frame.interaction.npc_reply
        );
    }

    // =================================================
    // Result State
    // =================================================
    private void ApplyResultState(ResultState state)
    {
        if (state == null)
            return;

        npcVisibilityManager?.ApplyResultState(state);
        npcSumBubble?.ApplyResultState(state);
        refreshEvidence?.ApplyResultState(state);
        hintManager?.ApplyResultState(state);

        evidenceService.ApplyResultState(state);

        // achievement
        achievementSidebar?.ApplyResultState(state);

        exitInteraction?.ApplyResultState(state);

    }

    // =================================================
    // ===== JSON 数据结构（与后端 / 前端协议一致）====
    // =================================================

    [Serializable]
    public class Frame
    {
        public int step_id;
        public string npc_id;
        public Interaction interaction;
        public ResultState result_state;
    }

    [Serializable]
    public class Interaction
    {
        public string ask_content;
        public List<string> submit_evidence_id;
        public string npc_reply;
    }

    [Serializable]
    public class ResultState
    {
        public int stage;

        public List<string> visible_npcs;
        public List<string> visible_evidences;
        public List<string> npc_marks;

        public List<EvidenceService.TestimonyItem> testimony;
        public List<Achievement> achievements;

        public string hint;

        // 终局字段（仅终局帧出现）
        public AnswerResult answer_result;
        public string stop_reason;
        public Ending ending;
    }

    [Serializable]
    public class Achievement
    {
        public int id;
        public string name;
        public string description;
    }

    [Serializable]
    public class AnswerResult
    {
        public bool murderer;
        public bool motivation;
        public bool method;
    }

    [Serializable]
    public class Ending
    {
        public string id;
        public string title;
        public string description;
    }

}
