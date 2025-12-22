

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerControl : MonoBehaviour
{
    private VideoPlayer videoPlayer;         // VideoPlayer 组件
    public Slider progressBar;               // 控制视频进度的进度条
    public Slider volumeSlider;              // 控制音量的滑动条
    public RawImage showVideo;            // 用于显示视频的 RawImage

    void Start()
    {
        // 获取对应挂载的 VideoPlayer 组件
        videoPlayer = GetComponent<VideoPlayer>();

        //// 设置视频源为本地文件路径（你可以将此路径替换为本地视频路径）
        //string videoPath = "file://" + Application.dataPath + "/Videos/your_video.mp4"; // 替换为实际的视频路径
        //videoPlayer.url = videoPath;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        // 初始化进度条
        progressBar.maxValue = (float)videoPlayer.length;
        progressBar.minValue = 0f;
        progressBar.onValueChanged.AddListener(OnSliderChanged);

        // 设置音量滑块的最大值和最小值
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = 0.8f; // 默认音量为最大
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // 设置视频显示目标为 RawImage
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;  // 只使用 API 渲染
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 16); // 设置视频的渲染目标
        showVideo.texture = videoPlayer.targetTexture;

        videoPlayer.Prepare();
    }

    void Update()
    {
        // 更新进度条
        if (videoPlayer.isPlaying)
        {
            progressBar.value = (float)videoPlayer.time;
        }

        // 控制暂停、播放、音量和进度
        ControlPause();
        ControlVolume();
        ControlProcess();
    }

    // 处理暂停和播放
    private void ControlPause()
    {
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0))  // 按空格键切换播放状态
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
            else
            {
                videoPlayer.Play();
            }
        }
    }

    // 控制音量
    private void ControlVolume()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))  // 按上下键调节音量
        {
            volumeSlider.value += 0.1f;
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            volumeSlider.value -= 0.1f;
        }
    }

    // 控制视频进度（后退和快进）
    private void ControlProcess()
    {
        if (Input.GetKeyUp(KeyCode.LeftArrow))  // 按左箭头后退5秒
        {
            videoPlayer.time -= 5f;
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))  // 按右箭头快进5秒
        {
            videoPlayer.time += 5f;
        }
    }

    // 进度条改变时，更新视频的播放时间
    private void OnSliderChanged(float value)
    {
        videoPlayer.time = value;
    }

    // 音量滑动条改变时，更新音量
    private void OnVolumeChanged(float value)
    {
        videoPlayer.GetTargetAudioSource(0).volume = value;
    }

    // 视频准备完成时调用
    private void OnVideoPrepared(VideoPlayer vp)
    {
        videoPlayer.Play();  // 视频准备好后自动播放
    }
}
