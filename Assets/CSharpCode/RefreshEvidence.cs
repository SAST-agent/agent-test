using System;
using UnityEngine;

public class RefreshEvidence : MonoBehaviour
{
    // ======================
    // Backend (legacy, unused in Saiblo/WS mode)
    // ======================
    [Header("Backend (Legacy / Unused)")]
    public string baseUrl = "http://localhost:8082";
    public float timeoutSeconds = 10f;

    // 当前阶段
    private int currentStage = -1;

    // 物品栏（在 Inspector 里拖 ItemBar）
    public GameObject itemBar;

    private void Start()
    {
        // ✅ 方案A：不再 Start 时 HTTP 拉取
        // 等 FrameDispatcher 推送 result_state 后再更新
        // 也可以先做一次“全隐藏”兜底（按你项目需要）
        ApplyStageToUI(-1);
    }

    // =================================================
    // ✅ 方案A核心：由 FrameDispatcher 每帧调用
    // =================================================
    public void ApplyResultState(FrameDispatcher.ResultState state)
    {
        if (state == null) return;

        // 方案A最小改动：继续按 stage 控制显示
        // 如果你想更准确（不依赖 stage），可以改成用 state.visible_evidences
        int stage = state.stage;

        // 没变化就不刷（可选优化）
        if (stage == currentStage) return;

        currentStage = stage;
        ApplyStageToUI(currentStage);
    }

    // =========================
    // 根据 Stage 显示证物（复用你原逻辑）
    // =========================
    private void ApplyStageToUI(int stage)
    {
        if (itemBar == null)
        {
            Debug.LogError("[RefreshEvidence] itemBar not assigned!");
            return;
        }

        // 先把相关证物隐藏（避免上一局残留）
        SetChildActiveSafe(itemBar.transform, 1, false);
        SetChildActiveSafe(itemBar.transform, 2, false);

        // 再按 stage 解锁
        if (stage >= 2)
            SetChildActiveSafe(itemBar.transform, 1, true);

        if (stage >= 8)
            SetChildActiveSafe(itemBar.transform, 2, true);
    }

    private void SetChildActiveSafe(Transform parent, int childIndex, bool active)
    {
        if (parent == null) return;
        if (childIndex < 0 || childIndex >= parent.childCount) return;

        var go = parent.GetChild(childIndex).gameObject;
        if (go != null) go.SetActive(active);
    }
}
