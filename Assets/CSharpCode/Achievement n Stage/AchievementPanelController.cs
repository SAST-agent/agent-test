using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementPanelController : MonoBehaviour
{
    // =========================
    // 数据结构（与后端一致）
    // =========================

    [Serializable]
    private class AchievementItemFromServer
    {
        public int id;
        public string name;
        public string description;
    }

    [Serializable]
    private class AchievementResponse
    {
        public List<AchievementItemFromServer> achievements;
    }

    // =========================
    // 成就总表（你定义的规则表）
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
    // 内部状态
    // =========================

    private readonly HashSet<string> unlockedKeys = new();

    private bool waitingResponse = false;

    private void OnEnable()
    {
        RefreshAchievements();
    }

    // =========================
    // 对外接口
    // =========================

    public void RefreshAchievements()
    {
        if (!WsClient.Instance.IsConnected)
        {
            Debug.LogError("[AchievementPanel] WS not connected");
            return;
        }

        if (waitingResponse)
        {
            Debug.Log("[AchievementPanel] Already waiting response");
            return;
        }

        waitingResponse = true;

        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new AchievementActionContent
            {
                action = "achievement"
            }
        };

        WsClient.Instance.ExpectNextMessage(OnAchievementResponse);
        WsClient.Instance.Send(JsonUtility.ToJson(req));
    }

    // =========================
    // WS 回包处理
    // =========================

    private void OnAchievementResponse(string json)
    {
        waitingResponse = false;

        json = json.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');
        Debug.Log("[AchievementPanel] Raw JSON:\n" + json);

        AchievementResponse response;

        try
        {
            response = JsonUtility.FromJson<AchievementResponse>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[AchievementPanel] JSON parse error: " + e.Message);
            return;
        }

        unlockedKeys.Clear();

        if (response != null && response.achievements != null)
        {
            foreach (var a in response.achievements)
            {
                unlockedKeys.Add(a.id.ToString());
            }
        }

        Debug.Log(
            "[AchievementPanel] unlocked keys = " +
            string.Join(",", unlockedKeys)
        );

        BuildUI();
    }

    // =========================
    // 构建 UI
    // =========================

    private void BuildUI()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var achievement in GetAllSorted())
        {
            bool unlocked = unlockedKeys.Contains(achievement.key);

            AchievementItem item =
                Instantiate(itemPrefab, contentRoot);

            item.SetData(
                achievement.name,
                achievement.description,
                unlocked
            );
        }
    }

    // =========================
    // WS JSON 映射
    // =========================

    [Serializable]
    private class WsActionRequest
    {
        public string request;
        public string token;
        public AchievementActionContent content;
    }

    [Serializable]
    private class AchievementActionContent
    {
        public string action;
    }
}
