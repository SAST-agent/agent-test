using System;
using System.Collections.Generic;
using UnityEngine;

public class EvidenceService : MonoBehaviour
{
    public static EvidenceService Instance { get; private set; }

    [Header("WS")]
    public int pageSize = 10;

    private readonly List<TestimonyItem> cachedItems = new();
    private int cachedTotalCount = 0;

    private bool isRefreshing = false;
    private int currentPage = 1;
    private int totalPages = 1;
    private Action<bool> refreshCallback;

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

    public IReadOnlyList<TestimonyItem> CachedItems => cachedItems;
    public int CachedTotalCount => cachedTotalCount;

    // =================== 对外接口 ===================

    public void RefreshAllTestimonies(Action<bool> onCompleted = null)
    {
        if (isRefreshing)
        {
            Debug.Log("[EvidenceService] Already refreshing");
            return;
        }

        if (!WsClient.Instance.IsConnected)
        {
            Debug.LogError("[EvidenceService] WS not connected");
            onCompleted?.Invoke(false);
            return;
        }

        isRefreshing = true;
        refreshCallback = onCompleted;

        cachedItems.Clear();
        cachedTotalCount = 0;

        currentPage = 1;
        totalPages = 1;

        SendTestimonyRequest(currentPage);
    }

    public void RequestCurrentEvidenceCount(Action<int> onResult)
    {
        WsClient.Instance.ExpectNextMessage((json) =>
        {
            try
            {
                TestimonyResponse resp =
                    JsonUtility.FromJson<TestimonyResponse>(json);

                onResult?.Invoke(resp.total);
            }
            catch
            {
                onResult?.Invoke(0);
            }
        });

        SendTestimonyRequest(1, sizeOverride: 1);
    }

    // =================== 内部逻辑 ===================

    private void SendTestimonyRequest(int page, int? sizeOverride = null)
    {
        WsActionRequest req = new WsActionRequest
        {
            request = "action",
            token = ApiConfigService.Instance.token,
            content = new TestimonyActionContent
            {
                action = "testimony",
                page = page,
                size = sizeOverride ?? pageSize
            }
        };

        WsClient.Instance.ExpectNextMessage(OnTestimonyResponse);
        WsClient.Instance.Send(JsonUtility.ToJson(req));
    }

    private void OnTestimonyResponse(string json)
    {
        TestimonyResponse resp;

        try
        {
            resp = JsonUtility.FromJson<TestimonyResponse>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[EvidenceService] JSON parse error: " + e.Message);
            Finish(false);
            return;
        }

        if (resp == null)
        {
            Finish(false);
            return;
        }

        if (currentPage == 1)
        {
            cachedTotalCount = resp.total;
            totalPages = resp.total_pages;
        }

        if (resp.items != null)
            cachedItems.AddRange(resp.items);

        if (currentPage < totalPages)
        {
            currentPage++;
            SendTestimonyRequest(currentPage);
        }
        else
        {
            Finish(true);
        }
    }

    private void Finish(bool success)
    {
        isRefreshing = false;
        refreshCallback?.Invoke(success);
        refreshCallback = null;
    }

    // =================== WS JSON 映射 ===================

    [Serializable]
    private class WsActionRequest
    {
        public string request;
        public string token;
        public TestimonyActionContent content;
    }

    [Serializable]
    private class TestimonyActionContent
    {
        public string action;
        public int page;
        public int size;
    }

    // =================== 业务数据 ===================

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
