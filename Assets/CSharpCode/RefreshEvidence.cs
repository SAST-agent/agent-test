using System;
using UnityEngine;

public class RefreshEvidence : MonoBehaviour
{
    // 当前阶段
    private int currentStage = -1;

    // 物品栏（在 Inspector 里拖 ItemBar）
    public GameObject itemBar;

    private bool waitingResponse = false;

    private void Start()
    {
        if (WsClient.Instance != null && WsClient.Instance.IsConnected)
            Refresh();
        else
            WsClient.Instance.OnConnected += Refresh;
    }

    // =========================
    // 对外接口
    // =========================
    public void Refresh()
    {
        if (!WsClient.Instance.IsConnected)
        {
            Debug.LogError("[RefreshEvidence] WS not connected");
            return;
        }

        if (waitingResponse)
        {
            Debug.Log("[RefreshEvidence] Already waiting response");
            return;
        }

        waitingResponse = true;

        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new ActionContent
            {
                action = "stage"
            }
        };

        WsClient.Instance.ExpectNextMessage(OnStageResponse);
        WsClient.Instance.Send(JsonUtility.ToJson(req));
    }

    // =========================
    // WS 回包处理
    // =========================
    private void OnStageResponse(string json)
    {
        waitingResponse = false;

        json = json.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');
        Debug.Log("[RefreshEvidence] Raw JSON: " + json);

        StageResponse response;

        try
        {
            response = JsonUtility.FromJson<StageResponse>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[RefreshEvidence] JSON parse error: " + e.Message);
            return;
        }

        if (response == null)
        {
            Debug.LogError("[RefreshEvidence] StageResponse parse failed");
            return;
        }

        currentStage = response.stage;
        Debug.Log($"[RefreshEvidence] Current Stage = {currentStage}");

        ShowEvidenceForStage();
    }

    // =========================
    // 根据 Stage 显示证物
    // =========================
    private void ShowEvidenceForStage()
    {
        if (itemBar == null)
        {
            Debug.LogError("[RefreshEvidence] itemBar not assigned!");
            return;
        }

        if (currentStage >= 2)
        {
            itemBar.transform.GetChild(1).gameObject.SetActive(true);
        }

        if (currentStage >= 8)
        {
            itemBar.transform.GetChild(2).gameObject.SetActive(true);
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
        public ActionContent content;
    }

    [Serializable]
    private class ActionContent
    {
        public string action;
    }

    [Serializable]
    private class StageResponse
    {
        public int stage;
    }
}
