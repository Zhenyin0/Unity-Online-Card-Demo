using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
    [Header("核心引用")]
    public Dropdown buildingDropdown;      // 建筑选择下拉框
    public Building[] buildingPrefabs;     // 建筑UI预制体（需挂Building脚本）
    public Canvas canvas;                  // 场景主Canvas（限制拖动范围）
    public Image Panel;
    public MapSwitchSystem mapSwitchSystem;
    public Image buildImage;               // 🔥 新增：你的建筑预览图 build-image（关键）

    [Header("预览视觉设置")]
    public Color canPlaceColor = Color.green;
    public Color cannotPlaceColor = Color.red;
    [Range(0.3f, 1f)] public float previewAlpha = 0.7f;

    private int _selectedBuildingIndex = 0;
    private Building _currentPreviewBuilding;
    private bool _isDragging = false;
    private RectTransform _canvasRect;     // Canvas的矩形范围（限制拖动）

    private void Start()
    {
        // 初始化Canvas范围
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
            Debug.LogWarning($"未指定Canvas，自动查找：{canvas?.name}", this);
        }
        _canvasRect = canvas.GetComponent<RectTransform>();

        // 下拉框事件绑定
        
        buildingDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        _selectedBuildingIndex = 0;
        Debug.Log("✅ UI建筑放置系统初始化完成");
        Debug.Log($"📋 当前选中建筑：{buildingPrefabs[_selectedBuildingIndex].name}");
        // 在你原有Start最后一行下面加
        RefreshBuildParent();
    }

    private void Update()
    {
        // 👇 【正确写法】左键按下 + 鼠标指向UI
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 创建点击检测射线（专门检测鼠标下的UI）
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            // 2. 存储射线检测到的所有UI
            System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            // 3. 遍历检测：鼠标是否点击了 build-image
            bool clickBuildImage = false;
            foreach (RaycastResult res in results)
            {
                if (res.gameObject == buildImage.gameObject)
                {
                    clickBuildImage = true;
                    break;
                }
            }

            // 4. 只有点击了 build-image 才触发拖拽
            if (clickBuildImage)
            {
                //========== 追加：背包资源校验 ==========
                bool canBuild = true;
                BagItemForMapInteract bagInteract = FindObjectOfType<BagItemForMapInteract>();
                if (bagInteract != null)
                {
                    // 下拉框索引0对应建筑1，索引+1为建筑ID
                    int buildId = _selectedBuildingIndex + 1;
                    canBuild = bagInteract.CheckBuildAffordable(buildId);
                }

                if (canBuild)
                {
                    StartDragging();
                }
                else
                {
                    Debug.LogWarning("⚠️ 背包资源不足，无法开始建造");
                }
                // ========== 追加结束 ==========
            }
        }

        // 拖拽过程（不变）
        if (_isDragging && _currentPreviewBuilding != null)
        {
            UpdatePreviewPosition();
        }

        // 拖拽结束（不变）
        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            EndDragging();
        }
    }
    
    /// <summary>
    /// 切换地图后调用：自动把建筑挂到当前地图的RegionToBuild
    /// </summary>
    public void RefreshBuildParent()
    {
        if (mapSwitchSystem != null)
        {
            Panel = mapSwitchSystem.GetCurrentRegionToBuild().GetComponent<Image>();
        }
        
    }

    /// <summary>
    /// 开始拖拽：生成预览建筑
    /// </summary>
    private void StartDragging()
    {
        _isDragging = true;
        
        // 生成预览建筑（UI需实例化在Canvas下）
        Vector2 uiPos = GetMouseUIPosition();
        _currentPreviewBuilding = Instantiate(
            buildingPrefabs[_selectedBuildingIndex], 
            Panel.transform,  // 父节点设为Canvas，保证UI层级
            false
        );
        _currentPreviewBuilding.transform.position = uiPos;
        
        // 设置预览状态
        _currentPreviewBuilding.SetAsPreview(canPlaceColor, cannotPlaceColor, previewAlpha);
        Debug.Log($"🖱️ 开始拖拽预览建筑：{buildingPrefabs[_selectedBuildingIndex].name}");
    }

    /// <summary>
    /// 更新预览位置（限制在Canvas内）
    /// </summary>
    private void UpdatePreviewPosition()
    {
        // 1. 获取鼠标的屏幕坐标
        Vector2 targetPos = GetMouseUIPosition();

        // 2. 获取建筑的半宽/半高，确保整个建筑都在屏幕内（不会被裁掉边缘）
        RectTransform buildRect = _currentPreviewBuilding.GetComponent<RectTransform>();
        float halfWidth = buildRect.sizeDelta.x / 2f;
        float halfHeight = buildRect.sizeDelta.y / 2f;

        // 3. 用屏幕尺寸限制坐标，适配Overlay模式
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        targetPos.x = Mathf.Clamp(targetPos.x, halfWidth, screenWidth - halfWidth);
        targetPos.y = Mathf.Clamp(targetPos.y, halfHeight, screenHeight - halfHeight);

        // 4. 更新建筑位置
        buildRect.position = targetPos;
    }

    /// <summary>
    /// 结束拖拽：判断是否放置
    /// </summary>
    private void EndDragging()
    {
        _isDragging = false;

        if (_currentPreviewBuilding == null)
        {
            Debug.LogWarning("⚠️ 拖拽结束但没有预览建筑");
            return;
        }

        // 检查是否可放置
        if (_currentPreviewBuilding.CanPlace)
        {
            _currentPreviewBuilding.ConvertToFormal();
            
            // 【修改】支持查找未激活的面板
            BuildingInfoPath infoPath = FindObjectOfType<BuildingInfoPath>(true);
            
            if (infoPath != null)
            {
                infoPath.OnBuildingPlacedSuccess();
            }
            
            Debug.Log($"🎉 成功放置建筑：{_currentPreviewBuilding.name}");
            
            // ========== 追加：扣减建造资源 ==========
            BagItemForMapInteract bagInteract = FindObjectOfType<BagItemForMapInteract>();
            if (bagInteract != null)
            {
                int buildId = _selectedBuildingIndex + 1;
                bagInteract.DeductBuildResource(buildId);
            }
            // ========== 追加结束 ==========
        }
        else
        {
            Destroy(_currentPreviewBuilding.gameObject);
            Debug.Log("❌ 位置无效，拖拽取消，预览已销毁");
        }

        _currentPreviewBuilding = null;
    }

    /// <summary>
    /// 鼠标世界坐标转Canvas内的UI坐标
    /// </summary>
    private Vector2 GetMouseUIPosition()
    {
        // Screen Space - Overlay的UI，position就是屏幕像素坐标，和Input.mousePosition格式完全匹配
        return Input.mousePosition;
    }
    
    

    /// <summary>
    /// 下拉框切换建筑
    /// </summary>
    private void OnDropdownValueChanged(int index)
    {
        _selectedBuildingIndex = index;
        Debug.Log($"📋 切换选中建筑：{buildingPrefabs[_selectedBuildingIndex].name}");
    }
}