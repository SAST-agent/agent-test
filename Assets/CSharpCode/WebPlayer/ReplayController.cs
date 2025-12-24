using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ReplayController
/// ----------------------------
/// - 缓存所有 FrameDispatcher.Frame
/// - 支持任意跳帧
/// - 支持顺序播放
/// - ❗ 不做任何逻辑
/// - ❗ 只调用 FrameDispatcher
///
/// 兼容层：
/// - Record(Step step) 兼容旧本地逻辑（StoryController/Step）
/// - ReplayAll() 兼容旧测试按钮
/// </summary>
public class ReplayController : MonoBehaviour
{
    [Header("Frames")]
    public List<FrameDispatcher.Frame> frames = new();

    [Header("Dispatcher")]
    public FrameDispatcher frameDispatcher;

    [Header("State")]
    public int currentIndex = -1;
    public bool autoPlay = false;
    public float playInterval = 1.0f;

    private float timer = 0f;

    // =================================================
    // 初始化
    // =================================================
    public void Init(List<FrameDispatcher.Frame> replayFrames)
    {
        frames = replayFrames ?? new List<FrameDispatcher.Frame>();
        currentIndex = -1;
        timer = 0f;

        Debug.Log($"[ReplayController] Init with {frames.Count} frames");
    }

    // =================================================
    // Unity Update（自动播放）
    // =================================================
    private void Update()
    {
        if (!autoPlay || frames == null || frames.Count == 0)
            return;

        timer += Time.deltaTime;
        if (timer >= playInterval)
        {
            timer = 0f;
            StepNext();
        }
    }

    // =================================================
    // ⭐ 前端 / Saiblo 调用
    // =================================================

    /// <summary>
    /// 拖进度条时调用
    /// </summary>
    public void LoadFrame(int index)
    {
        if (frames == null || frames.Count == 0) return;

        if (index < 0 || index >= frames.Count)
        {
            Debug.LogWarning("[ReplayController] Frame index out of range");
            return;
        }

        currentIndex = index;
        ApplyCurrentFrame();
    }

    /// <summary>
    /// 播放下一帧
    /// </summary>
    public void StepNext()
    {
        if (frames == null || frames.Count == 0) return;

        if (currentIndex + 1 >= frames.Count)
        {
            Debug.Log("[ReplayController] Reach end");
            autoPlay = false;
            return;
        }

        currentIndex++;
        ApplyCurrentFrame();
    }

    /// <summary>
    /// 播放上一帧（可选）
    /// </summary>
    public void StepPrev()
    {
        if (frames == null || frames.Count == 0) return;

        if (currentIndex - 1 < 0)
            return;

        currentIndex--;
        ApplyCurrentFrame();
    }

    // =================================================
    // 核心：应用帧
    // =================================================
    private void ApplyCurrentFrame()
    {
        if (frameDispatcher == null)
        {
            Debug.LogError("[ReplayController] FrameDispatcher not assigned");
            return;
        }

        if (frames == null || currentIndex < 0 || currentIndex >= frames.Count)
            return;

        var frame = frames[currentIndex];
        if (frame == null) return;

        Debug.Log($"[ReplayController] Apply frame {frame.step_id}");
        frameDispatcher.ApplyFrame(frame);
    }

    // =================================================
    // 播放控制
    // =================================================
    public void SetAutoPlay(bool play)
    {
        autoPlay = play;
    }

    public int FrameCount => frames == null ? 0 : frames.Count;

    // =================================================
    // ✅ 录制：Frame 版（新对接）
    // =================================================
    public void Record(FrameDispatcher.Frame frame)
    {
        if (frame == null) return;

        frames ??= new List<FrameDispatcher.Frame>();
        frames.Add(frame);

        // 如需录制时自动指向最新帧可打开：
        // currentIndex = frames.Count - 1;
    }

    // =================================================
    // ✅ 兼容旧项目：Record(Step)
    // - 通过 JSON 方式把旧 Step 转成 FrameDispatcher.Frame
    // - 只要字段名一致即可：step_id / npc_id / interaction / result_state
    // =================================================
    public void Record(Step step)
    {
        if (step == null) return;

        try
        {
            string json = JsonUtility.ToJson(step);
            var frame = JsonUtility.FromJson<FrameDispatcher.Frame>(json);
            if (frame != null)
            {
                Record(frame);
            }
            else
            {
                Debug.LogWarning("[ReplayController] Record(Step) convert to Frame failed: frame is null");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ReplayController] Record(Step) convert exception: " + e.Message);
        }
    }

    // =================================================
    // ✅ 兼容旧项目：ReplayAll()
    // - 顺序把所有帧应用一遍（立刻播完）
    // =================================================
    public void ReplayAll()
    {
        if (frames == null || frames.Count == 0)
        {
            Debug.LogWarning("[ReplayController] ReplayAll: no frames");
            return;
        }

        if (frameDispatcher == null)
        {
            Debug.LogError("[ReplayController] ReplayAll: FrameDispatcher not assigned");
            return;
        }

        for (int i = 0; i < frames.Count; i++)
        {
            var f = frames[i];
            if (f != null)
                frameDispatcher.ApplyFrame(f);
        }

        // 同步状态指针
        currentIndex = frames.Count - 1;
    }

    // =================================================
    // 可选：清空录制
    // =================================================
    public void ClearRecord()
    {
        frames?.Clear();
        currentIndex = -1;
        timer = 0f;
        autoPlay = false;
    }
}
