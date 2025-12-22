using UnityEngine;
using TMPro;

public class HintManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject hintPanel;

    [Header("提示间隔（秒）")]
    public float idleTimeLimit = 60f;

    // 计时器
    private float idleTimer = 0f;

    [Header("Evidence 相关")]
    private int evidenceCount = -1;

    // ====== 状态控制 ======
    private bool hasShownHintThisRound = false;
    private bool isRequestingHint = false;
    private bool isRequestingEvidence = false;

    private void Start()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void Update()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTimeLimit &&
            !isRequestingEvidence &&
            !isRequestingHint)
        {
            idleTimer = 0f;
            CheckEvidenceAndMaybeShowHint();
        }
    }

    // =================================================
    // 检查线索数量
    // =================================================
    private void CheckEvidenceAndMaybeShowHint()
    {
        if (EvidenceService.Instance == null)
        {
            Debug.LogWarning("[HintManager] EvidenceService.Instance is null.");
            return;
        }

        isRequestingEvidence = true;

        EvidenceService.Instance.RequestCurrentEvidenceCount((newCount) =>
        {
            bool isSame = (newCount == evidenceCount);

            if (!isSame)
            {
                evidenceCount = newCount;
                hasShownHintThisRound = false;

                Debug.Log(
                    $"[HintManager] Evidence changed to {newCount}, reset hint state."
                );
            }
            else
            {
                if (!hasShownHintThisRound)
                {
                    RequestHint();
                    hasShownHintThisRound = true;
                }
                else
                {
                    Debug.Log("[HintManager] Hint already shown for this evidence state.");
                }
            }

            isRequestingEvidence = false;
        });
    }

    // =================================================
    // WS 请求 hint
    // =================================================
    private void RequestHint()
    {
        if (!WsClient.Instance.IsConnected)
        {
            Debug.LogError("[HintManager] WS not connected");
            return;
        }

        isRequestingHint = true;

        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new ActionContent
            {
                action = "hint"
            }
        };

        WsClient.Instance.ExpectNextMessage(OnHintResponse);
        WsClient.Instance.Send(JsonUtility.ToJson(req));
    }

    // =================================================
    // WS 回包处理
    // =================================================
    private void OnHintResponse(string json)
    {
        isRequestingHint = false;

        json = json.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');
        Debug.Log("[HintManager] Raw JSON: " + json);

        HintResponse resp;

        try
        {
            resp = JsonUtility.FromJson<HintResponse>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[HintManager] JSON parse failed: " + e.Message);
            return;
        }

        if (resp != null && !string.IsNullOrEmpty(resp.hint))
        {
            ShowHint(resp.hint);
        }
    }

    // =================================================
    // UI
    // =================================================
    private void ShowHint(string hint)
    {
        if (hintPanel == null) return;

        TextMeshProUGUI tmp =
            hintPanel.transform.GetChild(0)
                .GetComponent<TextMeshProUGUI>();

        tmp.text = hint;
        hintPanel.SetActive(true);
    }

    public void DismissHint()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
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

    [System.Serializable]
    private class HintResponse
    {
        public string hint;
    }
}
