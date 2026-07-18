using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // 用于 UnityAction 类型

public class EnterFightRoomPath : MonoBehaviour
{
    [Header("核心引用")]
    public MapSwitchSystem mapSwitchSystem; // 拖拽赋值：地图切换系统
    public RectTransform panelRect;         // 拖拽赋值：EnterFightRoomPath面板自身的RectTransform
    public float moveSpeed = 5000f;         // 面板平滑移动速度（适配不同设备）
    public float panelOffsetX = 400f;       // 面板与region的X轴偏移量（可在面板调整）
    public float panelOffsetY = 236f; 
    public float fixedPanelY = 0f;          // 面板固定的Y轴坐标（根据UI设计调整）
    public Image fightSenceImage;           // 新增：拖拽赋值FightSenceImage的Image组件
    public Button pathCloseBtn; // 新增：拖拽赋值PathClose按钮
    public Button selectCardBtn;    // 新增：拖拽赋值SelectCard按钮
    public GameObject cardPathRoot; // 新增：拖拽赋值cardPath根节点
    
    [Header("参数切换")]
    public Sprite FightRoom1;
    public Sprite FightRoom2;
    public Sprite FightRoom3;
    public Sprite FightRoom4;

    [Header("调试参数")]
    [SerializeField] private bool isDebug = true;

    // 内部缓存
    private List<RectTransform> _regionList = new List<RectTransform>(); // 存储当前地图的所有region
    private List<Button> _regionBtnList = new List<Button>();            // 存储region对应的Button组件
    private List<UnityAction> _regionClickActions = new List<UnityAction>(); // 新增
    private RectTransform _currentClickedRegion;                         // 记录最后点击的region
    private Vector3 _panelTargetPos;                                     // 面板目标位置
    private bool _isPanelMoving = false;                                 // 面板是否在移动中
    private bool _isPanelActive = false;                                 // 面板当前是否显示
    private bool _isLoadGameData = true;

    void Start()
    {
 
        // 初始化面板状态：隐藏
        if (panelRect != null)
        {
            _isPanelActive = panelRect.gameObject.activeSelf;
            panelRect.gameObject.SetActive(false);
            _isPanelActive = false;
            // 记录面板初始Y轴（也可手动指定fixedPanelY）
            if (fixedPanelY == 0) fixedPanelY = panelRect.position.y;
        }
        
        // 初始化：收集当前激活地图（默认Map1）的region
        if (mapSwitchSystem != null)
        {
            RefreshCurrentMapRegions();
        }
        else
        {
            Debug.LogError("❌ EnterFightRoomPath：未赋值MapSwitchSystem！", this);
        }
        
        if (pathCloseBtn != null)
        {
            pathCloseBtn.onClick.AddListener(HidePanel); // 复用已有HidePanel方法
        }
        else
        {
            Debug.LogError("❌ EnterFightRoomPath：PathClose按钮未赋值！", this);
        }
        
        // 新增：注册SelectCard按钮点击事件（选卡入口）
        if (selectCardBtn != null)
        {
            selectCardBtn.onClick.AddListener(OnSelectCardClicked);
        }
        else
        {
            Debug.LogError("❌ EnterFightRoomPath：SelectCard按钮未赋值！", this);
        }

        if (isDebug) Debug.Log("✅ EnterFightRoomPath初始化完成，当前region数量：" + _regionList.Count, this);
    }

    void Update()
    {
        // 处理面板平滑移动
        if (_isPanelMoving && panelRect != null)
        {
            UpdatePanelPosition();
        }
    }

    /// <summary>
    /// 刷新当前地图的Region（和BuildingPlacementManager的RefreshBuildParent风格一致）
    /// </summary>
    public void RefreshCurrentMapRegions()
    {
        // 1. 清空旧数据和事件
        ClearOldRegionsAndEvents();

        // 2. 获取当前激活的地图对象
        RectTransform currentMap = mapSwitchSystem.GetCurrentRegionToBuild()?.parent.GetComponent<RectTransform>();
        if (currentMap == null)
        {
            Debug.LogWarning("⚠️ EnterFightRoomPath：未找到当前激活的地图！", this);
            return;
        }

        SwitchFightSenceImageByMap(currentMap.name);
        // 3. 遍历当前地图的直接子物体，收集所有region-x
        for (int i = 0; i < currentMap.childCount; i++)
        {
            Transform child = currentMap.GetChild(i);
            if (child.name.StartsWith("region-"))
            {
                RectTransform regionRect = child.GetComponent<RectTransform>();
                Button regionBtn = child.GetComponent<Button>();

                if (regionRect != null && regionBtn != null)
                {
                    _regionList.Add(regionRect);
                    _regionBtnList.Add(regionBtn);
                    
                    // 4. 注册点击事件
                    UnityAction clickAction = () => OnRegionClicked(regionRect);
                    regionBtn.onClick.AddListener(clickAction);
                    _regionClickActions.Add(clickAction);
                }
                else
                {
                    Debug.LogWarning($"⚠️ EnterFightRoomPath：{child.name} 缺少RectTransform/Button组件！", this);
                }
            }
        }

        
        RefreshRegionPermission();

        if (isDebug) Debug.Log($"📋 刷新完成，当前地图{currentMap.name}的region数量：{_regionList.Count}", this);
    }

    /// <summary>
    /// 刷新当前所有区域按钮的交互权限（外部可调用，物体禁用状态也能正常执行）
    /// </summary>
    public void RefreshRegionPermission()
    {
        if (InitAndLockAllResouce.Instance == null)
        {
            // 数据还没就绪，默认全部不可交互，避免误触
            foreach (var btn in _regionBtnList)
            {
                if (btn != null) btn.interactable = false;
            }
            Debug.LogWarning("⚠️ EnterFightRoomPath：权限数据未就绪，按钮暂全部禁用");
            return;
        }

        
        for (int i = 0; i < _regionBtnList.Count; i++)
        {
            if (_regionBtnList[i] == null) continue;
        
            string regionName = _regionList[i].name;
            if (InitAndLockAllResouce.Instance.RegionDict.TryGetValue(regionName, out var regionData))
            {
                if (!regionData.allowPick)
                {
                    _regionBtnList[i].onClick.RemoveListener(_regionClickActions[i]); // 只改括号里的内容
                }
                else
                {
                    // 有权限：先删再加，防止多次刷新导致重复触发
                    _regionBtnList[i].onClick.RemoveListener(_regionClickActions[i]);
                    _regionBtnList[i].onClick.AddListener(_regionClickActions[i]);
                }
                
            }
            else
            {
                //_regionBtnList[i].interactable = false;
                _regionBtnList[i].onClick.RemoveListener(() => OnRegionClicked(_regionList[i]));
                Debug.LogWarning($"⚠️ 未找到区域{regionName}的权限数据，默认不可交互");
            }
        }
        
    }

    /// <summary>
    /// SelectCard按钮点击事件（选卡入口）
    /// </summary>
    private void OnSelectCardClicked()
    {
        if (CardSystemManager.Instance == null)
        {
            Debug.LogError("❌ EnterFightRoomPath：CardSystemManager单例不存在！", this);
            return;
        }
        if (cardPathRoot == null)
        {
            Debug.LogError("❌ EnterFightRoomPath：cardPathRoot未赋值！", this);
            return;
        }
        
        
        // 2. 激活选卡面板根节点
        cardPathRoot.SetActive(true);

        if (_isLoadGameData)
        {
            CardSystemManager.Instance.Inite();
            _isLoadGameData = false;
        }

        // 1. 触发卡牌系统初始化（加载存档/恢复状态）
        
        
        if (isDebug) Debug.Log("🎮 选卡入口已触发，卡牌系统已加载，选卡面板已激活", this);
    }

    /// <summary>
    /// 清空旧的Region数据和事件监听
    /// </summary>
    private void ClearOldRegionsAndEvents()
    {
        // 移除所有Button事件监听
        foreach (Button btn in _regionBtnList)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }
        // 清空列表
        _regionList.Clear();
        _regionBtnList.Clear();
        _regionClickActions.Clear(); // 新增
        // 重置状态
        _currentClickedRegion = null;
        _isPanelMoving = false;
    }

    /// <summary>
    /// Region点击事件核心逻辑
    /// </summary>
    /// <param name="clickedRegion">被点击的Region</param>
    private void OnRegionClicked(RectTransform clickedRegion)
    {
        if (clickedRegion == null || panelRect == null) return;

        // 1. 判断是否是同一个Region重复点击
        bool isSameRegion = clickedRegion == _currentClickedRegion;

        if (isSameRegion)
        {
            if (_isPanelActive)
            {
                // 同一Region+面板显示 → 隐藏面板
                HidePanel();
                if (isDebug) Debug.Log($"🔻 重复点击{clickedRegion.name}，隐藏面板", this);
                return;
            }
            // 同一Region+面板隐藏 → 执行显示+移动逻辑
        }
        else
        {
            // 不同Region → 直接执行显示+移动逻辑
            _currentClickedRegion = clickedRegion;
        }

        // 2. 计算面板目标位置
        _panelTargetPos = CalculatePanelTargetPos(clickedRegion);
        // 3. 显示并移动面板
        ShowPanel();
        _isPanelMoving = true;

        if (isDebug) Debug.Log($"🎯 点击{clickedRegion.name}，面板目标位置：{_panelTargetPos}", this);
        
        // ========== 新增：提取场景信息并调用数据处理方法 ==========
        // 1. 提取当前激活地图名称（region的父物体就是当前地图）
        RectTransform currentMap = clickedRegion.parent.GetComponent<RectTransform>();
        string mapName = currentMap != null ? currentMap.name : "未知地图";

        // 2. 提取被点击的region名称
        string regionName = clickedRegion.name;

        // 3. 提取region第二个子物体的文本内容（索引1为第二个子物体）
        string secondChildText = "无文本";
        if (clickedRegion.childCount >= 2)
        {
            Transform secondChild = clickedRegion.GetChild(1);
            Text textComp = secondChild.GetComponent<Text>();
            if (textComp != null)
            {
                secondChildText = textComp.text;
            }
            else
            {
                Debug.LogWarning($"⚠️ {regionName}的第二个子物体缺少Text组件", this);
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ {regionName}的子物体数量不足2个", this);
        }

        // 4. 调用CollectResourceToInitScene的方法，传递数据
        if (CollectResourceToInitScene.Instance != null)
        {
            CollectResourceToInitScene.Instance.UpdateSceneAndCardData(mapName, regionName, secondChildText);
            CardSystemManager.Instance.ManaAndCardCountToInit();
        }
        else
        {
            Debug.LogError("❌ CollectResourceToInitScene单例未初始化！", this);
        }
        // ========== 新增结束 ==========
    }

    /// <summary>
    /// 计算面板目标位置（严格按坐标规则）
    /// </summary>
    private Vector3 CalculatePanelTargetPos(RectTransform region)
    {
        Vector3 targetPos = Vector3.zero;

        // 1. 获取Region的局部坐标（判断左右）
        float regionLocalX = region.localPosition.x;
        float regionLocalY = region.localPosition.y;
        // 2. 获取Region的世界坐标（计算面板位置）
        Vector3 regionWorldPos = region.position;

        // 3. 计算X轴：右侧-800，左侧+800
        if (regionLocalX > 0)
        {
            targetPos.x = regionWorldPos.x - panelOffsetX;
            if (regionLocalY > 0)
            {
                targetPos.y = fixedPanelY +  panelOffsetY -50;
            }
            else if(regionLocalY < 0)
            {
                targetPos.y = fixedPanelY -  panelOffsetY;
            }
            else
            {
                targetPos.y = fixedPanelY;
            }
            
        }
        else if (regionLocalX < 0)
        {
            targetPos.x = regionWorldPos.x + panelOffsetX;
            if (regionLocalY > 0)
            {
                targetPos.y = fixedPanelY +  panelOffsetY-50;
            }
            else if(regionLocalY < 0)
            {
                targetPos.y = fixedPanelY -  panelOffsetY;
            }
            else
            {
                targetPos.y = fixedPanelY;
            }
        }
        else
        {
            // 居中时默认右偏移（可自定义）
            targetPos.x = regionWorldPos.x + panelOffsetX;
            
            if (regionLocalY > 0)
            {
                targetPos.y = fixedPanelY +  panelOffsetY-50;
            }
            else if(regionLocalY < 0)
            {
                targetPos.y = fixedPanelY -  panelOffsetY;
            }
            else
            {
                targetPos.y = fixedPanelY;
            }
        }
        
        // 5. Z轴和Region保持一致（UI的Z轴不影响）
        targetPos.z = regionWorldPos.z;

        return targetPos;
    }
    
    /// <summary>
    /// 根据当前地图名称切换FightSenceImage的Sprite
    /// </summary>
    /// <param name="mapName">当前激活地图名称</param>
    private void SwitchFightSenceImageByMap(string mapName)
    {
        if (fightSenceImage == null)
        {
            Debug.LogWarning("⚠️ EnterFightRoomPath：FightSenceImage未赋值！", this);
            return;
        }

        switch (mapName)
        {
            case "Map1":
                fightSenceImage.sprite = FightRoom1;
                break;
            case "Map2":
                fightSenceImage.sprite = FightRoom2;
                break;
            case "Map3":
                fightSenceImage.sprite = FightRoom3;
                break;
            case "Map4":
                fightSenceImage.sprite = FightRoom4;
                break;
            default:
                Debug.LogWarning($"⚠️ EnterFightRoomPath：未匹配到地图{mapName}对应的Sprite！", this);
                break;
        }
        fightSenceImage.enabled = true; // 确保Image组件启用
    }

    /// <summary>
    /// 更新面板位置（平滑移动）
    /// </summary>
    private void UpdatePanelPosition()
    {
        // 平滑移动到目标位置
        panelRect.position = Vector3.MoveTowards(
            panelRect.position,
            _panelTargetPos,
            moveSpeed * Time.deltaTime
        );

        // 移动完成
        if (Vector3.Distance(panelRect.position, _panelTargetPos) < 0.1f)
        {
            panelRect.position = _panelTargetPos;
            _isPanelMoving = false;
        }
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    private void ShowPanel()
    {
        RectTransform MapChild = mapSwitchSystem.GetCurrentRegionToBuild()?.parent.GetComponent<RectTransform>();
        Transform child = MapChild.GetChild(0);
        child.gameObject.SetActive(true);
        
        panelRect.gameObject.SetActive(true);
        _isPanelActive = true;
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    private void HidePanel()
    {
        RectTransform MapChild = mapSwitchSystem.GetCurrentRegionToBuild()?.parent.GetComponent<RectTransform>();
        Transform child = MapChild.GetChild(0);
        child.gameObject.SetActive(false);
        
        panelRect.gameObject.SetActive(false);
        _isPanelActive = false;
        _isPanelMoving = false;
    }
}

