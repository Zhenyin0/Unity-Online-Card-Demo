using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 建筑信息面板控制器：数据持久化、状态描述、面板显隐位移、建筑拆除
/// 设计原则：自主读写配置文件，不依赖InitAndLockAllResouce作为数据中介
/// 配套修改说明（必须同步调整才能正常运行）
// Building.cs 新增公共属性
// 在 Building.cs 中添加一行，暴露当前所在区域，供外部读取：
// csharp
// 运行
// public UIRegionCheck CurrentRegion => _currentRegion;
// BuildingPlacementManager 放置成功回调
// 在 BuildingPlacementManager.cs 的 EndDragging 方法中，放置成功分支（_currentPreviewBuilding.ConvertToFormal() 之后）添加调用：
// csharp
// 运行
// // 通知建筑信息面板
// BuildingInfoPath infoPath = FindObjectOfType<BuildingInfoPath>();
// if (infoPath != null)
// {
//     infoPath.OnBuildingPlacedSuccess();
// }
// BuildingPlacementManager 地图切换回调
// 在 BuildingPlacementManager.cs 的 RefreshBuildParent 方法末尾添加调用：
// csharp
// 运行
// BuildingInfoPath infoPath = FindObjectOfType<BuildingInfoPath>();
// if (infoPath != null)
// {
//     infoPath.OnMapSwitchCompleted();
// }

/// 
/// </summary>
public class BuildingInfoPath : MonoBehaviour
{
    [Header("自身UI组件（Inspector拖拽绑定）")]
    public RectTransform panelRect;     // 面板自身RectTransform
    public Text contentText;            // 第一个子对象content的Text组件
    public Button demolishBtn;          // demolish拆除按钮

    [Header("外部脚本引用（Inspector拖拽绑定）")]
    public BuildingPlacementManager placementManager;   // 建筑放置管理器
    public MapSwitchSystem mapSwitchSystem;             // 地图切换系统
    public InitAndLockAllResouce initResouce;           // 仅配合初始化场景使用

    // 配置文件固定路径
    private string _jsonPath;
    private string _iniPath;

    // 建筑描述内存字典：key = 建筑ID(1~6)
    private Dictionary<int, string> _productDescDict = new Dictionary<int, string>();
    private Dictionary<int, string> _waitDescDict = new Dictionary<int, string>();

    // 当前选中建筑运行时数据
    private GameObject _currentBuildObj;
    private Building _currentBuildComp;
    private int _currentBuildId;
    private string _currentMapName;
    private string _currentRegionName;

    // 协程句柄
    private Coroutine _moveCoroutine;
    private Coroutine _autoHideCoroutine;

    // 常量配置
    private const float TOTAL_OFFSET_X = 310f;     // 建筑中心到面板中心的总水平偏移
    private const float MOVE_DURATION = 0.3f;      // 位移动画时长
    private const float AUTO_HIDE_DELAY = 15f;     // 无操作自动隐藏时长
    private const float CANVAS_HALF_WIDTH = 960f;  // 画布水平中线
    private const float WAITE_TIME = 0.1f;      //等待时间
    private void Awake()
    {
        // 初始化配置路径
        _jsonPath = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/MapResource.json");
        _iniPath = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/Build-Status-Description.ini");

        // 加载两份配置文件
        LoadIniConfig();

        // 注册拆除按钮事件
        if (demolishBtn != null)
        {
            demolishBtn.onClick.AddListener(OnDemolishClicked);
        }
    }

    void Start()
    {
        StartCoroutine(WaiteUIRegionInit());
    }

    IEnumerator WaiteUIRegionInit()
    {
        yield return new WaitForSeconds(WAITE_TIME);
        // 仅保留初始地图的建筑按钮收集
        CollectCurrentMapBuildButtons();
        
        // 初始隐藏保持注释，避免影响查找
        gameObject.SetActive(false);
    }

    #region 配置文件读取
    /// <summary>
    /// 读取并解析Build-Status-Description.ini，存入内存字典
    /// </summary>
    private void LoadIniConfig()
    {
        if (!File.Exists(_iniPath))
        {
            Debug.LogError($"❌ BuildingInfoPath：INI配置文件不存在 - {_iniPath}", this);
            return;
        }

        string[] allLines = File.ReadAllLines(_iniPath);
        string currentSection = string.Empty;

        // 编号映射：节名中的One~Six对应建筑ID 1~6
        Dictionary<string, int> idMap = new Dictionary<string, int>
        {
            {"One", 1}, {"Two", 2}, {"Three", 3},
            {"Four", 4}, {"Five", 5}, {"Six", 6}
        };

        foreach (string line in allLines)
        {
            string trimLine = line.Trim();
            // 跳过空行与注释
            if (string.IsNullOrEmpty(trimLine) || trimLine.StartsWith(";") || trimLine.StartsWith("//"))
                continue;

            // 匹配节名 [Build-X-Product-description] / [Build-X-Wait-description]
            if (trimLine.StartsWith("[") && trimLine.EndsWith("]"))
            {
                currentSection = trimLine.Substring(1, trimLine.Length - 2);
                continue;
            }

            // 解析键值对
            if (currentSection.StartsWith("Build-"))
            {
                string[] kvPair = trimLine.Split(new[] {'='}, 2);
                if (kvPair.Length != 2) continue;

                string key = kvPair[0].Trim();
                string value = kvPair[1].Trim().Trim('"'); // 去除首尾引号

                string[] sectionParts = currentSection.Split('-');
                if (sectionParts.Length >= 4)
                {
                    string numKey = sectionParts[1]; // One / Two ...
                    string typeKey = sectionParts[2]; // Product / Wait

                    if (idMap.TryGetValue(numKey, out int buildId))
                    {
                        if (typeKey == "Product" && key == "Product-description")
                        {
                            _productDescDict[buildId] = value;
                        }
                        else if (typeKey == "Wait" && key == "Wait-description")
                        {
                            _waitDescDict[buildId] = value;
                        }
                    }
                }
            }
        }
        Debug.Log($"✅ INI配置加载完成：生产描述{_productDescDict.Count}条，停摆描述{_waitDescDict.Count}条", this);
    }
    #endregion

    #region 建筑按钮收集与事件绑定
    /// <summary>
    /// 收集当前地图RegionToBuild下所有正式建筑的Button组件，绑定点击事件
    /// 初始化、地图切换后调用
    /// </summary>
    public void CollectCurrentMapBuildButtons()
    {
        if (placementManager == null)
            return;

        Transform panelRoot = placementManager.Panel.transform;
        for (int i = 0; i < panelRoot.childCount; i++)
        {
            GameObject child = panelRoot.GetChild(i).gameObject;
            Building buildComp = child.GetComponent<Building>();
            Button buildBtn = child.GetComponent<Button>();
            
            // 缓存当前建筑全量信息
            CacheCurrentBuildInfo(child.gameObject, buildComp);

            // 只处理正式建筑，跳过预览状态
            if (buildComp != null && buildBtn != null && !buildComp.IsPreview)
            {
                buildBtn.onClick.RemoveAllListeners();
                GameObject captureObj = child; // 闭包捕获
                buildBtn.onClick.AddListener(() => OnExistingBuildClicked(captureObj));
            }
        }
    }
    #endregion

    #region 建筑放置成功入口（任务一：JSON数据录入）
    /// <summary>
    /// 建筑放置成功的对外入口，由BuildingPlacementManager调用
    /// </summary>
    public void OnBuildingPlacedSuccess()
    {
        if (placementManager == null || placementManager.Panel == null) return;

        Transform panelRoot = placementManager.Panel.transform;
        if (panelRoot.childCount == 0) return;

        // 获取最新生成的建筑实例（Panel的最后一个子物体）
        Transform lastBuild = panelRoot.GetChild(panelRoot.childCount - 1);
        Building buildComp = lastBuild.GetComponent<Building>();
        if (buildComp == null || buildComp.IsPreview) return;

        // 缓存当前建筑全量信息
        CacheCurrentBuildInfo(lastBuild.gameObject, buildComp);

        // 更新JSON并持久化
        WriteBuildDataToJson();

        // 给新建筑绑定点击事件
        Button buildBtn = lastBuild.GetComponent<Button>();
        if (buildBtn != null)
        {
            buildBtn.onClick.RemoveAllListeners();
            GameObject captureObj = lastBuild.gameObject;
            buildBtn.onClick.AddListener(() => OnExistingBuildClicked(captureObj));
        }

        // 激活面板并执行位移动画
        ShowAndMovePanel(lastBuild.position);
    }

    /// <summary>
    /// 缓存当前选中建筑的基础信息：ID、所属地图、所属区域
    /// </summary>
    private void CacheCurrentBuildInfo(GameObject buildObj, Building buildComp)
    {
        _currentBuildObj = buildObj;
        _currentBuildComp = buildComp;
        _currentBuildId = ParseBuildIdFromName(buildObj.name);
        _currentMapName = GetCurrentActiveMapName();
        _currentRegionName = GetBuildBelongRegionName(buildComp);
    }

    /// <summary>
    /// 从对象名中拆解建筑ID，支持 build-1 / build-1(Clone) 格式
    /// </summary>
    private int ParseBuildIdFromName(string objName)
    {
        Match match = Regex.Match(objName, @"build-(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
            return id;
        
        Debug.LogWarning($"⚠️ 无法解析建筑ID：{objName}", this);
        return 0;
    }

    /// <summary>
    /// 获取当前激活地图的名称
    /// </summary>
    private string GetCurrentActiveMapName()
    {
        if (mapSwitchSystem == null) return string.Empty;
        RectTransform regionToBuild = mapSwitchSystem.GetCurrentRegionToBuild();
        return regionToBuild != null && regionToBuild.parent != null 
            ? regionToBuild.parent.name 
            : string.Empty;
    }

    /// <summary>
    /// 从Building组件获取所属区域名称
    /// 注意：需在Building.cs中新增公共只读属性暴露_currentRegion
    /// </summary>
    private string GetBuildBelongRegionName(Building buildComp)
    {
        // 需配合Building.cs新增：public UIRegionCheck CurrentRegion => _currentRegion;
        UIRegionCheck regionCheck = buildComp.GetType()
            .GetProperty("CurrentRegion")?
            .GetValue(buildComp) as UIRegionCheck;
        
        if (regionCheck == null)
        {
            Debug.LogWarning("⚠️ 无法获取建筑所属区域组件", this);
            return string.Empty;
        }

        // region-collide的父物体即为region-x节点
        return regionCheck.transform.parent != null 
            ? regionCheck.transform.parent.name 
            : regionCheck.gameObject.name;
    }

    /// <summary>
    /// 将建筑数据写入MapResource.json并持久化
    /// </summary>
    private void WriteBuildDataToJson()
    {
        if (!File.Exists(_jsonPath))
        {
            Debug.LogError($"❌ JSON文件不存在：{_jsonPath}", this);
            return;
        }

        try
        {
            string jsonStr = File.ReadAllText(_jsonPath);
            JObject root = JObject.Parse(jsonStr);
            JObject mapNode = root["MapResource"]?[_currentMapName] as JObject;
            JObject regionNode = mapNode?[_currentRegionName] as JObject;

            if (mapNode == null || regionNode == null)
            {
                Debug.LogError($"❌ 未找到数据节点：{_currentMapName} / {_currentRegionName}", this);
                return;
            }

            // 1. 更新地图级 build-number 数组
            JArray buildNumberArr = mapNode["build-number"] as JArray ?? new JArray();
            buildNumberArr.Add(_currentBuildId);
            mapNode["build-number"] = buildNumberArr;

            // 2. 更新区域级 build-ID
            regionNode["build-ID"] = _currentBuildId;

            // 3. 更新区域级 build-position（相对于RegionToBuild的锚点坐标）
            RectTransform buildRect = _currentBuildObj.GetComponent<RectTransform>();
            JArray posArr = new JArray { buildRect.anchoredPosition.x, buildRect.anchoredPosition.y };
            regionNode["build-position"] = posArr;

            // 4. 更新区域级 allow-building 为false
            regionNode["allow-building"] = false;

            // 写回文件
            File.WriteAllText(_jsonPath, root.ToString(Newtonsoft.Json.Formatting.Indented));
            Debug.Log($"✅ 建筑数据已写入JSON：{_currentMapName} - {_currentRegionName}", this);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ JSON写入失败：{e.Message}", this);
        }
    }
    #endregion

    #region 面板位移与显隐逻辑
    /// <summary>
    /// 激活面板并平滑移动到目标建筑旁
    /// </summary>
    private void ShowAndMovePanel(Vector3 buildWorldPos)
    {
        gameObject.SetActive(true);
        UpdateDescriptionContent();

        Vector3 targetPos = CalculateTargetPosition(buildWorldPos);

        // 中断上一次移动
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        
        _moveCoroutine = StartCoroutine(MovePanelCoroutine(targetPos));

        // 重置自动隐藏计时
        ResetAutoHideTimer();
    }

    /// <summary>
    /// 根据建筑位置计算面板目标坐标
    /// </summary>
    private Vector3 CalculateTargetPosition(Vector3 buildWorldPos)
    {
        Vector3 target = buildWorldPos;
        target.z = panelRect.position.z; // 保持Z轴层级不变

        // 左半区：面板在建筑右侧
        if (buildWorldPos.x <= CANVAS_HALF_WIDTH)
            target.x += TOTAL_OFFSET_X;
        // 右半区：面板在建筑左侧
        else
            target.x -= TOTAL_OFFSET_X;

        return target;
    }

    /// <summary>
    /// 平滑动效协程
    /// </summary>
    private IEnumerator MovePanelCoroutine(Vector3 targetPos)
    {
        Vector3 startPos = panelRect.position;
        float elapsed = 0f;

        while (elapsed < MOVE_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / MOVE_DURATION);
            panelRect.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        panelRect.position = targetPos;
        _moveCoroutine = null;
    }

    /// <summary>
    /// 重置15秒自动隐藏计时器
    /// </summary>
    private void ResetAutoHideTimer()
    {
        if (_autoHideCoroutine != null)
            StopCoroutine(_autoHideCoroutine);
        
        _autoHideCoroutine = StartCoroutine(AutoHideCoroutine());
    }

    /// <summary>
    /// 自动隐藏协程
    /// </summary>
    private IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSeconds(AUTO_HIDE_DELAY);
        HidePanel();
    }

    /// <summary>
    /// 隐藏面板并清理所有协程
    /// </summary>
    private void HidePanel()
    {
        gameObject.SetActive(false);

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        if (_autoHideCoroutine != null)
        {
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }
    }
    #endregion

    #region 已有建筑点击交互
    /// <summary>
    /// 点击已有建筑的回调
    /// </summary>
    private void OnExistingBuildClicked(GameObject buildObj)
    {
        // 同建筑已显示 → 隐藏
        if (_currentBuildObj == buildObj && gameObject.activeSelf)
        {
            HidePanel();
            return;
        }

        // 切换建筑：更新缓存、移动面板、不修改JSON
        Building buildComp = buildObj.GetComponent<Building>();
        if (buildComp == null) return;

        CacheCurrentBuildInfo(buildObj, buildComp);
        ShowAndMovePanel(buildObj.transform.position);
    }
    #endregion

    #region 建筑状态描述更新（任务二）
    /// <summary>
    /// 根据建筑子对象激活状态，切换content描述文本
    /// </summary>
    private void UpdateDescriptionContent()
    {
        if (_currentBuildObj == null || contentText == null) return;

        // 按固定子物体顺序判断状态
        Transform firstChild = _currentBuildObj.transform.GetChild(0); // people-buildAnim
        Transform secondChild = _currentBuildObj.transform.GetChild(1); // Item-background

        bool isProducing = firstChild.gameObject.activeSelf && !secondChild.gameObject.activeSelf;
        string targetDesc = string.Empty;

        if (isProducing)
            _productDescDict.TryGetValue(_currentBuildId, out targetDesc);
        else
            _waitDescDict.TryGetValue(_currentBuildId, out targetDesc);

        contentText.text = targetDesc;
    }
    #endregion

    #region 建筑拆除逻辑（任务三）
    // / <summary>
    // / 拆除按钮点击事件
    // / </summary>
    private void OnDemolishClicked()
    {
        if (_currentBuildObj == null || string.IsNullOrEmpty(_currentRegionName))
            return;

        // ========== 新增：解绑建筑按钮事件，清理录入资料，防止空引用 ==========
        Button buildBtn = _currentBuildObj.GetComponent<Button>();
        if (buildBtn != null)
        {
            buildBtn.onClick.RemoveAllListeners();
        }
        // ========== 新增结束 ==========

        // 1. 销毁建筑实例（Building的OnDestroy会自动减少RegionZone计数）
        Destroy(_currentBuildObj);

        // 2. 更新JSON数据
        RemoveBuildDataFromJson();

        // 3. 还原区域建造上限
        RestoreRegionBuildCapacity();

        // 4. 隐藏面板，清理全量关联缓存变量
        HidePanel();
        ClearCurrentBuildCache();

        Debug.Log("🗑️ 建筑已拆除，按钮事件已解绑，数据已同步", this);
    }

    /// <summary>
    /// 从JSON中移除建筑数据
    /// </summary>
    private void RemoveBuildDataFromJson()
    {
        if (!File.Exists(_jsonPath)) return;

        try
        {
            string jsonStr = File.ReadAllText(_jsonPath);
            JObject root = JObject.Parse(jsonStr);
            JObject mapNode = root["MapResource"]?[_currentMapName] as JObject;
            JObject regionNode = mapNode?[_currentRegionName] as JObject;

            if (mapNode == null || regionNode == null) return;

            // 1. 从build-number数组移除对应ID
            JArray buildNumberArr = mapNode["build-number"] as JArray;
            if (buildNumberArr != null)
            {
                for (int i = 0; i < buildNumberArr.Count; i++)
                {
                    if ((int)buildNumberArr[i] == _currentBuildId)
                    {
                        buildNumberArr.RemoveAt(i);
                        break;
                    }
                }
            }

            // 2. 清空build-ID与build-position
            regionNode["build-ID"] = null;
            regionNode["build-position"] = new JArray();

            // 3. 重置allow-building为true
            regionNode["allow-building"] = true;

            // 写回文件
            File.WriteAllText(_jsonPath, root.ToString(Newtonsoft.Json.Formatting.Indented));
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 拆除数据写入失败：{e.Message}", this);
        }
    }

    /// <summary>
    /// 还原对应区域的建造上限为1
    /// </summary>
    private void RestoreRegionBuildCapacity()
    {
        if (mapSwitchSystem == null) return;
        RectTransform regionToBuild = mapSwitchSystem.GetCurrentRegionToBuild();
        if (regionToBuild?.parent == null) return;

        Transform regionTrans = regionToBuild.parent.Find(_currentRegionName);
        Transform collideTrans = regionTrans?.Find("region-collide");
        RegionZone zone = collideTrans?.GetComponent<RegionZone>();

        if (zone != null)
            zone.maxBuildingCount = 1;
    }

    /// <summary>
    /// 清理当前建筑缓存
    /// </summary>
    private void ClearCurrentBuildCache()
    {
        _currentBuildObj = null;
        _currentBuildComp = null;
        _currentBuildId = 0;
        _currentMapName = string.Empty;
        _currentRegionName = string.Empty;
    }
    #endregion

    #region 地图切换适配
    /// <summary>
    /// 地图切换完成后调用，由BuildingPlacementManager的RefreshBuildParent触发
    /// </summary>
    public void OnMapSwitchCompleted()
    {
        HidePanel();
        ClearCurrentBuildCache();
        CollectCurrentMapBuildButtons();
    }
    #endregion
}
