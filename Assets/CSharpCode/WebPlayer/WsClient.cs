using System;
using UnityEngine;

/// <summary>
/// WsClient
/// ----------------------------
/// 轻量 WS “客户端”
/// - 不负责真正建立 WebSocket（WebGL 下由 JS 插件 Connect_ws/Send_ws 做）
/// - 只负责：
///   1) 统一发送 payload（转交给 WebInteractionController）
///   2) 非帧消息的“等待下一条回包”机制（ExpectNextMessage）
///   3) 连接状态管理（IsConnected / OnConnected）
/// </summary>
public class WsClient : MonoBehaviour
{
    public static WsClient Instance { get; private set; }

    [Header("Bridge (drag WebInteractionController here)")]
    public WebInteractionController bridge;

    public bool IsConnected { get; private set; } = false;

    public event Action OnConnected;

    // 等待下一条“非帧消息”的回调（achievement/chat 等）
    private Action<string> nextMessageCb;

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

    /// <summary>
    /// 由 WebInteractionController 在收到第一条 WS 消息时调用
    /// </summary>
    public void MarkConnected()
    {
        if (IsConnected) return;

        IsConnected = true;
        Debug.Log("[WsClient] Connected");
        OnConnected?.Invoke();
    }

    /// <summary>
    /// 发送 WS payload（完整 JSON 字符串）
    /// </summary>
    public void Send(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return;

        if (bridge == null)
        {
            Debug.LogError("[WsClient] bridge is NULL, cannot Send. Please drag WebInteractionController into WsClient.bridge");
            return;
        }

        bridge.SendRawWs(payload);
    }

    /// <summary>
    /// 期待下一条“非帧消息”（judger 返回的 recv.content，但不是 Frame）
    /// 用法：WsClient.Instance.ExpectNextMessage(cb); 然后 Send(action...)
    /// </summary>
    public void ExpectNextMessage(Action<string> cb)
    {
        nextMessageCb = cb;
    }

    /// <summary>
    /// 由 WebInteractionController 在判定“不是 Frame”的回包时调用
    /// </summary>
    public void DispatchNonFrameMessage(string content)
    {
        var cb = nextMessageCb;
        nextMessageCb = null;

        if (cb != null)
        {
            cb(content);
        }
        else
        {
            // 没有人在等，就打印一下，方便调试
            Debug.Log("[WsClient] Non-frame message received but no callback is waiting:\n" + content);
        }
    }
}
