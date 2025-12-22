using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Web / iframe / Saiblo Judger 协议适配器（无 Newtonsoft 版本）
/// 职责：
/// 1. 接收前端 iframe 消息（init / replay / load_frame / load_next_frame）
/// 2. 与评测机 WebSocket 通信（connect / action / history / watch / time）
/// 3. 将评测机 content（Step JSON）转交给 InteractionController / StoryController
/// 4. 将必要信息回发前端（loaded / init_successfully / resized / error）
/// 注意：
/// - JSON 对象 → JsonUtility
/// - JSON 字符串数组 → SimpleJsonArrayParser
/// </summary>
public class WebInteractionController : MonoBehaviour
{
    [Header("Core Controllers")]
    public InteractionController interactionController;
    public ReplayController replayController;
    public StoryController storyController;

    [Header("Optional UI Root")]
    public RectTransform uiRoot;

    // ========== runtime ==========
    private bool loadedSent = false;
    private string tokenB64 = null;

    private FrontendMode currentMode = FrontendMode.None;

    private readonly List<Step> cachedSteps = new();
    private int cursor = 0;

    private float lastHeight = -1f;

    // ========== Unity ==========
    private void Update()
    {
        if (!loadedSent)
        {
            loadedSent = true;
            SendLoadedToFrontend();
            SendResizedIfNeeded(true);
        }

        SendResizedIfNeeded(false);
    }

    // =========================================================
    // ========== Frontend → Player (iframe messages) ==========
    // =========================================================
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
            case FrontendData.MsgType.init_player_player:
                currentMode = FrontendMode.Online;
                ModeController.SwitchToPlay();
                ConnectToJudger(msg.token);
                break;

            case FrontendData.MsgType.init_spectator_player:
                currentMode = FrontendMode.Spectator;
                ModeController.SwitchToReplay();
                ConnectToJudger(msg.token);
                break;

            case FrontendData.MsgType.init_replay_player:
                currentMode = FrontendMode.OfflineReplay;
                ModeController.SwitchToReplay();

                cachedSteps.Clear();
                cursor = 0;

                int frameCount = msg.payload;
                if (frameCount <= 0)
                {
                    SendErrorToFrontend("init_replay_player missing payload(frameCount)");
                    return;
                }

                for (int i = 0; i < frameCount; i++)
                {
                    Getoperation(i);
                }
                break;

            case FrontendData.MsgType.load_frame:
                LoadFrame(msg.index);
                break;

            case FrontendData.MsgType.load_next_frame:
                LoadNextFrame();
                break;

            case FrontendData.MsgType.load_players:
                // 可选：交给 UI
                break;
        }
    }

    // =================================================
    // ========== Player → Judger (WebSocket) ==========
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
            var decoded = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(token)
            );

            string address = "wss://" + decoded;
            Connect_ws(address);

            JudgerSend connect = new JudgerSend
            {
                request = "connect",
                token = tokenB64,
                content = null
            };
            Send_ws(JsonUtility.ToJson(connect));
        }
        catch (Exception e)
        {
            SendErrorToFrontend("Connect judger failed: " + e.Message);
        }
    }

    public void SendActionToJudger(string content)
    {
        if (string.IsNullOrEmpty(tokenB64)) return;

        JudgerSend msg = new JudgerSend
        {
            request = "action",
            token = tokenB64,
            content = content
        };
        Send_ws(JsonUtility.ToJson(msg));
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

        switch (recv.request)
        {
            case "action":
                HandleJudgerAction(recv.content);
                break;

            case "history":
                HandleJudgerHistory(recv.content);
                break;

            case "watch":
                HandleJudgerWatch(recv.content);
                break;

            case "time":
                // 可选：显示倒计时
                break;
        }
    }

    private void HandleJudgerAction(string content)
    {
        if (TryParseStep(content, out Step step))
        {
            ApplyStep(step, true);
        }
    }

    private void HandleJudgerHistory(string content)
    {
        HashSet<string> arr = SimpleJsonArrayParser.ParseStringArray(content);

        cachedSteps.Clear();
        cursor = 0;

        foreach (var s in arr)
        {
            if (TryParseStep(s, out Step step))
                cachedSteps.Add(step);
        }

        if (cachedSteps.Count > 0)
        {
            ModeController.SwitchToReplay();
            ApplyStep(cachedSteps[cachedSteps.Count - 1], true);
        }

        SendFrameCountToFrontend(cachedSteps.Count);
    }

    private void HandleJudgerWatch(string content)
    {
        if (TryParseStep(content, out Step step))
        {
            ModeController.SwitchToReplay();
            cachedSteps.Add(step);
            ApplyStep(step, true);
        }
    }

    // ======================================
    // ========== Offline Replay =============
    // ======================================
    public void HandleOperation(string operation)
    {
        if (TryParseStep(operation, out Step step))
        {
            cachedSteps.Add(step);
        }
    }

    private void LoadFrame(int index)
    {
        if (cachedSteps.Count == 0) return;

        cursor = Mathf.Clamp(index, 0, cachedSteps.Count - 1);
        ApplyStep(cachedSteps[cursor], false);
    }

    private void LoadNextFrame()
    {
        if (cachedSteps.Count == 0) return;

        cursor = Mathf.Clamp(cursor + 1, 0, cachedSteps.Count - 1);
        ApplyStep(cachedSteps[cursor], false);
    }

    // ======================================
    // ========== Apply Step =================
    // ======================================
    private void ApplyStep(Step step, bool record)
    {
        if (step == null || step.interaction == null) return;

        // 1️ 记录（只在 Play 模式下生效）
        if (record && replayController != null)
        {
            replayController.Record(step);
        }

        // 2️ 执行 Interaction（核心）
        interactionController.PlayerInteract(
            step.npc_id,
            step.interaction.ask_content,
            step.interaction.submit_evidence_id
        );
    }

    private bool TryParseStep(string json, out Step step)
    {
        step = null;
        if (string.IsNullOrWhiteSpace(json)) return false;

        try
        {
            step = JsonUtility.FromJson<Step>(json);
            return step != null && step.interaction != null;
        }
        catch
        {
            return false;
        }
    }

    // ======================================
    // ========== Player → Frontend ==========
    // ======================================
    private void SendLoadedToFrontend()
    {
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

    private void SendToFrontend(FrontendReplyData reply)
    {
        string json = JsonUtility.ToJson(reply);
        Send_frontend(json);
    }

    // ======================================
    // ========== Data Models =================
    // ======================================
    private enum FrontendMode
    {
        None,
        Online,
        OfflineReplay,
        Spectator
    }

    [Serializable]
    private class FrontendData
    {
        public MsgType message;
        public string token;
        public int index;
        public int payload;
        public List<string> players;

        public enum MsgType
        {
            init_player_player,
            init_replay_player,
            init_spectator_player,
            load_frame,
            load_next_frame,
            load_players
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

    // ======================================
    // ========== JS bridge ==================
    // ======================================
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void Connect_ws(string address);
    [DllImport("__Internal")] private static extern void Send_ws(string payload);
    [DllImport("__Internal")] private static extern void Send_frontend(string json);
    [DllImport("__Internal")] private static extern void Getoperation(int index);
#else
    private static void Connect_ws(string address) => Debug.Log("[Stub] Connect_ws " + address);
    private static void Send_ws(string payload) => Debug.Log("[Stub] Send_ws " + payload);
    private static void Send_frontend(string json) => Debug.Log("[Stub] Send_frontend " + json);
    private static void Getoperation(int index) => Debug.Log("[Stub] Getoperation " + index);
#endif
}
