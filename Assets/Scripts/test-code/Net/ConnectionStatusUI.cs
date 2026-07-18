using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 连接状态提示UI
/// 挂载位置：各业务场景Canvas下新建空物体「ConnectionStatus」，添加Text组件后拖拽赋值
/// 默认30号字体，右下角对齐
/// </summary>
public class ConnectionStatusUI : MonoBehaviour
{
    public Text statusText;
    public int fontSize = 30;

    void Awake()
    {
        if (statusText != null)
        {
            statusText.fontSize = fontSize;
            statusText.alignment = TextAnchor.LowerRight;
        }
    }

    void Update()
    {
        if (statusText == null || WebSocketManager.Instance == null)
            return;

        string serverName = GetCurrentServerName();
        string state = WebSocketManager.Instance.IsConnected 
            ? "<color=green>已连接</color>" 
            : "<color=red>已断开</color>";
        
        statusText.text = $"当前服务：{serverName} | 连接状态：{state}";
    }

    private string GetCurrentServerName()
    {
        string url = WebSocketManager.Instance.CurrentServer;
        if (string.IsNullOrEmpty(url)) return "未连接";

        if (url.Contains("11012")) return "hall大厅";
        if (url.Contains("11021")) return "map地图";
        if (url.Contains("11080")) return "fight战斗";
        if (url.Contains("11090")) return "battle对战";
        return "未知服务";
    }
}
