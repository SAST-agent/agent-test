using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections.Generic;

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
    public AchievementSidebarController achievementSidebar;   // 新增

    [Header("NPC")]
    public NpcVisibilityManager npcVisibilityManager;

    // ======================
    // Backend
    // ======================
    [Header("Backend")]
    public string baseUrl = "http://localhost:8082";
    public float timeoutSeconds = 10f;

    // ======================
    // Stage / Evidence 状态
    // ======================
    private int currentStage = -1;
    private bool evidence1Get = false;
    private bool evidence2Get = false;

    private HashSet<string> otherEvidenceIds = new HashSet<string>();

    // ======================
    // NPC 问号
    // ======================
    public GameObject npc;
    private NpcSumBubble sumBubble;

    // ======================
    // Achievement 状态
    // ======================
    private HashSet<int> achievementIds = new HashSet<int>();
    private bool achievementInited = false;

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
    // Stage
    // ======================
    private IEnumerator FetchStage()
    {
        string url = $"{baseUrl}/api/stage";
        using var req = UnityWebRequest.Get(url);
        req.timeout = Mathf.CeilToInt(timeoutSeconds);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            yield break;

        var resp = JsonUtility.FromJson<StageResponse>(req.downloadHandler.text);
        OnStageUpdated(resp.stage);
    }

    private void OnStageUpdated(int stage)
    {
        if (currentStage == -1)
        {
            currentStage = stage;
            return;
        }

        if (stage > currentStage && stagePanel != null)
        {
            stagePanel.ShowStageUnlocked(stage, GetStageHint(stage));
        }

        currentStage = stage;

        if (npcVisibilityManager != null)
            npcVisibilityManager.RefreshNpcVisibility();
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
    // Evidence
    // ======================
    private IEnumerator FetchOtherEvidence()
    {
        string url = $"{baseUrl}/api/evidence/others";
        using var req = UnityWebRequest.Get(url);
        req.timeout = Mathf.CeilToInt(timeoutSeconds);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            yield break;

        var resp = JsonUtility.FromJson<OtherEvidenceResponse>(req.downloadHandler.text);

        if (resp.evidences != null)
        {
            foreach (var e in resp.evidences)
                otherEvidenceIds.Add(e.id);

            RefreshEvidenceUI();
        }
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
    // ⭐ Achievement
    // ======================
    private IEnumerator FetchAchievements()
    {
        string url = $"{baseUrl}/api/evidence/achievement";
        using var req = UnityWebRequest.Get(url);
        req.timeout = Mathf.CeilToInt(timeoutSeconds);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            yield break;

        var resp = JsonUtility.FromJson<AchievementResponse>(req.downloadHandler.text);
        if (resp == null || resp.achievements == null)
            yield break;

        OnAchievementsUpdated(resp.achievements);
    }

    private void OnAchievementsUpdated(Achievement[] achievements)
    {
        // 第一次：只记录
        if (!achievementInited)
        {
            achievementIds.Clear();
            foreach (var a in achievements)
                achievementIds.Add(a.id);

            achievementInited = true;
            return;
        }

        // 后续：发现新成就 → 弹 Sidebar
        foreach (var a in achievements)
        {
            if (achievementIds.Add(a.id))
            {
                Debug.Log($"[exitInteraction] New achievement: {a.name}");

                if (achievementSidebar != null)
                {
                    achievementSidebar.ShowAchievementUnlocked(
                        a.name,
                        a.description
                    );
                }
            }
        }
    }

    // ======================
    // JSON 映射
    // ======================
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

    public int getStage()
    {
        return currentStage;
    }

    // 进程调用总接口
    // ✅ 新增：由 FrameDispatcher 调用（方案A核心）
    public void ApplyResultState(FrameDispatcher.ResultState state)
    {
        if (state == null) return;

        // -----------------
        // 1) Stage 提示逻辑：复用你原来的 OnStageUpdated
        // -----------------
        OnStageUpdated(state.stage);

        // -----------------
        // 2) Evidence：用 state.visible_evidences 驱动 UI
        //    （替代 otherEvidenceIds + /api/evidence/others）
        // -----------------
        if (state.visible_evidences != null && itemBar != null)
        {
            bool has111 = state.visible_evidences.Contains("111");
            bool has711 = state.visible_evidences.Contains("711");

            itemBar.transform.Find("Bar/evidence1")?.gameObject.SetActive(has111);
            itemBar.transform.Find("Bar/evidence2")?.gameObject.SetActive(has711);
        }

        // -----------------
        // 3) Achievement：用 state.achievements（替代 /api/evidence/achievement）
        // -----------------
        if (state.achievements != null)
        {
            // 第一次只记录
            if (!achievementInited)
            {
                achievementIds.Clear();
                foreach (var a in state.achievements) achievementIds.Add(a.id);
                achievementInited = true;
            }
            else
            {
                // 后续出现新成就 -> 弹 Sidebar
                foreach (var a in state.achievements)
                {
                    if (achievementIds.Add(a.id))
                    {
                        if (achievementSidebar != null)
                            achievementSidebar.ShowAchievementUnlocked(a.name, a.description);
                    }
                }
            }
        }

        // -----------------
        // 4) NPC 可见 / 问号：建议交给各自脚本吃 state
        // -----------------
        npcVisibilityManager?.ApplyResultState(state);
        sumBubble?.ApplyResultState(state); // 如果你给 NpcSumBubble 加了这个入口
    }

    // ✅ 修改 refreshAll：不再发 HTTP
    public void refreshAll()
    {
        // 方案A下：refreshAll 不做网络拉取
        // 如果你想“强制刷新显示”，应该等下一帧 result_state 推送即可。
        // 可选：仅做一些纯本地刷新（不依赖网络）
        sumBubble?.RefreshNpcMark(); // 这行也可以删，最好让它吃 state.npc_marks
    }

}
