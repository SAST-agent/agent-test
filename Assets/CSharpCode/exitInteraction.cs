using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class exitInteraction : MonoBehaviour
{
    // ======================
    // Player & Dialogue
    // ======================
    [Header("Player")]
    public GameObject player;
    private isInteraction i;

    [Header("Dialogue")]
    public GameObject dialoguePanel;
    public GameObject itemBar;

    [Header("Stage")]
    public StageSidebarController stagePanel;

    [Header("Achievement")]
    public AchievementSidebarController achievementSidebar;

    [Header("NPC")]
    public NpcVisibilityManager npcVisibilityManager;

    // ======================
    // Stage / Evidence 状态
    // ======================
    private int currentStage = -1;
    private bool evidence1Get = false;
    private bool evidence2Get = false;

    private readonly HashSet<string> otherEvidenceIds = new();

    // ======================
    // NPC 问号
    // ======================
    public GameObject npc;
    private NpcSumBubble sumBubble;

    // ======================
    // Achievement 状态
    // ======================
    private readonly HashSet<int> achievementIds = new();
    private bool achievementInited = false;

    // ======================
    // WS 调度状态
    // ======================
    private enum RefreshStep
    {
        None,
        Stage,
        OtherEvidence,
        Achievement
    }

    private RefreshStep refreshStep = RefreshStep.None;

    private void Start()
    {
        i = player.GetComponent<isInteraction>();
        sumBubble = npc.GetComponent<NpcSumBubble>();
    }

    // ======================
    // 退出对话
    // ======================
    public void cancelInteraction()
    {
        if (i.getIsPaused()) return;

        i.changeIsTalk(false);
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(CloseDialogueLater());
    }

    private IEnumerator CloseDialogueLater()
    {
        yield return new WaitForSeconds(0.5f);
        dialoguePanel.SetActive(false);
    }

    // ======================
    // ⭐ 对外总刷新接口
    // ======================
    public void refreshAll()
    {
        if (!WsClient.Instance.IsConnected)
        {
            Debug.LogError("[exitInteraction] WS not connected");
            return;
        }

        refreshStep = RefreshStep.Stage;
        RequestStage();

        sumBubble.RefreshNpcMark();
    }

    // ======================
    // Stage
    // ======================
    private void RequestStage()
    {
        SendAction("stage");
        WsClient.Instance.ExpectNextMessage(OnStageResponse);
    }

    private void OnStageResponse(string json)
    {
        var resp = JsonUtility.FromJson<StageResponse>(json);
        OnStageUpdated(resp.stage);

        refreshStep = RefreshStep.OtherEvidence;
        RequestOtherEvidence();
    }

    private void OnStageUpdated(int stage)
    {
        if (currentStage == -1)
        {
            currentStage = stage;
            return;
        }

        if (stage > currentStage && stagePanel != null)
            stagePanel.ShowStageUnlocked(stage, GetStageHint(stage));

        currentStage = stage;

        npcVisibilityManager?.RefreshNpcVisibility();
    }

    private string GetStageHint(int stage)
    {
        if (stage == 2 || stage == 8)
            return "请查看物品栏，有新的物品！";
        if (stage >= 4 && stage <= 7)
            return "请查看线索版，有新的线索！";
        return null;
    }

    // ======================
    // Evidence (others)
    // ======================
    private void RequestOtherEvidence()
    {
        SendAction("others");
        WsClient.Instance.ExpectNextMessage(OnOtherEvidenceResponse);
    }

    private void OnOtherEvidenceResponse(string json)
    {
        var resp = JsonUtility.FromJson<OtherEvidenceResponse>(json);

        if (resp.evidences != null)
        {
            foreach (var e in resp.evidences)
                otherEvidenceIds.Add(e.id);

            RefreshEvidenceUI();
        }

        refreshStep = RefreshStep.Achievement;
        RequestAchievement();
    }

    private void RefreshEvidenceUI()
    {
        if (!evidence1Get && otherEvidenceIds.Contains("111"))
        {
            evidence1Get = true;
            itemBar.transform.Find("Bar/evidence1")?.gameObject.SetActive(true);
        }

        if (!evidence2Get && otherEvidenceIds.Contains("711"))
        {
            evidence2Get = true;
            itemBar.transform.Find("Bar/evidence2")?.gameObject.SetActive(true);
        }
    }

    // ======================
    // Achievement
    // ======================
    private void RequestAchievement()
    {
        SendAction("achievement");
        WsClient.Instance.ExpectNextMessage(OnAchievementResponse);
    }

    private void OnAchievementResponse(string json)
    {
        var resp = JsonUtility.FromJson<AchievementResponse>(json);
        if (resp == null || resp.achievements == null) return;

        OnAchievementsUpdated(resp.achievements);
    }

    private void OnAchievementsUpdated(Achievement[] achievements)
    {
        if (!achievementInited)
        {
            achievementIds.Clear();
            foreach (var a in achievements)
                achievementIds.Add(a.id);

            achievementInited = true;
            return;
        }

        foreach (var a in achievements)
        {
            if (achievementIds.Add(a.id))
            {
                achievementSidebar?.ShowAchievementUnlocked(
                    a.name,
                    a.description
                );
            }
        }
    }

    // ======================
    // WS 发送工具
    // ======================
    private void SendAction(string action)
    {
        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new ActionContent { action = action }
        };

        WsClient.Instance.Send(JsonUtility.ToJson(req));
    }

    // ======================
    // JSON 映射
    // ======================
    [System.Serializable]
    private class WsActionRequest
    {
        public string request;
        public string token;
        public ActionContent content;
    }

    [System.Serializable]
    private class ActionContent
    {
        public string action;
    }

    [System.Serializable]
    public class StageResponse
    {
        public int stage;
    }

    [System.Serializable]
    public class OtherEvidenceItem
    {
        public string id;
        public int series;
        public string name;
        public string type;
        public string content;
    }

    [System.Serializable]
    public class OtherEvidenceResponse
    {
        public OtherEvidenceItem[] evidences;
    }

    [System.Serializable]
    public class Achievement
    {
        public int id;
        public string name;
        public string description;
    }

    [System.Serializable]
    public class AchievementResponse
    {
        public Achievement[] achievements;
    }

    public int getStage() => currentStage;
}
