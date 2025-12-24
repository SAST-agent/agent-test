using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementSidebarController : MonoBehaviour
{
    [Header("UI")]
    public RectTransform panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Animation")]
    public Vector2 hiddenPos = new Vector2(400, 0);
    public Vector2 shownPos = Vector2.zero;
    public float slideDuration = 0.3f;
    public float stayTime = 5f;

    private Coroutine currentRoutine;

    // ===== 方案A新增：成就状态缓存 =====
    private readonly HashSet<int> knownAchievementIds = new HashSet<int>();
    private bool inited = false;

    private void Awake()
    {
        if (panel != null)
            panel.anchoredPosition = hiddenPos;

        // 初始不显示描述
        if (descriptionText != null)
            descriptionText.gameObject.SetActive(false);
    }

    // =================================================
    // ✅ 方案A核心：由 FrameDispatcher 每帧调用
    // =================================================
    public void ApplyResultState(FrameDispatcher.ResultState state)
    {
        if (state == null || state.achievements == null)
            return;

        // 第一次：只记录，不弹窗（避免一进来把历史成就全弹一遍）
        if (!inited)
        {
            knownAchievementIds.Clear();
            foreach (var a in state.achievements)
            {
                knownAchievementIds.Add(a.id);
            }
            inited = true;
            return;
        }

        // 后续：发现新的成就 id -> 弹 Sidebar
        foreach (var a in state.achievements)
        {
            if (knownAchievementIds.Add(a.id))
            {
                ShowAchievementUnlocked(a.name, a.description);
            }
        }
    }

    /// <summary>
    /// 对外接口：显示成就解锁 Sidebar
    /// </summary>
    public void ShowAchievementUnlocked(string title, string description = null)
    {
        Debug.Log($"[AchievementSidebar] ShowAchievementUnlocked title={title}, description={description}");

        if (titleText == null) Debug.LogError("titleText is NULL");
        if (descriptionText == null) Debug.LogError("descriptionText is NULL");
        if (panel == null) Debug.LogError("panel is NULL");

        // 标题
        if (titleText != null)
            titleText.text = $"成就解锁：{title}";

        // 描述
        if (descriptionText != null)
        {
            if (string.IsNullOrEmpty(description))
            {
                descriptionText.gameObject.SetActive(false);
            }
            else
            {
                descriptionText.text = description;
                descriptionText.gameObject.SetActive(true);
            }
        }

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return Slide(hiddenPos, shownPos);
        yield return new WaitForSeconds(stayTime);
        yield return Slide(shownPos, hiddenPos);
    }

    private IEnumerator Slide(Vector2 from, Vector2 to)
    {
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            if (panel != null)
                panel.anchoredPosition = Vector2.Lerp(from, to, t / slideDuration);
            yield return null;
        }
        if (panel != null)
            panel.anchoredPosition = to;
    }
}
