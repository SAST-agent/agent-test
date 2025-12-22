using System.Collections;
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

    private void Awake()
    {
        panel.anchoredPosition = hiddenPos;

        // 初始不显示描述
        if (descriptionText != null)
            descriptionText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 对外接口：显示 成就解锁 Sidebar
    /// </summary>
    public void ShowAchievementUnlocked(string title, string description = null)
    {
        Debug.Log($"[AchievementSidebar] ShowAchievementUnlocked title={title}, description={description}");

        if (titleText == null) Debug.LogError("titleText is NULL");
        if (descriptionText == null) Debug.LogError("descriptionText is NULL");
        if (panel == null) Debug.LogError("panel is NULL");

        // 标题
        titleText.text = $"成就解锁：{title}";

        // 描述
        if (string.IsNullOrEmpty(description))
        {
            descriptionText.gameObject.SetActive(false);
        }
        else
        {
            descriptionText.text = description;
            descriptionText.gameObject.SetActive(true);
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
            panel.anchoredPosition = Vector2.Lerp(from, to, t / slideDuration);
            yield return null;
        }
        panel.anchoredPosition = to;
    }
}
