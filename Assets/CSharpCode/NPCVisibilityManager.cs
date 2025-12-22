using System.Collections.Generic;
using UnityEngine;

public class NpcVisibilityManager : MonoBehaviour
{
    // 注册表：npcId -> GameObject
    private readonly Dictionary<string, GameObject> npcMap = new();

    // 当前可见 npcId
    private HashSet<string> visibleIds = new();

    private bool waitingResponse = false;

    private void Awake()
    {
        // 1) 扫描场景里所有 NPC
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

        // 默认全部隐藏（防止闪）
        SetAllActive(false);
    }

    private void Start()
    {
        if (WsClient.Instance != null && WsClient.Instance.IsConnected)
            RefreshNpcVisibility();
        else
            WsClient.Instance.OnConnected += RefreshNpcVisibility;
    }

    // =================================================
    // 对外接口
    // =================================================
    public void RefreshNpcVisibility()
    {
        if (!WsClient.Instance.IsConnected)
        {
            Debug.LogError("[NpcVisibility] WS not connected");
            return;
        }

        if (waitingResponse)
        {
            Debug.Log("[NpcVisibility] Already waiting response");
            return;
        }

        waitingResponse = true;

        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new ActionContent
            {
                action = "npc"
            }
        };

        WsClient.Instance.ExpectNextMessage(OnNpcListResponse);
        WsClient.Instance.Send(JsonUtility.ToJson(req));
    }

    // =================================================
    // WS 回包处理
    // =================================================
    private void OnNpcListResponse(string json)
    {
        waitingResponse = false;

        // 清理 BOM / 不可见字符
        json = json.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');
        Debug.Log("[NpcVisibility] Raw JSON = " + json);

        // 期望返回 JSON 数组: ["npc1","npc2",...]
        visibleIds = SimpleJsonArrayParser.ParseStringArray(json);

        ApplyVisibility();
    }

    // =================================================
    // 应用可见性
    // =================================================
    private void ApplyVisibility()
    {
        // 先全部隐藏
        SetAllActive(false);

        // 再按名单显示
        foreach (var id in visibleIds)
        {
            if (npcMap.TryGetValue(id, out var go) && go != null)
            {
                go.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"后端返回的 npcId 在场景中找不到：{id}");
            }
        }
    }

    private void SetAllActive(bool active)
    {
        foreach (var kv in npcMap)
        {
            if (kv.Value != null)
                kv.Value.SetActive(active);
        }
    }

    // =================================================
    // 对外查询接口
    // =================================================
    public List<string> getVisibleNpc()
    {
        return new List<string>(visibleIds);
    }

    // =================================================
    // WS JSON 映射
    // =================================================
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
}
