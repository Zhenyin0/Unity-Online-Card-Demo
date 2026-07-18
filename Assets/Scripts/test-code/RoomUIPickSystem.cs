using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间，用于按钮组件

public class RoomUIPickSystem : MonoBehaviour
{
    // 第一步：声明需要绑定的四个公开引用变量（对应拖拽的4个对象）
    [Header("设置界面相关UI引用")]
    [Tooltip("FightTopUIPath下的setting按钮")]
    public Button settingBtn; // 第一个：setting按钮对象
    [Tooltip("Canvas下的SettingPath整个设置界面父节点")]
    public GameObject settingPanel; // 第二个：设置界面父节点
    [Tooltip("DoPath下的ReturnToGame返回游戏按钮")]
    public Button returnToGameBtn; // 第三个：返回游戏按钮
    [Tooltip("DoPath下的PathClose关闭按钮")]
    public Button pathCloseBtn; // 第四个：关闭按钮

    
    public static RoomUIPickSystem Instance;
    // Start方法：初始化状态 + 绑定按钮事件
    void Start()
    {
        // 初始化逻辑已迁移至 Init() 方法
    }

    void Awake()
    {
        Instance = this;
    }
    
    public void Init()
    {
        // 第二步-1：初始状态 - 设置界面默认隐藏（未激活）
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }

        // 绑定按钮点击事件
        BindButtonEvents();
    }

    // Update方法（保留默认结构，暂无额外逻辑）
    void Update()
    {
        
    }

    // 绑定所有按钮的点击事件
    private void BindButtonEvents()
    {
        // 第二步-2：点击setting按钮 → 激活设置界面
        if (settingBtn != null)
        {
            settingBtn.onClick.AddListener(OpenSettingUI);
        }

        // 第二步-3：点击ReturnToGame按钮 → 关闭设置界面
        if (returnToGameBtn != null)
        {
            returnToGameBtn.onClick.AddListener(CloseSettingUI);
        }

        // 第二步-3：点击PathClose按钮 → 关闭设置界面
        if (pathCloseBtn != null)
        {
            pathCloseBtn.onClick.AddListener(CloseSettingUI);
        }
    }

    // 激活（显示）设置界面的方法
    private void OpenSettingUI()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }

    // 关闭（隐藏）设置界面的方法
    private void CloseSettingUI()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }

    // 可选：编辑器下的安全校验（防止空引用）
    // private void OnValidate()
    // {
    //     // 提示未绑定的引用（仅在编辑器模式生效，方便调试）
    //     if (settingBtn == null) Debug.LogWarning("未绑定 settingBtn 引用！", this);
    //     if (settingPanel == null) Debug.LogWarning("未绑定 settingPanel 引用！", this);
    //     if (returnToGameBtn == null) Debug.LogWarning("未绑定 returnToGameBtn 引用！", this);
    //     if (pathCloseBtn == null) Debug.LogWarning("未绑定 pathCloseBtn 引用！", this);
    // }
}
