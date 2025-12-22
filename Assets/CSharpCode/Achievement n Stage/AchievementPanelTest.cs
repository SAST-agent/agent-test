using System.Collections;
using UnityEngine;

public class AchievementPanelTest : MonoBehaviour
{
    public RectTransform panel;

    public Vector2 hiddenPos = new Vector2(400, 0); // 屏幕外
    public Vector2 shownPos = Vector2.zero;          // 屏幕内
    public float slideDuration = 0.3f;
    public float stayTime = 5f;

    void Start()
    {
        // 启动时自动测试
        panel.anchoredPosition = hiddenPos;
        StartCoroutine(TestRoutine());
    }

    IEnumerator TestRoutine()
    {
        // 平移进来
        yield return StartCoroutine(Slide(hiddenPos, shownPos));

        // 停留 5 秒
        yield return new WaitForSeconds(stayTime);

        // 平移出去
        yield return StartCoroutine(Slide(shownPos, hiddenPos));
    }

    IEnumerator Slide(Vector2 from, Vector2 to)
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
