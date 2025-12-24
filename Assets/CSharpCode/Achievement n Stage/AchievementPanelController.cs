using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementPanelController : MonoBehaviour
{
    // =========================
    // 成就总表（规则表）保持不变
    // =========================

    [Serializable]
    public class Achievement
    {
        public string key;
        public int id;
        public string name;
        public string description;
    }

    private static readonly Dictionary<string, Achievement> AllAchievements =
        new Dictionary<string, Achievement>
        {
            { "1", new Achievement { key = "1", id = 1, name = "姐友妹恭", description = "发现 Rose 与范敏敏是亲姐妹。" } },
            { "2", new Achievement { key = "2", id = 2, name = "爱情买卖", description = "发现崔安彦刻意接近邓达岭的原因。" } },
            { "3", new Achievement { key = "3", id = 3, name = "推理也有城墙这个英雄？", description = "发现叶文潇与邓达岭的爱情故事。" } },
            { "4", new Achievement { key = "4", id = 4, name = "眼前的黑不是黑", description = "发现萧定昂的真实身份。" } },
            { "5", new Achievement { key = "5", id = 5, name = "你说的白是什么白", description = "发现范敏敏假扮 Rose 上台。" } },
        };

    private static List<Achievement> GetAllSorted()
    {
        var list = new List<Achievement>(AllAchievements.Values);
        list.Sort((a, b) => a.id.CompareTo(b.id));
        return list;
    }

    // =========================
    // UI
    // =========================

    [Header("UI")]
    public Transform contentRoot;
    public AchievementItem itemPrefab;

    // =========================
    // 内部状态：已解锁集合
    // =========================
    private readonly HashSet<string> unlockedKeys = new HashSet<string>();

    // =========================
    // 方案A新增：缓存最新 ResultState（可选）
    // =========================
    private FrameDispatcher.ResultState latestState;

    private void OnEnable()
    {
        // 打开面板就用“当前缓存”构建一次 UI
        RefreshAchievements();
    }

    // =================================================
    // ✅ 方案A核心：由 FrameDispatcher 每帧调用
    // =================================================
    public void ApplyResultState(FrameDispatcher.ResultState state)
    {
        latestState = state;

        // 如果面板当前正在显示，可选择实时刷新
        // （不想每帧刷 UI，可以注释掉这行，仅在 OnEnable 刷）
        if (isActiveAndEnabled && gameObject.activeInHierarchy)
        {
            RefreshAchievements();
        }
    }

    // =========================
    // 对外接口：刷新成就 UI（不走网络）
    // =========================
    public void RefreshAchievements()
    {
        unlockedKeys.Clear();

        if (latestState != null && latestState.achievements != null)
        {
            foreach (var a in latestState.achievements)
            {
                // FrameDispatcher.Achievement: id/name/description
                unlockedKeys.Add(a.id.ToString());
            }
        }

        BuildUI();
    }

    // =========================
    // 构建 UI（保持你原来的逻辑）
    // =========================
    private void BuildUI()
    {
        if (contentRoot == null || itemPrefab == null) return;

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var achievement in GetAllSorted())
        {
            bool unlocked = unlockedKeys.Contains(achievement.key);

            AchievementItem item = Instantiate(itemPrefab, contentRoot);
            item.SetData(
                achievement.name,
                achievement.description,
                unlocked
            );
        }
    }
}
