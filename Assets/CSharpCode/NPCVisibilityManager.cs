using System;
using System.Collections.Generic;
using UnityEngine;

public class NpcVisibilityManager : MonoBehaviour
{
    // ======================
    // Backend (legacy, unused in Saiblo/WS mode)
    // 保留字段：避免 Inspector 引用丢失、减少项目改动
    // ======================
    [Header("Backend (Legacy / Unused)")]
    public string baseUrl = "http://localhost:8082";
    public float timeoutSeconds = 10f;

    // ======================
    // Registry: npcId -> GameObject
    // ======================
    private Dictionary<string, GameObject> npcMap = new();

    // Visible NPC ids
    private HashSet<string> visibleIds = new();

    private void Awake()
    {
        // 1) 扫描场景里所有 NPC（含隐藏）
        var allNpc = FindObjectsOfType<NpcIdentity>(includeInactive: true);
        npcMap.Clear();

        foreach (var npc in allNpc)
        {
            if (string.IsNullOrWhiteSpace(npc.npcId))
            {
                Debug.LogWarning($"NPC {npc.name} 没有填 npcId");
                continue;
            }

            if (npcMap.ContainsKey(npc.npcId))
            {
                Debug.LogWarning($"重复 npcId: {npc.npcId}，后面的会覆盖前面的");
            }

            npcMap[npc.npcId] = npc.gameObject;
        }

        // 2) 默认全部隐藏（防止闪一下）
        SetAllActive(false);
    }

    private void Start()
    {
        // ✅ 方案A：不再 Start 时 HTTP 拉取
        // NPC 可见性完全由 FrameDispatcher 推送 result_state 来驱动
        // 如果你希望“在第一帧到来前显示某些默认NPC”，可以在这里自定义逻辑
    }

    // ======================
    // ✅ 方案A核心：由 FrameDispatcher 每帧调用
    // 要求 state.visible_npcs / state.visibleNpcIds 之类字段存在
    // ======================
    public void ApplyResultState(FrameDispatcher.ResultState state)
    {
        if (state == null) return;

        // ⚠️ 这里字段名以你项目 FrameDispatcher.ResultState 为准：
        // 我先按常见命名写 visible_npcs（List<string> 或 string[]）
        // 如果你那边叫 visibleNpc / visibleNpcs / visible_npc_ids
        // 只需要改这一处取值即可。

        IEnumerable<string> incoming = state.visible_npcs;  // ← 若编译报错，就改成你真实字段名
        if (incoming == null)
        {
            // 没给可见列表：按你想要的兜底策略
            // 方案A里更推荐：保持上一帧的可见状态，不要突然全隐藏
            return;
        }

        // 更新 visibleIds（做成 Set）
        visibleIds.Clear();
        foreach (var id in incoming)
        {
            if (!string.IsNullOrWhiteSpace(id))
                visibleIds.Add(id);
        }

        ApplyVisibility();
    }

    // ======================
    // Legacy API：保留原 RefreshNpcVisibility 以减少其他代码改动
    // 方案A下它不再发请求，只做一次“按当前 visibleIds 刷新”
    // ======================
    public void RefreshNpcVisibility()
    {
        // 以前是 StartCoroutine(FetchAndApply())
        // 现在改成：直接用当前缓存的 visibleIds 应用显示
        ApplyVisibility();
    }

    // ======================
    // Apply visibility to scene objects
    // ======================
    private void ApplyVisibility()
    {
        // 先全部隐藏
        SetAllActive(false);

        // visibleIds 为空：就全部隐藏（你也可以改成全部显示）
        if (visibleIds == null || visibleIds.Count == 0)
            return;

        // 再按名单显示
        foreach (var id in visibleIds)
        {
            if (npcMap.TryGetValue(id, out var go) && go != null)
            {
                go.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"result_state 返回的 npcId 在场景中找不到：{id}");
            }
        }
    }

    private void SetAllActive(bool active)
    {
        foreach (var kv in npcMap)
        {
            if (kv.Value != null) kv.Value.SetActive(active);
        }
    }

    // ======================
    // 保留：对外接口（你原来其他脚本可能在用）
    // ======================
    public List<string> getVisibleNpc()
    {
        List<string> visibleNpcs = new List<string>();
        if (visibleIds == null) return visibleNpcs;

        foreach (var id in visibleIds)
            visibleNpcs.Add(id);

        return visibleNpcs;
    }
}
