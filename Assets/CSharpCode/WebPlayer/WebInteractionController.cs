using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

using Frame = FrameDispatcher.Frame;

public class WebInteractionController : MonoBehaviour
{
    [Header("Core")]
    public FrameDispatcher frameDispatcher;
    public ReplayController replayController;

    [Header("Optional WS Client (drag in Inspector)")]
    public WsClient wsClient; // ✅ 不再用 WsClient.Instance，改为拖引用

    [Header("Optional UI Root")]
    public RectTransform uiRoot;

    // ================= Runtime =================
    private string tokenB64 = null;
    private bool loadedSent = false;

    private readonly List<Frame> cachedFrames = new();
    private int cursor = 0;

    private float lastHeight = -1f;

    // ================= Unity =================
    private void Start()
    {
        SendLoadedToFrontend();
        SendResizedIfNeeded(true);
    }

    private void Update()
    {
        SendResizedIfNeeded(false);
    }

    // =================================================
    // ========== Frontend → Player ====================
    // =================================================
    public void HandleMessage(string buffer)
    {
        FrontendData msg;
        try
        {
            msg = JsonUtility.FromJson<FrontendData>(buffer);
        }
        catch
        {
            SendErrorToFrontend("Invalid frontend JSON");
            return;
        }

        switch (msg.message)
        {
            // ---------- Online / Spectator ----------
            case FrontendData.MsgType.init_player_player:
            case FrontendData.MsgType.init_spectator_player:
                ConnectToJudger(msg.token);
                break;

            // ---------- Offline Replay ----------
            case FrontendData.MsgType.init_replay_player:
                cachedFrames.Clear();
                cursor = 0;

                if (string.IsNullOrEmpty(msg.replay_data))
                {
                    SendErrorToFrontend("init_replay_player missing replay_data");
                    return;
                }

                ParseReplayBlob(msg.replay_data);

                if (cachedFrames.Count > 0)
                {
                    frameDispatcher.ApplyFrame(cachedFrames[0]);
                    SendFrameCountToFrontend(cachedFrames.Count);

                    // 如果你想让 ReplayController 也持有这份 frames（可选）
                    if (replayController != null)
                        replayController.Init(cachedFrames);
                }
                break;

            case FrontendData.MsgType.load_frame:
                LoadFrame(msg.index);
                break;

            case FrontendData.MsgType.load_next_frame:
                LoadNextFrame();
                break;
        }
    }

    // =================================================
    // ========== Player → Judger ======================
    // =================================================
    private void ConnectToJudger(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            SendErrorToFrontend("Token is empty");
            return;
        }

        tokenB64 = token;

        try
        {
            string decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));

            // decoded 理论上是 host/path 之类，按你协议拼 wss://
            Connect_ws("wss://" + decoded);

            Send_ws(JsonUtility.ToJson(new JudgerSend
            {
                request = "connect",
                token = tokenB64,
                content = null
            }));
        }
        catch (Exception e)
        {
            SendErrorToFrontend("Connect judger failed: " + e.Message);
        }
    }

    public void SendActionToJudger(string content)
    {
        if (string.IsNullOrEmpty(tokenB64)) return;

        Send_ws(JsonUtility.ToJson(new JudgerSend
        {
            request = "action",
            token = tokenB64,
            content = content
        }));
    }

    // =================================================
    // ========== Judger → Player ======================
    // =================================================
    public void ReceiveWebSocketMessage(string information)
    {
        JudgerRecv recv;
        try
        {
            recv = JsonUtility.FromJson<JudgerRecv>(information);
        }
        catch
        {
            SendErrorToFrontend("Invalid judger JSON");
            return;
        }

        // ⭐ 收到任何消息都可以认为“已连接”
        if (wsClient != null)
            wsClient.MarkConnected();

        switch (recv.request)
        {
            case "action":
            case "watch":
                {
                    // 1) 先当 Frame 解析：能解析就走帧流程
                    if (TryParseFrame(recv.content, out Frame frame))
                    {
                        if (replayController != null)
                            replayController.Record(frame);

                        frameDispatcher.ApplyFrame(frame);
                        cachedFrames.Add(frame);
                    }
                    else
                    {
                        // 2) 解析不了 Frame：说明是 chat/achievement 等非帧回包
                        if (wsClient != null)
                            wsClient.DispatchNonFrameMessage(recv.content);
                        else
                            Debug.LogWarning("[WebInteractionController] Non-frame message but wsClient is null");
                    }
                    break;
                }

            case "history":
                HandleHistory(recv.content);
                break;
        }
    }

    private void HandleHistory(string content)
    {
        cachedFrames.Clear();
        cursor = 0;

        foreach (var s in SimpleJsonArrayParser.ParseStringArray(content))
        {
            if (TryParseFrame(s, out Frame f))
                cachedFrames.Add(f);
        }

        if (cachedFrames.Count > 0)
        {
            frameDispatcher.ApplyFrame(cachedFrames[^1]);
            SendFrameCountToFrontend(cachedFrames.Count);

            if (replayController != null)
                replayController.Init(cachedFrames);
        }
    }

    // =================================================
    // ========== Offline Replay =======================
    // =================================================
    private void ParseReplayBlob(string replayData)
    {
        using var reader = new StringReader(replayData);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (TryParseFrame(line, out Frame frame))
                cachedFrames.Add(frame);
        }
    }

    private void LoadFrame(int index)
    {
        if (cachedFrames.Count == 0) return;

        cursor = Mathf.Clamp(index, 0, cachedFrames.Count - 1);
        frameDispatcher.ApplyFrame(cachedFrames[cursor]);
    }

    private void LoadNextFrame()
    {
        if (cachedFrames.Count == 0) return;

        cursor = Mathf.Clamp(cursor + 1, 0, cachedFrames.Count - 1);
        frameDispatcher.ApplyFrame(cachedFrames[cursor]);
    }

    // =================================================
    // ========== Helpers ==============================
    // =================================================
    private bool TryParseFrame(string json, out Frame frame)
    {
        frame = null;
        if (string.IsNullOrWhiteSpace(json)) return false;

        try
        {
            frame = JsonUtility.FromJson<Frame>(json);

            // 这里别卡太死：有些终局帧 interaction 可能为空
            // 你如果希望“只要能解析就算 Frame”，可以把 interaction 判断去掉
            return frame != null;
        }
        catch
        {
            return false;
        }
    }

    // =================================================
    // ========== Player → Frontend ====================
    // =================================================
    private void SendLoadedToFrontend()
    {
        if (loadedSent) return;
        loadedSent = true;

        SendToFrontend(new FrontendReplyData
        {
            message = FrontendReplyData.MsgType.loaded
        });
    }

    private void SendFrameCountToFrontend(int count)
    {
        SendToFrontend(new FrontendReplyData
        {
            message = FrontendReplyData.MsgType.init_successfully,
            number_of_frames = count,
            init_result = true
        });
    }

    private void SendErrorToFrontend(string msg)
    {
        SendToFrontend(new FrontendReplyData
        {
            message = FrontendReplyData.MsgType.error_marker,
            err_msg = msg
        });
    }

    private void SendResizedIfNeeded(bool force)
    {
        float h = uiRoot ? uiRoot.rect.height : Screen.height;
        if (!force && Mathf.Abs(h - lastHeight) < 1f) return;

        lastHeight = h;
        SendToFrontend(new FrontendReplyData
        {
            message = FrontendReplyData.MsgType.resized,
            height = h
        });
    }

    // 给 WsClient 调用：发送 raw ws payload
    public void SendRawWs(string payload)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Send_ws(payload);
#else
        Debug.Log("[Stub] Send_ws " + payload);
#endif
    }

    private void SendToFrontend(FrontendReplyData reply)
    {
        Send_frontend(JsonUtility.ToJson(reply));
    }

    // =================================================
    // ========== Data Models ==========================
    // =================================================
    [Serializable]
    private class FrontendData
    {
        public MsgType message;
        public string token;
        public string replay_data;
        public int index;

        public enum MsgType
        {
            init_player_player,
            init_replay_player,
            init_spectator_player,
            load_frame,
            load_next_frame
        }
    }

    [Serializable]
    private class FrontendReplyData
    {
        public MsgType message;
        public int number_of_frames;
        public bool init_result;
        public float height;
        public string err_msg;

        public enum MsgType
        {
            loaded,
            init_successfully,
            resized,
            error_marker
        }
    }

    [Serializable]
    private class JudgerSend
    {
        public string request;
        public string token;
        public string content;
    }

    [Serializable]
    private class JudgerRecv
    {
        public string request;
        public string content;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void Connect_ws(string address);
    [DllImport("__Internal")] private static extern void Send_ws(string payload);
    [DllImport("__Internal")] private static extern void Send_frontend(string json);
#else
    private static void Connect_ws(string address) => Debug.Log("[Stub] Connect_ws " + address);
    private static void Send_ws(string payload) => Debug.Log("[Stub] Send_ws " + payload);
    private static void Send_frontend(string json) => Debug.Log("[Stub] Send_frontend " + json);
#endif
}
