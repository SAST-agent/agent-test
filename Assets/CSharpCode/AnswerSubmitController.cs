using UnityEngine;

public class AnswerSubmitController : MonoBehaviour
{
    // ======================
    // 数据模型
    // ======================

    [System.Serializable]
    public class AnswerResponse
    {
        public bool murderer;
        public bool motivation;
        public bool method;
    }

    // ======================
    // 当前选择
    // ======================

    public string selectedMurderer;
    public string motivationText;
    public string methodText;

    private bool waitingResponse = false;

    // ======================
    // 对外接口
    // ======================

    public void SubmitAnswer()
    {
        if (!WsClient.Instance.IsConnected)
        {
            Debug.LogError("[AnswerSubmit] WS not connected");
            return;
        }

        if (waitingResponse)
        {
            Debug.Log("[AnswerSubmit] Already waiting response");
            return;
        }

        waitingResponse = true;

        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new AnswerActionContent
            {
                action = "answer",
                murderer = selectedMurderer,
                motivation = motivationText,
                method = methodText
            }
        };

        Debug.Log("[AnswerSubmit] Send answer");

        WsClient.Instance.ExpectNextMessage(OnAnswerResponse);
        WsClient.Instance.Send(JsonUtility.ToJson(req));
    }

    // ======================
    // WS 回包处理
    // ======================

    private void OnAnswerResponse(string json)
    {
        waitingResponse = false;

        json = json.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');
        Debug.Log("[AnswerSubmit] Raw JSON:\n" + json);

        AnswerResponse resp;

        try
        {
            resp = JsonUtility.FromJson<AnswerResponse>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[AnswerSubmit] JSON parse error: " + e.Message);
            return;
        }

        HandleResult(resp);
    }

    // ======================
    // 处理结果
    // ======================

    private void HandleResult(AnswerResponse result)
    {
        Debug.Log($"[AnswerSubmit] murderer: {result.murderer}");
        Debug.Log($"[AnswerSubmit] motivation: {result.motivation}");
        Debug.Log($"[AnswerSubmit] method: {result.method}");

        // TODO:
        // - 弹 UI 提示
        // - 进入结算 / 下一阶段
        // - 上报前端页面
    }

    // ======================
    // WS JSON 映射
    // ======================

    [System.Serializable]
    private class WsActionRequest
    {
        public string request;
        public string token;
        public AnswerActionContent content;
    }

    [System.Serializable]
    private class AnswerActionContent
    {
        public string action;
        public string murderer;
        public string motivation;
        public string method;
    }
}
