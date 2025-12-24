using UnityEngine;
using TMPro;

/// <summary>
/// 发呆提示管理（方案A：WS/Frame 驱动）
/// - FrameDispatcher 每帧推送 ResultState
/// - 记录“线索状态签名”（visible_evidences + testimony）
/// - 每 idleTimeLimit 秒检查一次：
///   - 若线索状态与上次检查一致 且 本轮尚未提示 → 显示 state.hint
///   - 若线索状态变化 → 重置提示状态
/// </summary>
public class HintManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject hintPanel;

    [Header("提示间隔（秒）")]
    public float idleTimeLimit = 60f;

    // 计时器
    private float idleTimer = 0f;

    [Header("Hint 接口 (Legacy / Unused)")]
    [SerializeField]
    private string hintApiUrl = "http://localhost:8082/api/stage/hint";

    [Header("Evidence 相关 (Legacy)")]
    [SerializeField]
    private int evidenceCount = -1; // 保留字段避免 Inspector 丢失

    // ====== 状态控制（核心） ======
    private bool hasShownHintThisRound = false;

    // ========== 方案A新增：来自 ResultState 的缓存 ==========
    private string latestHint = null;

    // 用于判断“线索状态有没有变”
    private string currentEvidenceSignature = "";
    private string lastCheckedSignature = "";

    private void Start()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);

        idleTimer = 0f;
    }

    private void Update()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTimeLimit)
        {
            idleTimer = 0f;
            CheckEvidenceAndMaybeShowHint_NoNetwork();
        }
    }

    // =================================================
    // ✅ 方案A核心：由 FrameDispatcher 每帧调用
    // =================================================
    public void ApplyResultState(FrameDispatcher.ResultState state)
    {
        if (state == null) return;

        // 1) 缓存 hint 文本（评测机/回放帧给的）
        latestHint = state.hint;

        // 2) 计算线索状态签名（visible_evidences + testimony）
        //    只要这两者有变化，就认为“线索状态变化”
        currentEvidenceSignature = BuildEvidenceSignature(state);

        // 3) 若线索状态变化：重置“本轮是否提示过”
        //    注意：这里的“变化”是相对上一次收到帧的状态
        //    lastCheckedSignature 是“上次发呆检查时的状态”，不要在这里改
        //    我们只需要在这里做：若当前状态≠上一次帧状态，则重置
        //    为了最小改动，直接用一个静态缓存记录“上一帧签名”
        if (_lastFrameSignature != currentEvidenceSignature)
        {
            hasShownHintThisRound = false;
            _lastFrameSignature = currentEvidenceSignature;
        }

        // 可选：如果你希望“只要线索变化就立刻隐藏提示面板”
        // if (hintPanel != null) hintPanel.SetActive(false);
    }

    // 上一帧的签名（仅用于检测“线索变化”）
    private string _lastFrameSignature = "";

    // =================================================
    // 发呆检查：不走网络，直接用缓存状态决定要不要弹提示
    // =================================================
    private void CheckEvidenceAndMaybeShowHint_NoNetwork()
    {
        // 线索状态与上次检查一致？
        bool isSameAsLastCheck = (currentEvidenceSignature == lastCheckedSignature);

        if (!isSameAsLastCheck)
        {
            // 线索变化：更新“上次检查”基准，并重置本轮提示状态
            lastCheckedSignature = currentEvidenceSignature;
            hasShownHintThisRound = false;
            return;
        }

        // 线索未变化：本轮还没提示过 → 弹提示
        if (!hasShownHintThisRound)
        {
            ShowHint(latestHint);
            hasShownHintThisRound = true;
        }
    }

    // =================================================
    // 显示提示（仅UI，不做网络）
    // =================================================
    private void ShowHint(string hintText)
    {
        if (hintPanel == null) return;
        if (string.IsNullOrWhiteSpace(hintText)) return;

        // 默认：hintPanel 的第0个子物体挂了 TextMeshProUGUI（沿用你原逻辑）
        var tmp = hintPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = hintText;

        hintPanel.SetActive(true);
    }

    /// <summary>
    /// 由“关闭提示”按钮调用：只关闭 UI，不重置状态
    /// </summary>
    public void DismissHint()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    // =================================================
    // 线索状态签名：可稳定反映“线索有没有变”
    // - visible_evidences 的数量 + 内容
    // - testimony 的数量 + 内容（如果 TestimonyItem 有 id/name 等字段，会更稳）
    // =================================================
    private string BuildEvidenceSignature(FrameDispatcher.ResultState state)
    {
        // 你 ResultState 里：
        // public List<string> visible_evidences;
        // public List<GetEvidence.TestimonyItem> testimony;

        System.Text.StringBuilder sb = new System.Text.StringBuilder(256);

        // visible_evidences
        if (state.visible_evidences != null)
        {
            sb.Append("E:");
            sb.Append(state.visible_evidences.Count);
            sb.Append('|');
            for (int i = 0; i < state.visible_evidences.Count; i++)
            {
                sb.Append(state.visible_evidences[i]);
                sb.Append(',');
            }
        }
        else
        {
            sb.Append("E:0|");
        }

        // testimony
        if (state.testimony != null)
        {
            sb.Append("T:");
            sb.Append(state.testimony.Count);
            sb.Append('|');
            for (int i = 0; i < state.testimony.Count; i++)
            {
                // 不确定 TestimonyItem 的字段名，这里用 ToString() 保底
                // 如果你知道它有 id / name / evidenceId，改成拼那个会更稳定
                sb.Append(state.testimony[i] != null ? state.testimony[i].ToString() : "null");
                sb.Append(',');
            }
        }
        else
        {
            sb.Append("T:0|");
        }

        return sb.ToString();
    }
}
