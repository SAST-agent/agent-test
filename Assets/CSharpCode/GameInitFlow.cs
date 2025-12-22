using System;
using UnityEngine;

public class GameInitFlow : MonoBehaviour
{
    public static GameInitFlow Instance { get; private set; }

    /// <summary>
    /// WS 已连接，游戏可以开始初始化
    /// </summary>
    public bool IsReady { get; private set; } = false;

    /// <summary>
    /// 游戏初始化事件（所有系统监听这个）
    /// </summary>
    public event Action OnGameReady;

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

    private void Start()
    {
        if (WsClient.Instance == null)
        {
            Debug.LogError("[GameInitFlow] WsClient not found in scene");
            return;
        }

        if (WsClient.Instance.IsConnected)
        {
            MarkReady();
        }
        else
        {
            Debug.Log("[GameInitFlow] Waiting for WS connection...");
            WsClient.Instance.OnConnected += MarkReady;
        }
    }

    private void MarkReady()
    {
        if (IsReady) return;

        IsReady = true;
        Debug.Log("[GameInitFlow] Game is READY");

        OnGameReady?.Invoke();
    }
}
