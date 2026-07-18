using UnityEngine;
using UnityEngine.UI;

public class LianJiRoomUIPick : MonoBehaviour
{
    [Header("顶部切换按钮")]
    public Button otherButton;
    public Button firendButton;
    public Button messageButton;

    [Header("对应滚动面板")]
    public GameObject otherScrollView;
    public GameObject firendScrollView;
    public GameObject messageScrollView;

    void Start()
    {
        // 给三个按钮绑定点击事件
        if (otherButton != null)
            otherButton.onClick.AddListener(ShowOtherPanel);
        if (firendButton != null)
            firendButton.onClick.AddListener(ShowFirendPanel);
        if (messageButton != null)
            messageButton.onClick.AddListener(ShowMessagePanel);

        // 可选：启动时默认显示第一个面板，不需要可以删掉这行
        ShowOtherPanel();
    }

    /// <summary> 显示Other面板，隐藏另外两个 </summary>
    void ShowOtherPanel()
    {
        otherScrollView.SetActive(true);
        firendScrollView.SetActive(false);
        messageScrollView.SetActive(false);
    }

    /// <summary> 显示好友面板，隐藏另外两个 </summary>
    void ShowFirendPanel()
    {
        otherScrollView.SetActive(false);
        firendScrollView.SetActive(true);
        messageScrollView.SetActive(false);
    }

    /// <summary> 显示消息面板，隐藏另外两个 </summary>
    void ShowMessagePanel()
    {
        otherScrollView.SetActive(false);
        firendScrollView.SetActive(false);
        messageScrollView.SetActive(true);
    }

    // 脚本销毁时移除监听，防止内存泄漏
    private void OnDestroy()
    {
        if (otherButton != null)
            otherButton.onClick.RemoveListener(ShowOtherPanel);
        if (firendButton != null)
            firendButton.onClick.RemoveListener(ShowFirendPanel);
        if (messageButton != null)
            messageButton.onClick.RemoveListener(ShowMessagePanel);
    }
}
