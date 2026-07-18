using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全局资源初始化与数据管理：配置加载、场景状态还原、战后数据同步、关卡进度解锁
/// 数据设计：地图与区域独立字典存储，仅通过归属关系关联，解除实例化强绑定
/// // ========== 追加：战斗入口权限过滤 ==========
// if (InitAndLockAllResouce.Instance != null) --130行EnterFightRoomPath.cs
// {
//     for (int i = 0; i < _regionBtnList.Count; i++)
//     {
//         string regionName = _regionList[i].name;
//         if (InitAndLockAllResouce.Instance.RegionDict.TryGetValue(regionName, out var regionData))
//         {
//             _regionBtnList[i].interactable = regionData.allowPick;
//         }
//         else
//         {
//             Debug.LogWarning($"⚠️ EnterFightRoomPath：未找到区域{regionName}的权限数据，默认不可交互", this);
//             _regionBtnList[i].interactable = false;
//         }
//     }
// }
// else
// {
//     Debug.LogError("❌ EnterFightRoomPath：InitAndLockAllResouce单例未初始化，无法进行权限过滤", this);
//     // 无数据时默认所有按钮不可交互，避免误触
//     foreach (var btn in _regionBtnList)
//     {
//         btn.interactable = false;
//     }
// }// // ========== 追加结束 ==========

/// </summary>
public class InitAndLockAllResouce : MonoBehaviour
{
    
    public static InitAndLockAllResouce Instance;

    [Header("核心引用（Inspector拖拽绑定）")]
    public Text manaText;                 // 魔力值显示文本组件
    public RectTransform bagItemRoot;     // 背包物品根节点（BagItemPath）
    public MapSwitchSystem mapSwitchSystem  ; // 地图切换系统引用

    [Header("地图与建筑预制体")]
    public RectTransform[] mapRoots;      // Map1~Map4 地图根节点，顺序对应 Map1~Map4
    public GameObject[] buildPrefabs;     // 建筑预制体，索引0对应 build-1（ID=1），以此类推

    // 内存数据：独立字典存储，数据解耦
    public int ManaCount { get; private set; }                     // 全局魔力值
    public Dictionary<string, int> BagItemCount { get; private set; } // 背包物品数量字典
    public Dictionary<string, MapData> MapDict { get; private set; }  // 地图数据字典
    public Dictionary<string, RegionData> RegionDict { get; private set; } // 区域数据字典

    private string _configFilePath;

    #region 生命周期与单例
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        
        _configFilePath = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/MapResource.json");
                
        LoadMapResourceConfig();
        ProcessBattleResult();
        
        // 【新增】提前到Awake生成全地图建筑
        SpawnAllBuildings();
    }
    

    void Start()
    {
        
        InitAllSceneElements();
    }
    #endregion

    #region 配置加载与解析
    /// <summary>
    /// 读取并解析 MapResource.json，拆解为独立内存字典
    /// </summary>
    private void LoadMapResourceConfig()
    {
        if (!File.Exists(_configFilePath))
        {
            Debug.LogError($"❌ InitAndLockAllResouce：配置文件不存在：{_configFilePath}");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(_configFilePath);
            JObject root = JObject.Parse(jsonContent);

            // 1. 全局基础数据
            ManaCount = (int)root["ManaCount"];
            BagItemCount = root["BagItemCount"].ToObject<Dictionary<string, int>>();

            // 2. 初始化字典容器
            MapDict = new Dictionary<string, MapData>();
            RegionDict = new Dictionary<string, RegionData>();

            // 3. 遍历解析所有地图与区域
            JObject mapResource = (JObject)root["MapResource"];
            foreach (var mapProp in mapResource.Properties())
            {
                string mapName = mapProp.Name;
                JObject mapObj = (JObject)mapProp.Value;

                // 提取地图基础数据
                MapData mapData = new MapData
                {
                    otherMapJump = (bool)mapObj["other-Map-jump"],
                    buildNumber = mapObj["build-number"].ToObject<List<int>>()
                };
                MapDict.Add(mapName, mapData);

                // 提取该地图下所有区域数据，存入独立区域字典
                foreach (var regionProp in mapObj.Properties())
                {
                    if (!regionProp.Name.StartsWith("region-")) continue;

                    JObject regionObj = (JObject)regionProp.Value;
                    RegionData regionData = new RegionData
                    {
                        allowPick = (bool)regionObj["allow-pick"],
                        enterRestrictMana = (int)regionObj["enter-restrict-mana"],
                        isWin = (bool)regionObj["is-win"],
                        allowBuilding = (bool)regionObj["allow-building"],
                        buildID = regionObj["build-ID"].Type == JTokenType.Null ? null : (int?)regionObj["build-ID"],
                        buildPosition = regionObj["build-position"].ToObject<List<float>>(),
                        rewardItemID = (int)regionObj["reward-item"]["ID"],
                        rewardItemCount = (int)regionObj["reward-item"]["count"],
                        belongMapName = mapName
                    };
                    RegionDict.Add(regionProp.Name, regionData);
                }
            }
            Debug.Log($"✅ 配置加载完成：地图 {MapDict.Count} 张，区域 {RegionDict.Count} 个");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 配置解析失败：{e.Message}");
        }
    }

    /// <summary>
    /// 将内存数据序列化写回 MapResource.json，持久化保存
    /// </summary>
    private void SaveMapResourceConfig()
    {
        try
        {
            JObject root = new JObject
            {
                ["ManaCount"] = ManaCount,
                ["BagItemCount"] = JObject.FromObject(BagItemCount)
            };

            JObject mapResource = new JObject();
            foreach (var mapKvp in MapDict)
            {
                string mapName = mapKvp.Key;
                MapData mapData = mapKvp.Value;

                JObject mapObj = new JObject
                {
                    ["other-Map-jump"] = mapData.otherMapJump,
                    ["build-number"] = JArray.FromObject(mapData.buildNumber)
                };

                // 组装该地图所属的所有区域数据
                foreach (var regionKvp in RegionDict)
                {
                    if (regionKvp.Value.belongMapName != mapName) continue;
                    
                    RegionData regionData = regionKvp.Value;
                    JObject regionObj = new JObject
                    {
                        ["allow-pick"] = regionData.allowPick,
                        ["enter-restrict-mana"] = regionData.enterRestrictMana,
                        ["is-win"] = regionData.isWin,
                        ["allow-building"] = regionData.allowBuilding,
                        ["build-ID"] = regionData.buildID.HasValue ? regionData.buildID.Value : null,
                        ["build-position"] = JArray.FromObject(regionData.buildPosition)
                    };

                    JObject rewardItem = new JObject
                    {
                        ["ID"] = regionData.rewardItemID,
                        ["count"] = regionData.rewardItemCount
                    };
                    regionObj["reward-item"] = rewardItem;

                    mapObj[regionKvp.Key] = regionObj;
                }

                mapResource[mapName] = mapObj;
            }

            root["MapResource"] = mapResource;
            string jsonOutput = root.ToString(Formatting.Indented);
            File.WriteAllText(_configFilePath, jsonOutput);

            Debug.Log("✅ 配置已持久化写入文件");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 配置保存失败：{e.Message}");
        }
    }
    #endregion

    #region 全量场景初始化
    /// <summary>
    /// 一次性初始化所有场景对象状态：UI、区域、建筑、地图权限
    /// </summary>
    private void InitAllSceneElements()
    {
        UpdateManaUI();
        UpdateBagUI();
        InitAllRegionStates();
        // 【删除】SpawnAllBuildings();  已移到Awake中执行 
        UpdateMapSwitchPermission();
    } 

    /// <summary>
    /// 更新魔力值UI显示
    /// </summary>
    private void UpdateManaUI()
    {
        if (manaText == null)
        {
            Debug.LogWarning("⚠️ 魔力值文本未赋值");
            return;
        }
        manaText.text = ManaCount.ToString();
    }

    /// <summary>
    /// 更新背包物品数量UI
    /// </summary>
    private void UpdateBagUI()
    {
        if (bagItemRoot == null)
        {
            Debug.LogWarning("⚠️ 背包根节点未赋值");
            return;
        }

        for (int i = 0; i < bagItemRoot.childCount; i++)
        {
            Transform item = bagItemRoot.GetChild(i);
            if (!item.name.StartsWith("BagItem-")) continue;

            if (BagItemCount.TryGetValue(item.name, out int count))
            {
                Text numberText = item.Find("number")?.GetComponent<Text>();
                if (numberText != null)
                {
                    numberText.text = count.ToString();
                }
            }
        }
    }

    /// <summary>
    /// 初始化所有区域状态：通关星标、建造权限
    /// </summary>
    private void InitAllRegionStates()
    {
        foreach (var mapRect in mapRoots)
        {
            if (mapRect == null) continue;

            for (int i = 0; i < mapRect.childCount; i++)
            {
                Transform region = mapRect.GetChild(i);
                if (!region.name.StartsWith("region-")) continue;

                string regionName = region.name;
                if (!RegionDict.TryGetValue(regionName, out RegionData regionData))
                {
                    Debug.LogWarning($"⚠️ 未找到区域 {regionName} 的数据");
                    continue;
                }

                // 1. 通关星标显隐
                Transform star = region.Find("Star");
                if (star != null)
                {
                    star.gameObject.SetActive(regionData.isWin);
                }

                // 2. 建造容量设置
                Transform regionCollide = region.Find("region-collide");
                if (regionCollide != null)
                {
                    RegionZone zone = regionCollide.GetComponent<RegionZone>();
                    if (zone != null)
                    {
                        zone.maxBuildingCount = regionData.allowBuilding ? 1 : 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 4张地图建筑全量实例化，启动时一次性完成，后续切换不做操作
    /// </summary>
    private void SpawnAllBuildings()
    {
        for (int i = 0; i < mapRoots.Length; i++)
        {
            RectTransform mapRect = mapRoots[i];
            if (mapRect == null) continue;

            string mapName = mapRect.name;
            if (!MapDict.TryGetValue(mapName, out MapData mapData))
            {
                Debug.LogWarning($"⚠️ 未找到地图 {mapName} 的数据");
                continue;
            }

            // 获取建筑父节点
            Transform regionToBuild = mapRect.Find("RegionToBuild");
            if (regionToBuild == null)
            {
                Debug.LogWarning($"⚠️ 地图 {mapName} 未找到 RegionToBuild 节点");
                continue;
            }

            // 第一步：按 build-number 数组实例化对应类型预制体
            Dictionary<int, Queue<GameObject>> buildPool = new Dictionary<int, Queue<GameObject>>();

            foreach (int buildId in mapData.buildNumber)
            {
                int prefabIndex = buildId - 1;
                if (prefabIndex < 0 || prefabIndex >= buildPrefabs.Length || buildPrefabs[prefabIndex] == null)
                {
                    Debug.LogWarning($"⚠️ 建筑ID {buildId} 对应预制体不存在");
                    continue;
                }
    
                GameObject build = Instantiate(buildPrefabs[prefabIndex], regionToBuild);
    
                // 按ID入队
                if (!buildPool.ContainsKey(buildId))
                {
                    buildPool[buildId] = new Queue<GameObject>();
                }
                buildPool[buildId].Enqueue(build);
            }

             // 第二步：遍历区域，按 build-ID 从对应队列中取出实例设置位置
            foreach (var regionKvp in RegionDict)
            {
                if (regionKvp.Value.belongMapName != mapName) continue;
                if (!regionKvp.Value.buildID.HasValue) continue;

                int buildId = regionKvp.Value.buildID.Value;
                List<float> pos = regionKvp.Value.buildPosition;
                if (pos == null || pos.Count < 2) continue;

                // 从对应ID的队列中取出一个实例
                if (!buildPool.ContainsKey(buildId) || buildPool[buildId].Count == 0)
                {
                    Debug.LogWarning($"⚠️ 建筑ID {buildId} 的实例已耗尽，区域 {regionKvp.Key} 无法放置建筑");
                    continue;
                }

                GameObject targetBuild = buildPool[buildId].Dequeue();
                RectTransform buildRect = targetBuild.GetComponent<RectTransform>();
                if (buildRect != null)
                {
                    buildRect.anchoredPosition = new Vector2(pos[0], pos[1]);
                }
            }
        }
        Debug.Log("✅ 全地图建筑实例化完成");
    }

    /// <summary>
    /// 更新地图切换按钮交互权限，按钮权限与「目标地图」的 other-Map-jump 绑定
    /// </summary>
    public void UpdateMapSwitchPermission()
    {
        if (mapSwitchSystem == null)
        {
            Debug.LogWarning("⚠️ MapSwitchSystem 未赋值");
            return;
        }

        // 查找当前激活的地图与索引
        int currentIndex = -1;
        for (int i = 0; i < mapRoots.Length; i++)
        {
            if (mapRoots[i].gameObject.activeSelf)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1) return;

        // ========== 左按钮：跳转到「上一张地图」，权限看上一张地图的 otherMapJump ==========
        bool leftBtnInteractable = false;
        if (currentIndex > 0)
        {
            string leftMapName = mapRoots[currentIndex - 1].name;
            if (MapDict.TryGetValue(leftMapName, out MapData leftMapData))
            {
                leftBtnInteractable = leftMapData.otherMapJump;
            }
        }
        mapSwitchSystem.switchLeftBtn.interactable = leftBtnInteractable;

        // ========== 右按钮：跳转到「下一张地图」，权限看下一张地图的 otherMapJump ==========
        bool rightBtnInteractable = false;
        if (currentIndex < mapRoots.Length - 1)
        {
            string rightMapName = mapRoots[currentIndex].name;
            if (MapDict.TryGetValue(rightMapName, out MapData rightMapData))
            {
                rightBtnInteractable = rightMapData.otherMapJump;
            }
        }
        mapSwitchSystem.switchRightBtn.interactable = rightBtnInteractable;
    }
    #endregion

    #region 战后数据同步与进度联动
    /// <summary>
    /// 战后数据处理入口：接收战斗结果，更新内存并持久化，触发关卡解锁
    /// </summary>
    public void ProcessBattleResult()
    {
        if (CollectResourceToInitScene.Instance == null)
        {
            Debug.LogError("❌ CollectResourceToInitScene 单例不存在，无法处理战后数据");
            return;
        }

        CollectResourceToInitScene battleData = CollectResourceToInitScene.Instance;
        
        string targetRegionName = ToRegionNameForBattle(battleData.battlePositionText);
        
        if (!RegionDict.TryGetValue(targetRegionName, out RegionData regionData))
        {
            Debug.LogWarning($"❌ 未找到目标区域 {targetRegionName}");
            return;
        }

        // 1. 战斗胜利逻辑
        if (battleData.isBattleWin)
        {
            // 更新区域三项状态
            regionData.isWin = true;
            regionData.allowBuilding = true;
            //regionData.allowPick = false;

            // 发放战斗奖励
            string rewardKey = $"BagItem-{regionData.rewardItemID}";
            if (BagItemCount.ContainsKey(rewardKey))
            {
                BagItemCount[rewardKey] += regionData.rewardItemCount;
                Debug.Log($"🎁 区域 {targetRegionName} 胜利，获得 {rewardKey} x{regionData.rewardItemCount}");
            }

            // 解锁下一区域
            UnlockNextRegion(targetRegionName);
            // 检查地图全通关，解锁跳转权限
            CheckMapFullClear(regionData.belongMapName);
        }

        // 2. 更新全局魔力值
        ManaCount = battleData.remainSelfManaPool;

        // 3. 持久化保存
        SaveMapResourceConfig();

        // 4. 刷新当前地图显示状态
        RefreshCurrentMapStates();
    }

    /// <summary>
    /// 解锁序号+1的下一区域战斗权限
    /// </summary>
    private void UnlockNextRegion(string currentRegionName)
    {
        if (!int.TryParse(currentRegionName.Replace("region-", ""), out int currentNum))
        {
            Debug.LogWarning($"⚠️ 无法解析区域编号：{currentRegionName}");
            return;
        }

        string nextRegionName = $"region-{currentNum + 1}";
        if (RegionDict.TryGetValue(nextRegionName, out RegionData nextRegion))
        {
            nextRegion.allowPick = true;
            Debug.Log($"🔓 解锁下一区域：{nextRegionName}");
        }
    }

    /// <summary>
    /// 战斗场景拆解
    /// </summary>
    private string ToRegionNameForBattle(string _targetRegionName)
    {
        // 按 '-' 字符分割字符串
        string[] nameParts = _targetRegionName.Split('-');
    
        // 取分割后的最后一段作为区域编号
        if (nameParts.Length > 0)
        {
            string regionNumber = nameParts[nameParts.Length - 1];
            return $"region-{regionNumber}";
        }

        // 格式异常时的容错：直接返回原字符串
        Debug.LogWarning($"⚠️ 区域名称格式异常，无法拆解：{_targetRegionName}");
        
        return _targetRegionName;
    }


    /// <summary>
    /// 检查地图是否全区域通关，通关则解锁地图跳转权限
    /// </summary>
    private void CheckMapFullClear(string mapName)
    {
        if (!MapDict.TryGetValue(mapName, out MapData mapData)) return;

        bool allWin = true;
        foreach (var regionKvp in RegionDict)
        {
            if (regionKvp.Value.belongMapName == mapName && !regionKvp.Value.isWin)
            {
                allWin = false;
                break;
            }
        }

        if (allWin)
        {
            mapData.otherMapJump = true;
            Debug.Log($"🗺️ 地图 {mapName} 全通关，解锁地图跳转权限");
        }
    }

    /// <summary>
    /// 刷新当前地图的UI与状态（战后增量刷新，避免全量重建）
    /// </summary>
    private void RefreshCurrentMapStates()
    {
        UpdateManaUI();
        UpdateBagUI();

        // 刷新当前激活地图的区域显示
        foreach (var mapRect in mapRoots)
        {
            if (!mapRect.gameObject.activeSelf) continue;

            for (int i = 0; i < mapRect.childCount; i++)
            {
                Transform region = mapRect.GetChild(i);
                if (!region.name.StartsWith("region-")) continue;

                string regionName = region.name;
                if (!RegionDict.TryGetValue(regionName, out RegionData regionData)) continue;

                Transform star = region.Find("Star");
                if (star != null) star.gameObject.SetActive(regionData.isWin);

                Transform regionCollide = region.Find("region-collide");
                if (regionCollide != null)
                {
                    RegionZone zone = regionCollide.GetComponent<RegionZone>();
                    if (zone != null) zone.maxBuildingCount = regionData.allowBuilding ? 1 : 0;
                }
            }
        }

        // 刷新区域按钮权限
        EnterFightRoomPath enterFight = FindObjectOfType<EnterFightRoomPath>();
        if (enterFight != null)
        {
            enterFight.RefreshCurrentMapRegions();
        }

        // 刷新地图切换权限
        UpdateMapSwitchPermission();
    }
    #endregion
}

#region 数据结构定义
/// <summary>
/// 地图基础数据
/// </summary>
[System.Serializable]
public class MapData
{
    public bool otherMapJump;    // 地图跳转权限
    public List<int> buildNumber; // 已建造建筑ID列表
}

/// <summary>
/// 区域完整数据，独立字典存储
/// </summary>
[System.Serializable]
public class RegionData
{
    public bool allowPick;         // 战斗入口权限
    public int enterRestrictMana;  // 进入魔力门槛
    public bool isWin;             // 通关状态
    public bool allowBuilding;     // 建造权限
    public int? buildID;           // 区域内建筑ID
    public List<float> buildPosition; // 建筑放置坐标
    public int rewardItemID;       // 奖励物品ID（原 reward-item 拆解平级）
    public int rewardItemCount;    // 奖励物品数量（原 reward-item 拆解平级）
    public string belongMapName;   // 所属地图名称，用于关联
}
#endregion
