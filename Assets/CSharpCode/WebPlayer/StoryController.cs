using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryController : MonoBehaviour
{
    [Header("Dialogue System")]
    public GameObject playerInput;      // 你已有
    public GameObject npc;               // 对话状态

    [Header("Evidence System")]
    public GameObject submit;   // 你已有
    public EvidenceService evidenceManager;     // 你已有

    public GameObject itemBar;

    [Header("Stage & NPC")]
    public GameObject exitButton;           // 你已有
    public GameObject npcManager;           // 你已有

    // ===== 执行一次 Interaction =====
    public ResultState ExecuteInteraction(string npcId, Interaction interaction)
    {
        ResultState result = new ResultState
        {
            achievements = new List<string>(),
            testimony = new List<string>(),
            visible_npcs = new List<string>(),
            visible_evidences = new List<string>()
        };

        // ---------- 1️ 对话 ----------
        if (!string.IsNullOrEmpty(interaction.ask_content))
        {
            Debug.Log($"[Story] Dialogue with {npcId}");

            // 进入对话状态
            for (int i = 10; i < npc.transform.childCount; i++)
            {
                Transform child = npc.transform.GetChild(i);
                if(child.GetComponent<NpcIdentity>().npcId == npcId)
                {
                    child.GetComponent<PlayerInteraction>().startInteraction();
                    break;
                }
            }

            // 提交对话内容（你原本在按钮里干的事）
            playerInput.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = interaction.ask_content;
            playerInput.transform.GetChild(1).GetComponent<Button>().onClick.Invoke();

        }

        // ---------- 2️ 提交证据 ----------
        if (interaction.submit_evidence_id != null &&
            interaction.submit_evidence_id.Count > 0)
        {
            submit.transform.GetComponent<SubmitEvidence>().submitEvidenceExternal(interaction.submit_evidence_id);

        }

        // ---------- 3️ Stage 推进 ----------
        exitInteraction Exit = exitButton.transform.GetComponent<exitInteraction>();
        Exit.refreshAll();
        int newStage = Exit.getStage();
        result.stage = newStage;

        // ---------- 4️ NPC 可见性 ----------
        List<string> visibleNPCs = npcManager.transform.GetComponent<NpcVisibilityManager>().getVisibleNpc();
        result.visible_npcs.AddRange(visibleNPCs);

        Debug.Log("[Story] Interaction Executed");

        return result;
    }
}
