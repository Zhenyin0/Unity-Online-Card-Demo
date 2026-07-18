using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI; // 新增：用于访问Text(Legacy)组件

public class CardSystemManager : MonoBehaviour
{
    public static CardSystemManager Instance; // 单例
    public static Text globalEffectText; // 新增：全局效果文本组件缓存
    
    // 常量定义（按你的要求，名称固定）
    public const string SELECT_CARD_PATH = "SelectCardPath";
    public const string USER_CARD_PATH = "UserCardPath";
    public const string VIEWPORT_NAME = "Viewport";
    public const string CONTENT_NAME = "Content";
    public const string CARD_PATH_ROOT = "cardPath"; // 新增：选卡UI根容器
    public const string HAND_CARD_EFFECT = "effect"; // 新增：每个手牌上的效果文本子对象
    public const string EFFECT_CONTENT = "Effectcontent"; // 新增：全局效果展示文本

    public const string CARD_COUNT = "CardCount";

    public const string MANA = "Mana";
    // 保存文件路径
    private string saveFilePath;
    
    // 全局卡牌数据
    [System.Serializable]
    public class CardSaveData
    {
        public int charId; // 角色卡ID（1~7）
        public bool isExpanded; // 是否散开
        public List<int> liftedHandCardIds = new List<int>(); // 抬起的手牌ID列表
        public List<int> userCardIds = new List<int>(); // 用卡区的卡牌ID列表
        public bool isUserAreaCard; // 新增：标记是否是用卡区角色卡
    }
    
    [System.Serializable]
    public class GameSaveData
    {
        public List<CardSaveData> cardDatas = new List<CardSaveData>();
    }
    
    private GameSaveData currentSaveData;

    // 卡牌数量限制配置类
    [System.Serializable]
    public class CardCountConfig
    {
        public int CardlimitCount = 40;
        public int CharlimitCount = 4;
        public int charForCard_1 = 10;
        public int charForCard_2 = 10;
        public int charForCard_3 = 10;
        public int charForCard_4 = 10;
        public int charForCard_5 = 10;
        public int charForCard_6 = 10;
        public int charForCard_7 = 10;
        public int card_1 = 3;
        public int card_2 = 3;
        public int card_3 = 3;
        public int card_4 = 1;
        public int card_5 = 1;
        public int card_6 = 1;
        public int card_7 = 1;
        public int card_8 = 1;
        public int card_9 = 1;
        public int card_10 = 2;
        public int card_11 = 2;
        public int card_12 = 1;
        public int card_13 = 3;
        public int card_14 = 7;
        public int card_15 = 1;
        public int card_16 = 10;
        public int card_17 = 1;
        public int card_18 = 10;
        public int card_19 = 10;
        public int card_20 = 7;
        public int card_21 = 1;
        public int card_22 = 18;
        public int card_23 = 1;
        public int card_24 = 1;
        public int card_25 = 1;
        public int card_26 = 18;
        public int card_27 = 1;
        public int card_28 = 1;
        public int card_29 = 1;
        public int card_30 = 1;
        public int card_31 = 1;
        public int card_32 = 1;
        public int card_33 = 2;
        public int card_34 = 2;
        public int card_35 = 2;
        public int card_36 = 2;
        public int card_37 = 10;
        public int card_38 = 3;
        public int card_39 = 1;
        public int card_40 = 1;
        public int card_41 = 7;
        public int card_42 = 2;
        public int card_43 = 1;
        public int card_44 = 1;
        public int card_45 = 1;
        public int card_46 = 6;
        public int card_47 = 1;
        public int card_48 = 1;
        public int card_49 = 1;
        public int card_50 = 2;
        public int card_51 = 2;
        public int card_52 = 7;
        public int card_53 = 8;
        public int card_54 = 5;
        public int card_55 = 1;
        public int card_56 = 1;
        public int card_57 = 1;
        public int card_58 = 1;
        public int card_59 = 1;
        public int card_60 = 1;
        public int card_61 = 2;
        public int card_62 = 1;
        public int card_63 = 3;
        public int card_64 = 2;
        public int card_65 = 11;
        public int card_66 = 11;
        public int card_67 = 1;
        public int card_68 = 2;
        public int card_69 = 2;
        public int card_70 = 1;
        public int card_71 = 5;
    }

    private string configFilePath;
    public CardCountConfig cardCountConfig;

    void Awake()
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
        
        saveFilePath = Path.Combine(Application.persistentDataPath, "cardSystemSave.json");
        configFilePath = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/cardCountInit.json");
        currentSaveData = new GameSaveData();
        
        LoadCardCountConfig();
    }

    void LoadCardCountConfig()
    {
        if (File.Exists(configFilePath))
        {
            string json = File.ReadAllText(configFilePath);
            cardCountConfig = JsonUtility.FromJson<CardCountConfig>(json);
            Debug.Log("卡牌数量限制配置已加载");
        }
        else
        {
            cardCountConfig = new CardCountConfig();
            Debug.LogWarning("未找到cardCountInit.json，使用默认值");
        }
    }



    public void Inite()
    {
        // 游戏启动时自动加载
        LoadGameData();

        // 新增：初始化全局效果文本缓存（只在启动时查找一次）
        GameObject effectObj = GameObject.Find(EFFECT_CONTENT);
        if (effectObj != null)
        {
            globalEffectText = effectObj.GetComponent<Text>();
        }
        else
        {
            Debug.LogWarning($"未找到全局效果展示对象：{EFFECT_CONTENT}");
        }
    }

    // 工具方法：递归查找指定名称的父对象
    public static Transform FindParentWithName(Transform child, string targetName)
    {
        Transform current = child;
        while (current != null)
        {
            if (current.name == targetName)
                return current;
            current = current.parent;
        }
        return null;
    }

    // 判断卡牌是否在选卡区
    public static bool IsInSelectArea(Transform cardTransform)
    {
        return FindParentWithName(cardTransform, SELECT_CARD_PATH) != null;
    }

    // 判断卡牌是否在用卡区
    public static bool IsInUserArea(Transform cardTransform)
    {
        return FindParentWithName(cardTransform, USER_CARD_PATH) != null;
    }

    // 获取选卡区的Content
    public static Transform GetSelectContent()
    {
        GameObject selectPath = GameObject.Find(SELECT_CARD_PATH);
        if (selectPath == null) return null;
        return selectPath.transform.Find($"{VIEWPORT_NAME}/{CONTENT_NAME}");
    }

    // 获取用卡区的Content
    public static Transform GetUserContent()
    {
        GameObject userPath = GameObject.Find(USER_CARD_PATH);
        if (userPath == null) return null;
        return userPath.transform.Find($"{VIEWPORT_NAME}/{CONTENT_NAME}");
    }

    public static Text GetManaCountText()
    {
        GameObject ManaRoot = GameObject.Find(MANA);
        Transform SecondChild = ManaRoot.transform.GetChild(1);

        // 3. 从第二个子对象身上获取 Text 组件
        Text ManaCountText = SecondChild.GetComponent<Text>();

        // 4. 返回找到的组件（修正了你原代码固定 return null 的致命错误）
        return ManaCountText;
    }
    
    public static Text GetCardCountText()
    {
        GameObject CardCountRoot = GameObject.Find(CARD_COUNT);
        Transform SecondChild = CardCountRoot.transform.GetChild(1);

        // 3. 从第二个子对象身上获取 Text 组件
        Text CardCountText = SecondChild.GetComponent<Text>();

        // 4. 返回找到的组件（修正了你原代码固定 return null 的致命错误）
        return CardCountText;
    }

    /// <summary>
    /// 检查用卡区是否已有指定ID的角色卡
    /// </summary>
    public bool IsCharCardExistInUserArea(int charId)
    {
        Transform userContent = GetUserContent();
        if (userContent == null) return false;

        foreach (Transform child in userContent)
        {
            CharCard charCard = child.GetComponent<CharCard>();
            if (charCard != null && charCard.charId == charId)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 检查用卡区是否已有指定ID的手牌
    /// </summary>
    public bool IsHandCardExistInUserArea(int cardId)
    {
        Transform userContent = GetUserContent();
        if (userContent == null) return false;

        foreach (Transform child in userContent)
        {
            HandCard handCard = child.GetComponent<HandCard>();
            if (handCard != null && handCard.cardId == cardId)
                return true;
        }
        return false;
    }

    public int GetUserAreaCharCount()
    {
        Transform userContent = GetUserContent();
        if (userContent == null) return 0;
        int count = 0;
        foreach (Transform child in userContent)
        {
            if (child.GetComponent<CharCard>() != null) count++;
        }
        return count;
    }

    public int GetUserAreaHandCardCount()
    {
        Transform userContent = GetUserContent();
        if (userContent == null) return 0;
        int count = 0;
        foreach (Transform child in userContent)
        {
            if (child.GetComponent<HandCard>() != null) count++;
        }
        return count;
    }

    public int GetUserAreaHandCardCountByCharId(int charId)
    {
        Transform userContent = GetUserContent();
        if (userContent == null) return 0;
        int count = 0;
        foreach (Transform child in userContent)
        {
            HandCard hc = child.GetComponent<HandCard>();
            if (hc != null && hc.charId == charId) count++;
        }
        return count;
    }

    public int GetUserAreaHandCardCountByCardId(int cardId)
    {
        Transform userContent = GetUserContent();
        if (userContent == null) return 0;
        int count = 0;
        foreach (Transform child in userContent)
        {
            HandCard hc = child.GetComponent<HandCard>();
            if (hc != null && hc.cardId == cardId) count++;
        }
        return count;
    }

    public int GetCharHandCardLimit(int charId)
    {
        switch (charId)
        {
            case 1: return cardCountConfig.charForCard_1;
            case 2: return cardCountConfig.charForCard_2;
            case 3: return cardCountConfig.charForCard_3;
            case 4: return cardCountConfig.charForCard_4;
            case 5: return cardCountConfig.charForCard_5;
            case 6: return cardCountConfig.charForCard_6;
            case 7: return cardCountConfig.charForCard_7;
            default: return 10;
        }
    }

    public int GetCardLimit(int cardId)
    {
        var field = typeof(CardCountConfig).GetField($"card_{cardId}");
        return field != null ? (int)field.GetValue(cardCountConfig) : 1;
    }

    public void RemoveUserCharCardFromSave(int charId)
    {
        for (int i = currentSaveData.cardDatas.Count - 1; i >= 0; i--)
        {
            if (currentSaveData.cardDatas[i].isUserAreaCard && currentSaveData.cardDatas[i].charId == charId)
            {
                currentSaveData.cardDatas.RemoveAt(i);
                break;
            }
        }
    }

    // 保存游戏数据
    public void SaveGameData()
    {
        currentSaveData.cardDatas.Clear();
        
        // 保存用卡区角色卡和对应手牌（支持重复cardId）
        Transform userContent = GetUserContent();
        if (userContent != null)
        {
            List<CharCard> userCharCards = new List<CharCard>();
            foreach (Transform child in userContent)
            {
                CharCard charCard = child.GetComponent<CharCard>();
                if (charCard != null) userCharCards.Add(charCard);
            }
            
            foreach (CharCard charCard in userCharCards)
            {
                CardSaveData data = new CardSaveData();
                data.charId = charCard.charId;
                data.isExpanded = charCard.isExpanded;
                data.liftedHandCardIds = charCard.GetLiftedHandCardIds();
                data.isUserAreaCard = true;
                
                // 收集所有手牌（包括重复）
                foreach (Transform child in userContent)
                {
                    HandCard handCard = child.GetComponent<HandCard>();
                    if (handCard != null && handCard.charId == charCard.charId)
                    {
                        data.userCardIds.Add(handCard.cardId);
                    }
                }
                
                currentSaveData.cardDatas.Add(data);
            }
        }
        
        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"游戏数据已保存到：{saveFilePath}");
        
        // 同步更新卡牌数量显示（仅统计手牌，不含角色卡）
        Text cardCountText = GetCardCountText();
        if (cardCountText != null)
        {
            cardCountText.text = GetUserAreaHandCardCount().ToString();
        }
    }

    // 加载游戏数据
    public void LoadGameData()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("没有找到保存文件，使用默认数据");
            return;
        }
        
        string json = File.ReadAllText(saveFilePath);
        currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
        
        Transform selectContent = GetSelectContent();
        Transform userContent = GetUserContent();
        
        // 清空用卡区
        if (userContent != null)
        {
            foreach (Transform child in userContent)
            {
                Destroy(child.gameObject);
            }
        }
        
        // 1. 恢复选卡区状态
        if (selectContent != null)
        {
            foreach (var data in currentSaveData.cardDatas)
            {
                if (!data.isUserAreaCard)
                {
                    Transform charTransform = selectContent.Find($"Char-{data.charId}");
                    if (charTransform != null)
                    {
                        CharCard charCard = charTransform.GetComponent<CharCard>();
                        charCard?.LoadState(data.isExpanded, data.liftedHandCardIds);
                    }
                }
            }
        }
        
        // 2. 先生成所有用卡区角色卡
        if (userContent != null)
        {
            foreach (var data in currentSaveData.cardDatas)
            {
                if (data.isUserAreaCard)
                {
                    Transform originalChar = selectContent.Find($"Char-{data.charId}");
                    if (originalChar != null)
                    {
                        GameObject newCharCard = Instantiate(originalChar.gameObject, userContent);
                        CharCard newCharComp = newCharCard.GetComponent<CharCard>();
                        RectTransform newCharRect = newCharComp.GetComponent<RectTransform>();
                        
                        newCharComp.isInSelectArea = false;
                        newCharComp.isExpanded = data.isExpanded;
                        float charX = (data.charId - 1) * 293f;
                        Vector3 charPos = new Vector3(charX, 0f, 0f);
                        newCharRect.anchoredPosition = charPos;
                        newCharComp.originalPosition = charPos;
                        
                        newCharComp.LoadState(data.isExpanded, data.liftedHandCardIds);
                    }
                }
            }
        }
        
        // 3. 再生成所有用卡区手牌（保证能找到对应角色卡堆叠）
        if (userContent != null)
        {
            foreach (var data in currentSaveData.cardDatas)
            {
                if (data.isUserAreaCard)
                {
                    foreach (int cardId in data.userCardIds)
                    {
                        Transform originalCard = selectContent.Find($"Card-{cardId}");
                        if (originalCard != null)
                        {
                            GameObject newCard = Instantiate(originalCard.gameObject, userContent);
                            HandCard handCard = newCard.GetComponent<HandCard>();
                            handCard.isInSelectArea = false;
                            handCard.isLifted = false;
                            
                            // 自动堆叠到对应角色卡
                            foreach (Transform child in userContent)
                            {
                                CharCard charCard = child.GetComponent<CharCard>();
                                if (charCard != null && charCard.charId == handCard.charId)
                                {
                                    handCard.rectTransform.anchoredPosition = child.GetComponent<RectTransform>().anchoredPosition;
                                    handCard.transform.SetSiblingIndex(charCard.transform.GetSiblingIndex());
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // 4. 触发全局多米诺效果，保证位置正确
        if (selectContent != null && selectContent.childCount > 0)
        {
            CharCard firstSelect = selectContent.GetChild(0).GetComponent<CharCard>();
            firstSelect?.Invoke("UpdateAllCharCardPositions", 0.1f);
        }
        
        if (userContent != null && userContent.childCount > 0)
        {
            CharCard firstUser = userContent.GetChild(0).GetComponent<CharCard>();
            firstUser?.Invoke("UpdateAllCharCardPositions", 0.1f);
        }
        
        Debug.Log("游戏数据已加载");
        
        // 同步更新卡牌数量显示（仅统计手牌，不含角色卡）
        Text cardCountText = GetCardCountText();
        if (cardCountText != null)
        {
            cardCountText.text = GetUserAreaHandCardCount().ToString();
        }
    }

    // 游戏退出时自动保存
    void OnApplicationQuit()
    {
        GameObject cardPathRoot = GameObject.Find(CARD_PATH_ROOT);
        if (cardPathRoot != null && cardPathRoot.activeSelf)
        {
            SaveGameData();
        }
        else
        {
            Debug.Log("退出游戏时cardPath未激活，保留上次有效存档");
        }
    }
    
    // 选卡完成按钮调用：保存操作 + 隐藏整个cardPath根容器
    public void SelectCardOver()
    {
        // 1. 保存当前所有操作到cardSystemSave.json
        SaveGameData();
        Debug.Log("选卡完成，已保存操作并隐藏整个选卡面板");
    
        // 2. 找到cardPath根对象，切换激活状态为隐藏
        GameObject cardPathRoot = GameObject.Find(CARD_PATH_ROOT);
        if (cardPathRoot != null)
        {
            cardPathRoot.SetActive(false);
            
            // 将魔力值和卡牌数存入 CollectResourceToInitScene
            if (CollectResourceToInitScene.Instance != null)
            {
                Text manaText = GetManaCountText();
                Text cardCountText = GetCardCountText();

                int.TryParse(manaText?.text, out int manaValue);
                int.TryParse(cardCountText?.text, out int cardCountValue);

                CollectResourceToInitScene.Instance.currentMana = manaValue;
                CollectResourceToInitScene.Instance.currentCardCount = cardCountValue;
            }
        }
        else
        {
            Debug.LogWarning($"未找到选卡UI根对象：{CARD_PATH_ROOT}");
        }
    }

    public void ManaAndCardCountToInit()
    {
        // 将魔力值和卡牌数存入 CollectResourceToInitScene
        if (CollectResourceToInitScene.Instance != null)
        {
            Text manaText = GetManaCountText();
            Text cardCountText = GetCardCountText();

            int.TryParse(manaText?.text, out int manaValue);
            int.TryParse(cardCountText?.text, out int cardCountValue);

            CollectResourceToInitScene.Instance.currentMana = manaValue;
            CollectResourceToInitScene.Instance.currentCardCount = cardCountValue;
        }
    }

// 选卡取消按钮调用：放弃保存 + 隐藏整个cardPath根容器（恢复到上次保存状态）
    public void SelectCardCancel()
    {
        // 1. 放弃当前未保存的操作，加载上次保存的状态
        LoadGameData();
        Debug.Log("选卡取消，已放弃未保存操作并隐藏整个选卡面板");
    
        // 2. 找到cardPath根对象，切换激活状态为隐藏
        GameObject cardPathRoot = GameObject.Find(CARD_PATH_ROOT);
        if (cardPathRoot != null)
        {
            cardPathRoot.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"未找到选卡UI根对象：{CARD_PATH_ROOT}");
        }
    }
    // 新增：静态包装器，按钮直接调用这个
    public static void StaticSelectCardOver() => Instance?.SelectCardOver();
    public static void StaticSelectCardCancel() => Instance?.SelectCardCancel();
}