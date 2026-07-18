using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    // 对外只读属性
    public bool IsPreview { get; private set; }
    public bool CanPlace { get; private set; }
    
    // --------------- 【只追加这1个变量】 ---------------
    //private Animator _anim;
    [Header("产出编号和数量")] 
    public int ProductItemCount;
    public int ProductItemID;

    [Header("UI区域检测")] 
    public List<UIRegionCheck> checkRegions; // 可放置的UI区域列表

    private RectTransform _uiRect;
    private Image _imageRenderer;            // UI仅用Image渲染
    private Color _canPlaceColor;
    private Color _cannotPlaceColor;
    private UIRegionCheck _currentRegion;    // 当前所在的UI区域
    
    public UIRegionCheck CurrentRegion => _currentRegion;
    
    // 🔥 只加这1行：绑定子对象的动画图
    private Image _buildAnimImage;

    private void Awake()
    {
        // 初始化UI组件
        _uiRect = GetComponent<RectTransform>();
        _imageRenderer = GetComponent<Image>();

        // 空值检查
        if (_imageRenderer == null)
        {
            Debug.LogError($"❌ 建筑 {gameObject.name} 缺少Image组件！", this);
            enabled = false;
            return;
        }

        // 🔥 只加这1行：自动找子对象 people-buildAnim 的Image
        _buildAnimImage = transform.Find("people-buildAnim").GetComponent<Image>();
        
        
        // ========== 新增：自动找场景里所有的UI区域，不用手动拖 ==========
        if (checkRegions == null || checkRegions.Count == 0)
        {
            // 运行时自动获取场景里所有带UIRegionCheck的对象
            checkRegions = new List<UIRegionCheck>(FindObjectsOfType<UIRegionCheck>());
            Debug.Log($"🔍 建筑 {gameObject.name} 自动绑定了 {checkRegions.Count} 个可放置区域");
        }

        // 默认状态
        IsPreview = false;
        CanPlace = false;
    
    }

    private void Start()
    {
        //_anim = GetComponentInChildren<Animator>();
            UpdatecurrentRegion();
    }

    private void Update()
    {
        // 仅预览状态下检测区域
        if (IsPreview)
        {
            UpdateRegionCheck();
        }
    }
    
    // public void TriggerBuildAnim()
    // {
    //     if(_anim != null)
    //     {
    //         // 只触发一次！就是你要的钩子
    //         _anim.SetTrigger("StartBuild");
    //     }
    // }

    /// <summary>
    /// 设置为预览建筑
    /// </summary>
    public void SetAsPreview(Color canPlace, Color cannotPlace, float alpha)
    {
        IsPreview = true;
        CanPlace = false;
        _canPlaceColor = canPlace;
        _cannotPlaceColor = cannotPlace;

        // 初始化预览视觉
        Color initialColor = _cannotPlaceColor;
        initialColor.a = alpha;
        _imageRenderer.color = initialColor;

        // 🔥 只加这1行：拖拽时子对象完全透明
        if (_buildAnimImage != null)
        {
            Color c = _buildAnimImage.color;
            c.a = 0;
            _buildAnimImage.color = c;
        }
        
        // ====== 加在 SetAsPreview() 方法内部 ======
        BuildAnimController animCtrl = GetComponentInChildren<BuildAnimController>(true);
        if (animCtrl != null)
        {
            animCtrl.enabled = false;
            animCtrl.CancelInvoke(); // 强制取消已启动的计时
        }
        
        Debug.Log($"👁️ 建筑 {gameObject.name} 已设置为预览状态");
        
        
    }

    /// <summary>
    /// 转换为正式建筑
    /// </summary>
    public void ConvertToFormal()
    {
        IsPreview = false;
    
        // 恢复正常颜色
        _imageRenderer.color = Color.white;
    
        // 🔥 只加这1行：放置成功恢复显示
        if (_buildAnimImage != null)
        {
            Color c = _buildAnimImage.color;
            c.a = 1;
            _buildAnimImage.color = c;
        }
        
        // 通知区域增加计数
        if (_currentRegion != null && _currentRegion.regionZone != null)
        {
            _currentRegion.regionZone.AddBuilding();
            Debug.Log($"✅ 预览建筑转正，区域 {_currentRegion.gameObject.name} 数量更新完成");
        }
        
        // ====== 加在 ConvertToFormal() 方法内部 ======
        BuildAnimController animCtrl = GetComponentInChildren<BuildAnimController>(true);
        if (animCtrl != null)
        {
            animCtrl.enabled = true;
            animCtrl.StartProduction(); // 正式放置后才开始生产倒计时
        }
    }

    /// <summary>
    /// 每帧检测当前所在UI区域，更新可放置状态
    /// </summary>
    private void UpdateRegionCheck()
    {
        UpdatecurrentRegion();
        
        // 判断是否可放置：在有效区域内 + 区域有可用位置
        CanPlace = _currentRegion != null && _currentRegion.regionZone != null && _currentRegion.regionZone.HasAvailableSlot;

        // 更新视觉颜色
        Color newColor = CanPlace ? _canPlaceColor : _cannotPlaceColor;
        newColor.a = _imageRenderer.color.a; // 保持透明度
        _imageRenderer.color = newColor;

        Debug.Log($"🔄 预览状态更新：可放置 = {CanPlace}");
    }

    private void OnDestroy()
    {
        // 正式建筑销毁时，通知区域减少计数
        if (!IsPreview && _currentRegion != null && _currentRegion.regionZone != null)
        {
            _currentRegion.regionZone.RemoveBuilding();
            Debug.Log($"🗑️ 正式建筑被销毁，区域 {_currentRegion.gameObject.name} 数量更新完成");
        }
    }

    private void UpdatecurrentRegion()
    {
        UIRegionCheck hitRegion = null;
        // 遍历所有可检测区域，判断是否在区域内
        foreach (var region in checkRegions)
        {
            if (region.IsInRegion(_uiRect.position))
            {
                hitRegion = region;
                break;
            }
        }

        // 更新当前区域
        _currentRegion = hitRegion;
    }

}
