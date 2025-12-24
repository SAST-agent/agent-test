using System;
using System.Collections.Generic;
using UnityEngine;

public class EvidenceService : MonoBehaviour
{
    public static EvidenceService Instance { get; private set; }

    // ======================
    // Backend (legacy, unused in Saiblo/WS mode)
    // ======================
    [Header("Backend (Legacy / Unused)")]
    public string baseUrl = "http://localhost:8082";
    public int pageSize = 10;
    public float timeoutSeconds = 10f;

    // 缓存：所有证言 & 总数量
    private readonly List<TestimonyItem> cachedItems = new List<TestimonyItem>();
    private int cachedTotalCount = 0;

    // 方案A：不再“刷新中”拉接口，但保留字段避免逻辑改动
    private bool isRefreshing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>只读访问缓存的所有证言</summary>
    public IReadOnlyList<TestimonyItem> CachedItems => cachedItems;

    /// <summary>只读访问缓存的线索总数（最近一次 ApplyResultState 后的）</summary>
    public int CachedTotalCount => cachedTotalCount;

    // =================================================
    // ✅ 方案A核心：由 FrameDispatcher 每帧调用
    // 把 result_state.testimony 同步到本地缓存
    // =================================================
    public void ApplyResultState(FrameDispatcher.ResultState state)
    {
        if (state == null) return;

        // 你 ResultState 中 testimony 类型是 List<GetEvidence.TestimonyItem>
        // 我这里做一个“字段拷贝”到本类的 TestimonyItem，避免其他 UI 依赖本类结构崩掉

        cachedItems.Clear();

        if (state.testimony != null)
        {
            foreach (var t in state.testimony)
            {
                if (t == null) continue;

                // ⚠️ 这里假设 GetEvidence.TestimonyItem 也有 id/name/type/content
                // 如果字段名不同（比如 evidence_id/title/text），你告诉我我给你对齐
                cachedItems.Add(new TestimonyItem
                {
                    id = t.id,
                    name = t.name,
                    type = t.type,
                    content = t.content
                });
            }
        }

        cachedTotalCount = cachedItems.Count;
    }

    // =================================================
    // 保留旧接口：RefreshAllTestimonies
    // 方案A下不再网络拉取，直接认为“当前缓存就是最新”
    // =================================================
    public void RefreshAllTestimonies(Action<bool> onCompleted = null)
    {
        if (isRefreshing)
        {
            Debug.Log("[EvidenceService] Already refreshing, ignore.");
            return;
        }

        // 方案A：没有网络刷新动作，直接回调成功
        onCompleted?.Invoke(true);
    }

    // =================================================
    // 保留旧接口：RequestCurrentEvidenceCount（HintManager 用）
    // 方案A下直接从缓存回调
    // =================================================
    public void RequestCurrentEvidenceCount(Action<int> onResult)
    {
        onResult?.Invoke(cachedTotalCount);
    }

    // =================================================
    // JSON 映射类（保留不动，避免别处引用出错）
    // =================================================
    [Serializable]
    public class TestimonyResponse
    {
        public int page;
        public int page_size;
        public int total;
        public int total_pages;
        public TestimonyItem[] items;
    }

    [Serializable]
    public class TestimonyItem
    {
        public string id;
        public string name;
        public string type;
        public string content;
    }
}
