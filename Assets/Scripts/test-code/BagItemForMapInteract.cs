using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;


public class BagItemForMapInteract : MonoBehaviour
{
    [Header("UI引用（Inspector拖拽绑定）")]
    public RectTransform bagItemRoot; // BagItemPath 背包根节点

    [Header("外部脚本引用")]
    public BuildingPlacementManager placementManager; // 建筑放置管理器

    // 配置文件固定路径
    private string _jsonPath;
    private string _iniPath;

    // 内存背包数据：key = 物品ID(1~12)，value = 物品数量
    private Dictionary<int, int> _bagItemCount = new Dictionary<int, int>();

    // 建筑消耗字典：key = 建筑ID(1~6)，value = <物品ID, 消耗数量>
    private Dictionary<int, Dictionary<int, int>> _buildCostDict = new Dictionary<int, Dictionary<int, int>>();

    // 物品名称与ID映射（仅前4类参与建造消耗）
    private readonly Dictionary<string, int> _itemNameToId = new Dictionary<string, int>
    {
        {"木头", 1},
        {"石头", 2},
        {"金属单元", 3},
        {"金币", 4}
    };

    // 建筑节名编号与ID映射
    private readonly Dictionary<string, int> _buildNumToId = new Dictionary<string, int>
    {
        {"One", 1},
        {"Two", 2},
        {"Three", 3},
        {"Four", 4},
        {"Five", 5},
        {"Six", 6}
    };

    void Start()
    {
        // 初始化配置文件路径
        _jsonPath = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/MapResource.json");
        _iniPath = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/Build-Status-Description.ini");

        // 加载建造消耗配置 + 初始化背包数据
        LoadIniBuildCost();
        LoadBagDataFromJson();

        Debug.Log("✅ BagItemForMapInteract 初始化完成", this);
    }

    #region INI配置解析：建造消耗提取
        /// <summary>
    /// 从INI的Waste-description字段中解析6类建筑的建造消耗
    /// 仅提取前4类背包物品，后8类不参与建造消耗
    /// </summary>
    private void LoadIniBuildCost()
    {
        if (!File.Exists(_iniPath))
        {
            Debug.LogError($"❌ BagItemForMapInteract：INI配置文件不存在 - {_iniPath}", this);
            return;
        }

        string[] allLines = File.ReadAllLines(_iniPath);
        string currentSection = string.Empty;
        // 正则适配新格式：物品名在前，数字在后，匹配「木头50」「石头30」这类写法
        Regex costRegex = new Regex(@"(木头|石头|金属单元|金币)(\d+)");

        foreach (string line in allLines)
        {
            string trimLine = line.Trim();

            // 跳过空行与注释
            if (string.IsNullOrEmpty(trimLine) || trimLine.StartsWith(";") || trimLine.StartsWith("//"))
                continue;

            // 匹配节名
            if (trimLine.StartsWith("[") && trimLine.EndsWith("]"))
            {
                currentSection = trimLine.Substring(1, trimLine.Length - 2);
                continue;
            }

            // 仅处理生产描述节下的内容
            if (currentSection.Contains("Product-description"))
            {
                string[] kvPair = trimLine.Split(new[] {'='}, 2);
                if (kvPair.Length != 2) continue;

                string key = kvPair[0].Trim();
                // 改为读取专属的消耗配置字段，不再解析展示文案
                if (key != "Waste-description") continue;

                string costText = kvPair[1].Trim().Trim('"');

                // 解析节名获取建筑ID
                string[] sectionParts = currentSection.Split('-');
                if (sectionParts.Length >= 2 && _buildNumToId.TryGetValue(sectionParts[1], out int buildId))
                {
                    Dictionary<int, int> costDict = new Dictionary<int, int>();
                    MatchCollection matches = costRegex.Matches(costText);

                    foreach (Match match in matches)
                    {
                        string itemName = match.Groups[1].Value;
                        int count = int.Parse(match.Groups[2].Value);

                        if (_itemNameToId.TryGetValue(itemName, out int itemId))
                        {
                            costDict[itemId] = count;
                        }
                    }

                    _buildCostDict[buildId] = costDict;
                }
            }
        }

        Debug.Log($"📦 INI消耗解析完成，共{_buildCostDict.Count}类建筑", this);
    }
    #endregion

    #region JSON背包数据自主读写
    /// <summary>
    /// 从MapResource.json读取背包物品数量到内存
    /// </summary>
    private void LoadBagDataFromJson()
    {
        if (!File.Exists(_jsonPath))
        {
            Debug.LogError($"❌ BagItemForMapInteract：JSON文件不存在 - {_jsonPath}", this);
            return;
        }

        try
        {
            string jsonStr = File.ReadAllText(_jsonPath);
            JObject root = JObject.Parse(jsonStr);
            JObject bagNode = root["BagItemCount"] as JObject;

            if (bagNode == null)
            {
                Debug.LogError("❌ JSON中未找到BagItemCount节点", this);
                return;
            }

            _bagItemCount.Clear();
            foreach (var prop in bagNode.Properties())
            {
                string itemKey = prop.Name;
                if (int.TryParse(itemKey.Replace("BagItem-", ""), out int itemId))
                {
                    _bagItemCount[itemId] = (int)prop.Value;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 背包JSON读取失败：{e.Message}", this);
        }
    }

    /// <summary>
    /// 将内存背包数据序列化写回MapResource.json
    /// </summary>
    private void SaveBagDataToJson()
    {
        if (!File.Exists(_jsonPath)) return;

        try
        {
            string jsonStr = File.ReadAllText(_jsonPath);
            JObject root = JObject.Parse(jsonStr);
            JObject bagNode = root["BagItemCount"] as JObject;

            if (bagNode == null) return;

            // 全量更新物品数量
            foreach (var kvp in _bagItemCount)
            {
                bagNode[$"BagItem-{kvp.Key}"] = kvp.Value;
            }

            File.WriteAllText(_jsonPath, root.ToString(Newtonsoft.Json.Formatting.Indented));
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 背包JSON写入失败：{e.Message}", this);
        }
    }
    #endregion

    #region UI更新工具
    /// <summary>
    /// 更新指定物品的数字文本
    /// 查找路径：BagItemPath → BagScrollView → Viewport → Content → BagItem-x → number
    /// 通过BagItem_ID组件精确定位对应物品节点
    /// </summary>
    private void UpdateSingleItemUI(int itemId, int count)
    {
        if (bagItemRoot == null) return;

        // 按层级定位到Content节点
        Transform scrollView = bagItemRoot.Find("BagScrollView");
        Transform viewport = scrollView?.Find("Viewport");
        Transform content = viewport?.Find("Content");
        if (content == null)
        {
            Debug.LogWarning("⚠️ 未找到背包Content节点", this);
            return;
        }

        // 遍历匹配BagItem_ID
        for (int i = 0; i < content.childCount; i++)
        {
            Transform item = content.GetChild(i);
            BagItem_ID idComp = item.GetComponent<BagItem_ID>();
            if (idComp != null && idComp.BagItemID == itemId)
            {
                Text numberText = item.Find("number")?.GetComponent<Text>();
                if (numberText != null)
                {
                    numberText.text = count.ToString();
                }
                break;
            }
        }
    }
    #endregion

    #region 建造资源校验与扣减
    /// <summary>
    /// 校验背包是否满足指定建筑的建造消耗
    /// 供BuildingPlacementManager拖拽前调用
    /// </summary>
    /// <param name="buildId">建筑ID(1~6)</param>
    /// <returns>资源是否充足</returns>
    public bool CheckBuildAffordable(int buildId)
    {
        if (!_buildCostDict.TryGetValue(buildId, out var costDict))
        {
            Debug.LogWarning($"⚠️ 未找到建筑{buildId}的消耗配置", this);
            return false;
        }

        foreach (var cost in costDict)
        {
            int itemId = cost.Key;
            int need = cost.Value;
            if (!_bagItemCount.TryGetValue(itemId, out int have) || have < need)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 扣减建造消耗的对应资源
    /// 建筑放置成功后由BuildingPlacementManager调用
    /// </summary>
    /// <param name="buildId">建筑ID(1~6)</param>
    public void DeductBuildResource(int buildId)
    {
        if (!_buildCostDict.TryGetValue(buildId, out var costDict))
            return;

        // 更新内存数据 + 同步UI
        foreach (var cost in costDict)
        {
            int itemId = cost.Key;
            int need = cost.Value;
            if (_bagItemCount.ContainsKey(itemId))
            {
                _bagItemCount[itemId] = Mathf.Max(0, _bagItemCount[itemId] - need);
                UpdateSingleItemUI(itemId, _bagItemCount[itemId]);
            }
        }

        // 持久化写回JSON
        SaveBagDataToJson();
        Debug.Log($"💰 建造建筑{buildId}，扣除对应背包资源", this);
    }
    #endregion

    #region 建筑产出资源结算
    /// <summary>
    /// 结算建筑产出，增加对应背包物品
    /// 由BuildAnimController.ResetAll()直接调用
    /// </summary>
    /// <param name="buildAnimObj">people-buildAnim 游戏对象</param>
    public void SettleBuildProduct(GameObject buildAnimObj)
    {
        if (buildAnimObj == null) return;

        // 获取父对象：建筑本体build-x
        Transform buildParent = buildAnimObj.transform.parent;
        if (buildParent == null) return;

        // 读取Building组件的产出配置
        Building buildComp = buildParent.GetComponent<Building>();
        if (buildComp == null)
        {
            Debug.LogWarning("⚠️ 未找到Building组件，无法结算产出", this);
            return;
        }

        int itemId = buildComp.ProductItemID;
        int addCount = buildComp.ProductItemCount;

        // 更新内存数据
        if (_bagItemCount.ContainsKey(itemId))
        {
            _bagItemCount[itemId] += addCount;
        }
        else
        {
            _bagItemCount[itemId] = addCount;
        }

        // 同步UI
        UpdateSingleItemUI(itemId, _bagItemCount[itemId]);

        // 持久化写回JSON
        SaveBagDataToJson();

        Debug.Log($"🎁 建筑产出结算：物品{itemId} +{addCount}", this);
    }
    #endregion
}
