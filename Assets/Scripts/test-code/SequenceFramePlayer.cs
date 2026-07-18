using UnityEngine;
using UnityEngine.UI;

public class SequenceFramePlayer : MonoBehaviour
{
    [Header("序列帧设置")]
    public Sprite[] frames;        // 拖入所有切割好的序列帧
    public float frameRate = 10f;  // 帧率（每秒播放多少帧，10-15足够流畅）
    public float playDuration = 60f; // 总播放时长（秒，这里设为1分钟）

    private Image _uiImage;
    private float _totalTimer;
    private float _frameTimer;
    private int _currentFrame;
    private bool _isPlaying;

    void Awake()
    {
        // 获取Image组件（你的people-buildAnim是UI对象，带Image）
        _uiImage = GetComponent<Image>();
        if (_uiImage == null)
        {
            Debug.LogError("对象上没找到Image组件！");
            enabled = false;
            return;
        }

        if (frames == null || frames.Length == 0)
        {
            Debug.LogError("序列帧数组是空的！");
            enabled = false;
        }
    }

    void OnEnable()
    {
        // 对象激活时自动重置状态，开始播放
        _totalTimer = 0f;
        _frameTimer = 0f;
        _currentFrame = 0;
        _isPlaying = true;
        _uiImage.sprite = frames[0]; // 显示第一帧
    }

    void Update()
    {
        if (!_isPlaying) return;

        // 1. 计时总时长
        _totalTimer += Time.deltaTime;
        if (_totalTimer >= playDuration)
        {
            EndAnimation();
            return;
        }

        // 2. 控制帧切换（循环播放，直到总时长结束）
        _frameTimer += Time.deltaTime;
        float frameInterval = 1f / frameRate;
        if (_frameTimer >= frameInterval)
        {
            _frameTimer -= frameInterval;
            _currentFrame = (_currentFrame + 1) % frames.Length;
            _uiImage.sprite = frames[_currentFrame];
        }
    }

    void EndAnimation()
    {
        _isPlaying = false;
        // 核心：播放结束后切换激活状态（这里是隐藏对象，SetActive(false)）
        gameObject.SetActive(false);
        
        // 如果你想结束后做其他操作（比如通知逻辑），可以在这里加事件/回调
    }

    // 外部调用的播放方法（可选，比如实例化后手动触发）
    public void StartPlay()
    {
        gameObject.SetActive(true);
        OnEnable();
    }
}