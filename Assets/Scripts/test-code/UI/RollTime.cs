using UnityEngine;
using UnityEngine.UI;

public class RollTime : MonoBehaviour
{
    [Tooltip("目标文本组件，不手动指定则自动查找子物体中的Text")]
    public Text targetText;

    // 当前累计等待秒数
    private int _waitSeconds;
    // 计时协程句柄，用于启停控制
    private Coroutine _timerCoroutine;

    void OnEnable()
    {
        // 没手动指定文本时，自动从子物体中查找第一个Text组件
        if (targetText == null)
        {
            targetText = GetComponentInChildren<Text>();
        }

        // 找不到文本组件则报错退出
        if (targetText == null)
        {
            Debug.LogError("未找到子物体中的Text文本组件，计时功能无法生效", gameObject);
            return;
        }

        // 重置秒数并启动计时
        _waitSeconds = 0;
        UpdateTextDisplay();
        _timerCoroutine = StartCoroutine(TimerTick());
    }

    void OnDisable()
    {
        // 对象禁用时停止计时，避免后台无效运行
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    /// <summary>
    /// 每秒递增一次的计时协程
    /// </summary>
    System.Collections.IEnumerator TimerTick()
    {
        while (true)
        {
            // 等待1秒
            yield return new WaitForSeconds(1f);
            _waitSeconds++;
            UpdateTextDisplay();
        }
    }

    /// <summary>
    /// 更新文本显示内容
    /// </summary>
    void UpdateTextDisplay()
    {
        if (targetText != null)
        {
            targetText.text = _waitSeconds.ToString();
        }
    }
}
