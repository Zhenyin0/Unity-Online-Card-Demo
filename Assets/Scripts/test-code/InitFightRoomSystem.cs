using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Collections;
using Unity.VisualScripting;

public class InitFightRoomSystem : MonoBehaviour
{
    public static InitFightRoomSystem Instance { get; private set; }

    [Header("===== 核心父物体引用 =====")]
    public RectTransform postionAnimalPath;   // 角色父节点
    public Transform cardPath;                // 己方案牌父物体（锚点0,0）
    public Transform otherCardPath;           // 敌方案牌父物体（锚点-67,-15）

    [Header("===== UI组件引用 =====")]
    public Image backgroundPath;              // 背景Image组件
    public Text localPositionText;            // 区域文本
    public GameObject allCardUI;              // ALLCard对象
    public GameObject manaUI;                 // ManaUI对象
    public GameObject otherAllCardUI;         // OtherAllCard对象
    public GameObject selfRoundCardUI;        // SelfRoundCard对象

    [Header("===== 资源配置 =====")]
    public Sprite[] mapSprites;               // 4张地图背景，索引0对应Map1，索引3对应Map4
    // public GameObject[] charAnimPrefabs;      // 角色预制体数组，下标对应预制体编号（下标0空置，1对应CharAnim-1，以此类推）
    // public GameObject cardPrefab;             // 卡牌预制体（带HandCard组件）

    // ===== 资源路径配置（替代原手动拖拽字段）=====
    private const string CHAR_PREFAB_ROOT = "assert-scene/CharAnim-prefab";
    private const string CARD_PREFAB_ROOT = "assert-scene/Card-prefab";

    // 卡牌预制体子文件夹（对应你分的8个分类 + Card-orgin）
    private readonly string[] _cardSubFolders = 
    { 
        "DaZhongMa", "HengFeiJi", "KuangXinTu", "moster", 
        "player", "QinShiHuang", "XiGeMa", "YaMaXun"
    };
    
    [Header("===== 常量配置 =====")]
    public int roundManaMax = 10;             // 单回合可用魔力上限
    public int roundCardMax = 5;              // 单回合单角色手牌上限

    // ========== 己方运行时数据 ==========
    public int _totalSelfManaPool;           // 总魔力池
    private int _currentRoundMana;            // 当前回合可用魔力
    private int _totalSelfCardCount;          // 己方总卡数
    private List<int> _selfCharIdList = new List<int>();
    private Dictionary<int, List<int>> _selfCardPools = new Dictionary<int, List<int>>();
    private int _selfCurrentCharIndex;

    private Text _allCardText;
    private Text _manaText;
    private Text _selfRoundCardText;

    // ========== 敌方运行时数据 ==========
    private int _totalOtherCardCount;         // 敌方总卡数
    private List<int> _otherCharIdList = new List<int>();
    private Dictionary<int, List<int>> _otherCardPools = new Dictionary<int, List<int>>();
    private int _otherCurrentCharIndex;

    private Text _otherAllCardText;

    // ========== 跨场景数据缓存 ==========
    private string _currentMapName;

    // 新增：记录当前波次内敌方角色的总数量（用于判断是否轮换完毕）
    private int _currentEnemyRoundCharTotal;
    private string _currentRegionName;
    private string _regionSecondChildText;

    // ========== 敌方运行时数据 ==========
    private List<int> _enemyRoundIdList;          // 敌方所有回合ID列表
    private JObject _currentRegionEnemyConfig;    // 当前区域的完整敌方配置
    private int _currentEnemyRoundIndex;          // 当前敌方回合索引（从0开始）
    // 原有 _totalOtherCardCount、_otherCharIdList 等字段保留不动
    

    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 预获取UI文本组件
        CacheUITextComponents();

        // ===== 一次性初次加载（操作1~8），全场仅执行一次 =====
        DoOneTimeInitialization();
        
    }
    
    private void Start()
    {
        // 所有Awake全部执行完后，再初始化子系统，100%不会出现单例为空
        InitSubSystems();
    }

// 加载角色预制体
private GameObject LoadCharPrefab(int charId)
{
    string prefabName = $"CharAnim-{charId}";
    string fullPath = $"{CHAR_PREFAB_ROOT}/{prefabName}";
    GameObject prefab = Resources.Load<GameObject>(fullPath);
    if (prefab != null) return prefab;
    Debug.LogWarning($"未找到角色预制：Resources/{fullPath}");
    return null;
}

// 加载卡牌预制体
private GameObject LoadCardPrefab(int cardId)
{
    string prefabName = $"Card-{cardId}";
    foreach (string folder in _cardSubFolders)
    {
        string fullPath = $"{CARD_PREFAB_ROOT}/{folder}/{prefabName}";
        GameObject prefab = Resources.Load<GameObject>(fullPath);
        if (prefab != null) return prefab;
    }
    Debug.LogWarning($"未找到卡牌预制：Card-{cardId}");
    return null;
}

    #region ===== 一次性初始化核心逻辑 =====
    private void DoOneTimeInitialization()
    {
        // --- 操作1、2：获取跨场景6项数据，构建己方存卡池 ---
        CollectResourceToInitScene dataSource = CollectResourceToInitScene.Instance;
        if (dataSource == null)
        {
            Debug.LogError("[InitFightRoom] 跨场景数据对象不存在，初始化失败");
            return;
        }

        _totalSelfManaPool = dataSource.currentMana;
        _totalSelfCardCount = dataSource.currentCardCount;
        _currentMapName = dataSource.currentMapName;
        _currentRegionName = dataSource.currentRegionName;
        _regionSecondChildText = dataSource.regionSecondChildText;

        // 解析己方角色ID与存卡池
        // 解析己方角色ID与存卡池
        _selfCharIdList.Clear();
        _selfCardPools.Clear();
        int inputCardCount = dataSource.currentCardCount; // 暂存传入值用于校验
        foreach (var kvp in dataSource.charCardDict)
        {
            if (int.TryParse(kvp.Key.Replace("char-", ""), out int charId))
            {
                _selfCharIdList.Add(charId);
                _selfCardPools[charId] = new List<int>(kvp.Value);
            }
        }
        _selfCurrentCharIndex = 0;

        // 以真实卡池总数为准，避免UI显示与实际可抽牌数不一致
        int realTotalCard = 0;
        foreach (var cardPool in _selfCardPools.Values)
        {
            realTotalCard += cardPool.Count;
        }
        _totalSelfCardCount = realTotalCard;

        // 传入值与真实值不一致时打警告，方便排查配置错误
        if (realTotalCard != inputCardCount)
        {
            Debug.LogWarning($"[InitFightRoom] 己方总卡数校验不一致：传入值{inputCardCount}，真实卡池总数{realTotalCard}，已自动以真实卡池为准");
        }

        // --- 操作3：解析敌方配置，构建敌方存卡池 ---
        LoadEnemyConfigAndInitPools();

        // --- 操作4：背景切换 + 区域文本初始化 ---
        InitBackgroundAndRegionText();

        // --- 操作5、6：己方总卡数 + 魔力双池初始化 ---
        InitSelfBaseUI();

        // --- 操作7：敌方总卡数UI初始化 ---
        InitOtherBaseUI();

        // --- 操作8：角色预制体实例化 + 位置排布 ---
        SpawnAllCharacters();
    }

    // 操作3：解析敌方JSON配置
    // 操作3：解析敌方JSON配置，预加载全部回合
    private void LoadEnemyConfigAndInitPools()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/MapRegionEnemyCardInit.json");
        if (!File.Exists(configPath))
        {
            Debug.LogError($"[InitFightRoom] 敌方配置文件不存在：{configPath}");
            return;
        }

        try
        {
            string jsonText = File.ReadAllText(configPath);
            JObject root = JObject.Parse(jsonText);

            if (!root.TryGetValue(_currentRegionName, out JToken regionToken))
            {
                Debug.LogError($"[InitFightRoom] 未找到区域配置：{_currentRegionName}");
                return;
            }
            _currentRegionEnemyConfig = regionToken as JObject;
            
            // 缓存全部回合ID列表
            _enemyRoundIdList = _currentRegionEnemyConfig["roundId"].ToObject<List<int>>();
            _currentEnemyRoundIndex = 0;

            // 加载第0回合的敌方阵容
            LoadEnemyRoundConfig(_currentEnemyRoundIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[InitFightRoom] 敌方配置解析失败：{e.Message}");
        }
    }

    // 加载指定索引的敌方单回合配置
    private void LoadEnemyRoundConfig(int roundIndex)
    {
        if (_enemyRoundIdList == null || roundIndex < 0 || roundIndex >= _enemyRoundIdList.Count)
        {
            Debug.LogWarning($"[InitFightRoom] 敌方回合索引越界：{roundIndex}");
            return;
        }

        int roundId = _enemyRoundIdList[roundIndex];
        string roundKey = $"round-{roundId}";
        if (!_currentRegionEnemyConfig.TryGetValue(roundKey, out JToken roundToken))
        {
            Debug.LogError($"[InitFightRoom] 未找到敌方回合配置：{roundKey}");
            return;
        }
        JObject roundObj = roundToken as JObject;

        // 构建新的敌方角色列表与存卡池
        _otherCharIdList = roundObj["charId"].ToObject<List<int>>();
        _otherCardPools.Clear();
        _totalOtherCardCount = 0;

        foreach (int charId in _otherCharIdList)
        {
            string charKey = $"char-{charId}";
            List<int> cardList = roundObj[charKey].ToObject<List<int>>();
            _otherCardPools[charId] = new List<int>(cardList);
            _totalOtherCardCount += cardList.Count;
        }
        //_otherCurrentCharIndex = 0;

         // 新增：记录当前波次的角色总数
        _currentEnemyRoundCharTotal = _otherCharIdList.Count;

        // 刷新敌方总卡数UI
        if (_otherAllCardText != null)
            _otherAllCardText.text = _totalOtherCardCount.ToString();
    }

    /// <summary>切换到下一轮敌方阵容（供回合状态机调用）</summary>
    public void SwitchToNextEnemyRound()
    {
        _currentEnemyRoundIndex++;
        // 超出配置回合数时循环回到第0轮，可按需改成战斗结束
        if (_currentEnemyRoundIndex >= _enemyRoundIdList.Count)
        {
            _currentEnemyRoundIndex = 0;
        }

        // 销毁旧的敌方角色实例
        DestroyEnemyCharacters();

        // 加载新回合卡池与角色列表
        LoadEnemyRoundConfig(_currentEnemyRoundIndex);

        // 生成新的敌方角色
        SpawnCharGroup(_otherCharIdList, false);
        StartCoroutine(LoadNewCardResource());

    }

    private IEnumerator LoadNewCardResource()
    {
        yield return new WaitForSeconds(1f);
                
        // 新增核心刷新：重新采集角色，清除已销毁物体缓存
        if (CharAnimSystem.Instance != null)
        {
            CharAnimSystem.Instance.Init();
        }


    }

    // 销毁所有敌方角色（通过X坐标正负判断阵营，匹配镜像规则）
    private void DestroyEnemyCharacters()
    {
        if (postionAnimalPath == null) return;
        
        for (int i = postionAnimalPath.childCount - 1; i >= 0; i--)
        {
            RectTransform rect = postionAnimalPath.GetChild(i).GetComponent<RectTransform>();
            if (rect != null && rect.anchoredPosition.x > 0)
            {
                Destroy(postionAnimalPath.GetChild(i).gameObject);
            }
        }
    }

    // 操作4：背景与区域文本
    private void InitBackgroundAndRegionText()
    {
        // 背景：Map尾号-1 = 数组索引
        if (mapSprites != null && mapSprites.Length >= 4)
        {
            if (int.TryParse(_currentMapName.Replace("Map", ""), out int mapNum))
            {
                int index = mapNum - 1;
                if (index >= 0 && index < mapSprites.Length)
                    backgroundPath.sprite = mapSprites[index];
            }
        }

        // 区域文本：提取region数字拼接
        int.TryParse(_currentRegionName.Replace("region-", ""), out int regionNum);
        localPositionText.text = $"{_regionSecondChildText}-{_currentMapName}-{regionNum}";
    }

    // 操作5、6：己方基础UI
    private void InitSelfBaseUI()
    {
        if (_allCardText != null)
            _allCardText.text = _totalSelfCardCount.ToString();

        // 首回合补充魔力到上限
        int firstSupply = Mathf.Min(roundManaMax, _totalSelfManaPool);
        _currentRoundMana = firstSupply;
        _totalSelfManaPool -= firstSupply;

        if (_manaText != null)
            _manaText.text = _currentRoundMana.ToString();
    }

    // 操作7：敌方基础UI
    private void InitOtherBaseUI()
    {
        if (_otherAllCardText != null)
            _otherAllCardText.text = _totalOtherCardCount.ToString();
    }

    // 操作8：批量生成角色并排布
    private void SpawnAllCharacters()
    {
        SpawnCharGroup(_selfCharIdList, true);
        SpawnCharGroup(_otherCharIdList, false);
    }

    private void SpawnCharGroup(List<int> charIdList, bool isSelf)
    {
        int count = charIdList.Count;
        if (count == 0 || postionAnimalPath == null) return;

        List<Vector2> posList = GetCharPosList(count, isSelf);

        for (int i = 0; i < count; i++)
        {
            int charId = charIdList[i];
            if (charId == 1) continue; // 咕哒子跳过实例化
            
            // #if UNITY_EDITOR
            //     int prefabIndex = GetPrefabIndex(charId);
            //     GameObject charPrefab = LoadCharPrefab(prefabIndex);
            // #else
            //     GameObject charPrefab = null; // 非编辑器模式预留，打包前统一替换为Resources/Addressables
            // #endif
            int prefabIndex = GetPrefabIndex(charId);
            GameObject charPrefab = LoadCharPrefab(prefabIndex);

            if (charPrefab == null)
            {
                Debug.LogWarning($"角色ID{charId}对应预制体缺失");
                continue;
            }
            GameObject charObj = Instantiate(charPrefab, postionAnimalPath);
            RectTransform rect = charObj.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = posList[i];

            // 仅2~7号角色执行左右激活
            if (charId >= 2 && charId <= 7)
            {
                Transform right = charObj.transform.Find("ACT-CharAnimRight");
                Transform left = charObj.transform.Find("ACT-CharAnimLeft");

                if (isSelf)
                {
                    right?.gameObject.SetActive(true);
                    left?.gameObject.SetActive(false);
                }
                else
                {
                    right?.gameObject.SetActive(false);
                    left?.gameObject.SetActive(true);
                }
            }
        }
    }

    private int GetPrefabIndex(int charId)
    {
        if (charId is >= 2 and <= 7) return charId - 1;
        if (charId >= 8) return charId;
        return -1;
    }

    private List<Vector2> GetCharPosList(int count, bool isSelf)
    {
        List<Vector2> list = new List<Vector2>();
        switch (count)
        {
            case 1: list.Add(new Vector2(-540, 0)); break;
            case 2:
                list.Add(new Vector2(-300, 0));
                list.Add(new Vector2(-700, 0));
                break;
            case 3:
                list.Add(new Vector2(-265, 0));
                list.Add(new Vector2(-540, 0));
                list.Add(new Vector2(-800, 0));
                break;
        }

        // 敌方镜像取反X
        if (!isSelf)
        {
            for (int i = 0; i < list.Count; i++)
                list[i] = new Vector2(-list[i].x, list[i].y);
        }
        return list;
    }
    #endregion

    #region ===== 子系统初始化控制 =====
    private void InitSubSystems()
    {
        // // 按你设计的顺序调用（示例顺序，需匹配你的业务）
        // RoomRoundStatusSys.Instance.Init();
        // MainCharAndCardSys.Instance.Init();
        // CharAnimSystem.Instance.Init();
        // RoomUIPickSystem.Instance.Init();
        if (RoomRoundStatusSys.Instance == null)
            Debug.LogWarning("RoomRoundStatusSys.Instance 为空，场景里可能没挂这个脚本");
        else
            RoomRoundStatusSys.Instance.Init();

        if (MainCharAndCardSys.Instance == null)
            Debug.LogWarning("MainCharAndCardSys.Instance 为空，场景里可能没挂这个脚本");
        else
            MainCharAndCardSys.Instance.Init();
        
        if (CharAnimSystem.Instance == null)
            Debug.LogWarning("CharAnimSystem.Instance 为空，场景里可能没挂这个脚本");
        else
            CharAnimSystem.Instance.Init();

        if (RoomUIPickSystem.Instance == null)
            Debug.LogWarning("RoomUIPickSystem.Instance 为空，场景里可能没挂这个脚本");
        else
            RoomUIPickSystem.Instance.Init();
    }
    #endregion

    #region ===== 每回合动态加载公共接口（供回合状态机调用） =====
    /// <summary>己方回合开始：清场→抽牌→实例化→更新存卡UI→补魔力</summary>
    public void OnSelfRoundStart()
    {
        if (_selfCharIdList.Count == 0) return;
        int charId = _selfCharIdList[_selfCurrentCharIndex];
        if (!_selfCardPools.ContainsKey(charId)) return;

        // 1. 清空手牌区
        ClearChildren(cardPath);

        // 2. 随机抽牌并从存卡池移除
        List<int> drawCards = DrawRandomCards(_selfCardPools[charId], roundCardMax);
        foreach (int id in drawCards) _selfCardPools[charId].Remove(id);

        // 3. 实例化卡牌到CardPath，位置(0,0)叠放
        SpawnCards(drawCards, cardPath, Vector2.zero);
        
        if (MainCharAndCardSys.Instance == null)
            Debug.LogWarning("MainCharAndCardSys.Instance 为空，场景里可能没挂这个脚本");
        else
        {
            //MainCharAndCardSys.Instance.InitCardDatas(); // 卡牌实例化完成后再采集！
            MainCharAndCardSys.Instance.StartCoroutine(MainCharAndCardSys.Instance.GuidanceAnimationCoroutine());
        }


        // 4. 更新存卡数UI（SelfRoundCard第二个子物体）
        if (_selfRoundCardText != null)
            _selfRoundCardText.text = _selfCardPools[charId].Count.ToString();

        // 5. 从总魔力池补满当回合可用魔力
        int need = roundManaMax - _currentRoundMana;
        int actual = Mathf.Min(need, _totalSelfManaPool);
        _currentRoundMana += actual;
        _totalSelfManaPool -= actual;
        if (_manaText != null) _manaText.text = _currentRoundMana.ToString();
    }

    /// <summary>己方回合结束：统计剩余→回补存卡池→清场→轮换角色</summary>
    /// <summary>己方回合结束：统计剩余→回补存卡池→清场→轮换角色</summary>
    public void OnSelfRoundEnd()
    {
        MainCharAndCardSys.Instance.CardPath.SetActive(false);
        
        if (_selfCharIdList.Count == 0) return;
        int charId = _selfCharIdList[_selfCurrentCharIndex];
        if (!_selfCardPools.ContainsKey(charId)) return;

        // 收集场上剩余卡牌的真实ID，回补到存卡池
        for (int i = 0; i < cardPath.childCount; i++)
        {
            HandCard card = cardPath.GetChild(i).GetComponent<HandCard>();
            if (card != null)
            {
                _selfCardPools[charId].Add(card.cardId);
            }
        }

        ClearChildren(cardPath);

        if (_selfRoundCardText != null)
            _selfRoundCardText.text = _selfCardPools[charId].Count.ToString();

        // 轮换到下一个角色
        _selfCurrentCharIndex = (_selfCurrentCharIndex + 1) % _selfCharIdList.Count;
        
    }
    /// <summary>敌方回合开始：清场→抽牌→实例化</summary>
    public void OnOtherRoundStart()
    {
        if (_otherCharIdList.Count == 0) return;
        int charId = _otherCharIdList[_otherCurrentCharIndex % _otherCharIdList.Count];
        if (!_otherCardPools.ContainsKey(charId)) return;

        ClearChildren(otherCardPath);

        List<int> drawCards = DrawRandomCards(_otherCardPools[charId], roundCardMax);
        foreach (int id in drawCards) _otherCardPools[charId].Remove(id);

        SpawnCards(drawCards, otherCardPath, new Vector2(-67, -15));
    }

    /// <summary>敌方回合结束：统计剩余→回补→清场→轮换角色</summary>
    /// <summary>敌方回合结束：统计剩余→回补→清场→轮换角色</summary>
    public void OnOtherRoundEnd()
    {
        if (_otherCharIdList.Count == 0) return;
        int charId = _otherCharIdList[_otherCurrentCharIndex % _otherCharIdList.Count];
        if (!_otherCardPools.ContainsKey(charId)) return;

        // 收集场上剩余卡牌的真实ID，回补到存卡池
        for (int i = 0; i < otherCardPath.childCount; i++)
        {
            HandCard card = otherCardPath.GetChild(i).GetComponent<HandCard>();
            if (card != null)
            {
                _otherCardPools[charId].Add(card.cardId);
            }
        }

        ClearChildren(otherCardPath);

        //_otherCurrentCharIndex = (_otherCurrentCharIndex + 1) % _otherCharIdList.Count;
        _otherCurrentCharIndex++;
        
    }

    public void switchEnemycard()
    {
        // 新增：判断当前波次的所有角色是否已轮换完毕，是则切换下一波敌方阵容
        if (_enemyRoundIdList.Count > 1)
        {
            if (_otherCurrentCharIndex == _enemyRoundIdList[1] && _currentEnemyRoundCharTotal > 0)
            {
                SwitchToNextEnemyRound();
            }
        }
    }

    /// <summary>己方出牌扣减：总卡数-1，魔力扣消耗</summary>
    public void OnSelfCardPlayed(int manaCost = 0)
    {
        _totalSelfCardCount--;
        if (_allCardText != null) _allCardText.text = _totalSelfCardCount.ToString();

        _currentRoundMana = Mathf.Max(0, _currentRoundMana - manaCost);
        if (_manaText != null) _manaText.text = _currentRoundMana.ToString();
    }

    /// <summary>敌方出牌扣减：总卡数-1</summary>
    public void OnOtherCardPlayed()
    {
        _totalOtherCardCount--;
        if (_otherAllCardText != null) _otherAllCardText.text = _totalOtherCardCount.ToString();
    }
    #endregion

    //模拟敌人出卡的背后流程的函数调用
    public void PlayOtherCard(int playtime)
    {
        StartCoroutine(PlayCardForOther(playtime));
    }

    //模拟敌人出卡的背后流程
    IEnumerator PlayCardForOther(int time)
    {
        if (_totalOtherCardCount == 0)
        {
            yield break;
        }
        while (time > 0)
        {
            yield return new WaitForSeconds(2f);
            HandCard card = otherCardPath.GetChild(time-1).GetComponent<HandCard>();
            int charId = _otherCharIdList[_otherCurrentCharIndex % _otherCharIdList.Count];
            otherCardPath.gameObject.SetActive(true);
            if (card != null)
            {
                OnOtherCardPlayed();
               card.gameObject.SetActive(true);
               if (CharAnimSystem.Instance != null)
               {
                   CharAnimSystem.Instance.ToFightTriggerBattleAnims();
               }
               yield return new WaitForSeconds(3f);
               _otherCardPools[charId].Remove(card.cardId);
               Destroy(card.gameObject); 
            }
            otherCardPath.gameObject.SetActive(false);
            time--;
        }
    }

    #region ===== 工具方法 =====
    private void CacheUITextComponents()
    {
        if (allCardUI) _allCardText = allCardUI.transform.GetChild(0).GetComponent<Text>();
        if (manaUI) _manaText = manaUI.transform.GetChild(0).GetComponent<Text>();
        if (otherAllCardUI) _otherAllCardText = otherAllCardUI.transform.GetChild(0).GetComponent<Text>();
        if (selfRoundCardUI) _selfRoundCardText = selfRoundCardUI.transform.GetChild(1).GetComponent<Text>();
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private List<int> DrawRandomCards(List<int> pool, int count)
    {
        List<int> result = new List<int>();
        int drawCount = Mathf.Min(count, pool.Count);
        List<int> temp = new List<int>(pool);

        for (int i = 0; i < drawCount; i++)
        {
            int idx = Random.Range(0, temp.Count);
            result.Add(temp[idx]);
            temp.RemoveAt(idx);
        }
        return result;
    }

    private void SpawnCards(List<int> cardIdList, Transform parent, Vector2 anchorPos)
    {
        foreach (int id in cardIdList)
        {
        // #if UNITY_EDITOR
        //     GameObject cardPrefab = LoadCardPrefab(id);
        // #else
        //     GameObject cardPrefab = null;
        // #endif

            GameObject cardPrefab = LoadCardPrefab(id);
            if (cardPrefab == null) continue;

            GameObject card = Instantiate(cardPrefab, parent);
            RectTransform rect = card.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = anchorPos;
            if(anchorPos.x == -67) card.SetActive(false);
        }
        
        
    }
    #endregion

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}