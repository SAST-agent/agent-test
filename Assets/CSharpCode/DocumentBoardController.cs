using System.Collections;
using UnityEngine;

public class GetDocumentInfo : MonoBehaviour
{
    [Header("UI")]
    public GameObject boardRoot;           // 线索板 Panel
    public Transform contentRoot;          // ScrollView/Content
    public GameObject testimonyItemPrefab; // 单条证言 UI prefab

    private Coroutine buildRoutine;

    // 打开线索板：刷新数据 + 生成 UI
    public void OpenBoard()
    {
        boardRoot.SetActive(true);
        ClearContent();

        if (buildRoutine != null)
            StopCoroutine(buildRoutine);

        buildRoutine = StartCoroutine(BuildBoardCoroutine());
    }

    public void CloseBoard()
    {
        boardRoot.SetActive(false);

        if (buildRoutine != null)
        {
            StopCoroutine(buildRoutine);
            buildRoutine = null;
        }
    }

    private void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 4; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    private IEnumerator BuildBoardCoroutine()
    {
        if (EvidenceService.Instance == null)
        {
            Debug.LogError("[GetDocumentInfo] EvidenceService.Instance is null!");
            yield break;
        }

        bool finished = false;
        bool ok = false;

        // 让 EvidenceService 刷新一次数据
        EvidenceService.Instance.RefreshAllTestimonies(success =>
        {
            ok = success;
            finished = true;
        });

        // 等它刷新完
        while (!finished)
            yield return null;

        if (!ok)
        {
            Debug.LogError("[GetDocumentInfo] RefreshAllTestimonies failed.");
            yield break;
        }

        // 用缓存的数据生成 UI
        var items = EvidenceService.Instance.CachedItems;
        foreach (var item in items)
        {
            GameObject go = Instantiate(testimonyItemPrefab, contentRoot);
            var ui = go.GetComponent<TestimonyItemUI>();
            ui.SetData(item.name, item.content);
        }

        Debug.Log($"[GetDocumentInfo] build board done. total = {EvidenceService.Instance.CachedTotalCount}");

        buildRoutine = null;
    }
}
