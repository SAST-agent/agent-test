using System;
using UnityEngine;

public class WsClient : MonoBehaviour
{
    public static WsClient Instance { get; private set; }

    public bool IsConnected { get; private set; } = false;

    // 当前“期待”的回包处理器（一次只允许一个）
    private Action<string> nextMessageHandler;

    // 🔔 WS 连接完成事件
    public event Action OnConnected;

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

    // ======================
    // WS 生命周期（唯一）
    // ======================

    /// <summary>
    /// 由 WS 底层在连接成功时调用
    /// </summary>
    public void OnOpen()
    {
        IsConnected = true;
        Debug.Log("[WsClient] Connected");

        // ⭐ 非常关键
        OnConnected?.Invoke();
    }

    /// <summary>
    /// 由 WS 底层在断开时调用
    /// </summary>
    public void OnClose()
    {
        IsConnected = false;
        Debug.Log("[WsClient] Disconnected");
    }

    // ======================
    // 业务接口
    // ======================

    /// <summary>
    /// 注册“下一条消息”的处理器（一次性）
    /// </summary>
    public void ExpectNextMessage(Action<string> handler)
    {
        nextMessageHandler = handler;
    }

    /// <summary>
    /// 统一发送 WS 文本
    /// </summary>
    public void Send(string json)
    {
        Debug.Log("[WsClient] Send: " + json);
        Send_ws(json);
    }

    // ======================
    // WS 消息入口
    // ======================

    /// <summary>
    // 必须由 WS 底层在收到消息时调用
    /// </summary>
    public void OnMessage(string message)
    {
        Debug.Log("[WsClient] Receive: " + message);

        if (nextMessageHandler != null)
        {
            var handler = nextMessageHandler;
            nextMessageHandler = null; // 防止串包
            handler.Invoke(message);
        }
        else
        {
            Debug.LogWarning("[WsClient] No handler for message");
        }
    }

    // ======================
    // 你已有的底层发送
    // ======================
    private void Send_ws(string msg)
    {
        // webSocket.Send(msg);
    }
}
