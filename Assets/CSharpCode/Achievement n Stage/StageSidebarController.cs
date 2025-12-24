using System.Collections;
using TMPro;
using UnityEngine;

public class StageSidebarController : MonoBehaviour
{
    [Header("UI")]
    public RectTransform panel;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI hintText;

    [Header("Animation")]
    public Vector2 hiddenPos = new Vector2(400, 0);
    public Vector2 shownPos = Vector2.zero;
    public float slideDuration = 0.3f;
    public float stayTime = 5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        panel.anchoredPosition = hiddenPos;
        // ��ʼ����ʾ��ʾ
        if (hintText != null)
            hintText.gameObject.SetActive(false);
    }

    /// <summary>
    /// ����ӿڣ���ʾ Stage �����ɾ�
    /// </summary>
    public void ShowStageUnlocked(int stage, string hint = null)
    {
        Debug.Log($"[StageSidebar] ShowStageUnlocked stage={stage}, hint={hint}");

        if (stageText == null) Debug.LogError("stageText is NULL");
        if (hintText == null) Debug.LogError("hintText is NULL");
        if (panel == null) Debug.LogError("panel is NULL");

        stageText.text = $"Stage {stage} ������";
        // Content����ʾ��
        if (string.IsNullOrEmpty(hint))
        {
            hintText.gameObject.SetActive(false);
        }
        else
        {
            hintText.text = hint;
            hintText.gameObject.SetActive(true);
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
