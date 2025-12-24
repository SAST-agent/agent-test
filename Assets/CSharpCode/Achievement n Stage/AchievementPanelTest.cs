using System.Collections;
using UnityEngine;

public class AchievementPanelTest : MonoBehaviour
{
    public RectTransform panel;

    public Vector2 hiddenPos = new Vector2(400, 0); // ��Ļ��
    public Vector2 shownPos = Vector2.zero;          // ��Ļ��
    public float slideDuration = 0.3f;
    public float stayTime = 5f;

    void Start()
    {
        // ����ʱ�Զ�����
        panel.anchoredPosition = hiddenPos;
        StartCoroutine(TestRoutine());
    }

    IEnumerator TestRoutine()
    {
        // ƽ�ƽ���
        yield return StartCoroutine(Slide(hiddenPos, shownPos));

        // ͣ�� 5 ��
        yield return new WaitForSeconds(stayTime);

        // ƽ�Ƴ�ȥ
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
