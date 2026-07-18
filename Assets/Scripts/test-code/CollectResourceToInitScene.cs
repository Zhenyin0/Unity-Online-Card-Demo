using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
// 新增：引入字典命名空间
using System;

public class CollectResourceToInitScene : MonoBehaviour
{
    public static CollectResourceToInitScene Instance; // 单例
    
    public int currentMana;      // 存储魔力数值
    public int currentCardCount; // 存储卡牌数量
    
    // ========== 新增：场景信息存储 ==========
    public string currentMapName; // 当前激活地图名称（Map1/Map2等）
    public string currentRegionName; // 被点击的region名称（region-1等）
    public string regionSecondChildText; // region第二个子物体的文本内容
    // ========== 新增：卡牌字典存储 ==========
    public Dictionary<string, List<int>> charCardDict = new Dictionary<string, List<int>>();
    
    private string _initFilePath1;
    private string _initFilePath2;
    
    // Start is called before the first frame update
    // ========== 新增：战斗状态存储 ==========
    public bool isBattleWin; // 战斗是否胜利
    public int remainSelfManaPool; // 己方剩余总魔力池
    public string battlePositionText; // 战斗地点文本
// ========== 原有变量保留 ==========

    void Start()
    {
        _initFilePath1 = Path.Combine(Application.persistentDataPath, "cardSystemSave.json");
        _initFilePath2 = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/MapRegionEnemyCardInit.json");
        
    }

     private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // ========== 新增：对外暴露的方法，接收场景信息并触发卡牌解析 ==========
    public void UpdateSceneAndCardData(string mapName, string regionName, string regionSecondText)
    {
        // 1. 存储场景信息
        currentMapName = mapName;
        currentRegionName = regionName;
        regionSecondChildText = regionSecondText;
        
        // 2. 读取并解析卡牌JSON
        LoadAndParseCardSystemSave();
    }

    // ========== 新增：读取并解析cardSystemSave.json ==========
    private void LoadAndParseCardSystemSave()
    {
        if (File.Exists(_initFilePath1))
        {
            try
            {
                string jsonContent = File.ReadAllText(_initFilePath1);
                CardSystemSaveData saveData = JsonUtility.FromJson<CardSystemSaveData>(jsonContent);
                
                // 清空旧数据，转换为指定字典结构
                charCardDict.Clear();
                foreach (var cardData in saveData.cardDatas)
                {
                    string key = $"char-{cardData.charId}";
                    charCardDict[key] = cardData.userCardIds;
                }
                
                Debug.Log($"✅ 卡牌JSON解析完成，共解析{charCardDict.Count}个角色的卡牌数据");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ 解析卡牌JSON失败：{e.Message}", this);
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ 卡牌存档文件不存在：{_initFilePath1}");
        }
    }

    // ========== 新增：JSON反序列化对应的实体类 ==========
    [System.Serializable]
    private class CardSystemSaveData
    {
        public List<CardData> cardDatas;
    }

    [System.Serializable]
    private class CardData
    {
        public int charId;
        public bool isExpanded;
        public List<int> liftedHandCardIds;
        public List<int> userCardIds;
        public bool isUserAreaCard;
    }

}
