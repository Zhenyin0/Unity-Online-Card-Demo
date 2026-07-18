using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class RoomRoundStatusSys : MonoBehaviour
{
    [Header("核心UI引用")]
    public Text roundStatusText;
    public Text statusLimitTimeText;
    public Button roundEndBtn;
    public Text roundCountText;
    
    public static RoomRoundStatusSys Instance;

    public enum RoundState
    {
        SelfRoundStart,
        SelfWaitAction,
        SelfActionEnd,
        SelfRoundEnd,
        OtherRoundStart,
        OtherWaitAction,
        OtherActionEnd,
        OtherRoundEnd
    }

    [System.Serializable]
    private struct RoundStatusData
    {
        public string statusName;
        public int limitTime;
    }

    private List<RoundStatusData> statusConfigList = new List<RoundStatusData>();
    private RoundState currentState;
    private int currentRound = 1;
    public Coroutine countdownCoroutine;

    // 初始化由InitFightRoomSystem统一调用，保证执行顺序
    public void Init()
    {
        LoadStatusConfig();
        roundEndBtn.onClick.AddListener(OnRoundEndButtonClicked);
        roundCountText.text = currentRound.ToString();

        if (statusConfigList.Count == 8)
        {
            EnterSelfRoundStart();
        }
        else
        {
            Debug.LogError("回合状态配置文件读取失败，请检查文件路径和格式");
        }
    }

    void Start()
    {
        // 空实现，禁止自动初始化
    }
    void Awake()
    {
        Instance = this;
    }

    #region 8个状态入口
    void EnterSelfRoundStart()
    {
        BaseEnterState(RoundState.SelfRoundStart);
        CharAnimSystem.Instance.isBattleAnimChange(false);
        // 触发己方回合卡牌加载
        InitFightRoomSystem.Instance?.OnSelfRoundStart();
        // 新增：当前大轮结束，切换到下一轮敌方阵容
        InitFightRoomSystem.Instance?.switchEnemycard();
        
        // ========== 新增：战斗结束判定 ==========
        if (GameOverReturnInit.Instance != null)
        {
            GameOverReturnInit.Instance.CheckGameOverCondition(currentRound);
            // 若战斗结束，直接返回，防止逻辑循环
            if (GameOverReturnInit.Instance.isGameOver) return;
        }
        // =======================================
        
    }

    void EnterSelfWaitAction()
    {
        BaseEnterState(RoundState.SelfWaitAction);
    }

    void EnterSelfActionEnd()
    {
        BaseEnterState(RoundState.SelfActionEnd);
        if (CharAnimSystem.Instance != null)
        {
            CharAnimSystem.Instance.RefreshSelfRoundEndPosition();
            CharAnimSystem.Instance.SwitchAttackAndHurtRole();
        }
        CharAnimSystem.Instance.isBattleAnimChange(true);
        
    }

    void EnterSelfRoundEnd()
    {
        BaseEnterState(RoundState.SelfRoundEnd);
        // 触发己方回合卡牌回收与角色轮换
        InitFightRoomSystem.Instance?.OnSelfRoundEnd();
    }

    void EnterOtherRoundStart()
    {
        BaseEnterState(RoundState.OtherRoundStart);
        // 触发敌方回合卡牌加载
        InitFightRoomSystem.Instance?.OnOtherRoundStart();
    }

    void EnterOtherWaitAction()
    {
        BaseEnterState(RoundState.OtherWaitAction);
        InitFightRoomSystem.Instance?.PlayOtherCard(2);
    }

    void EnterOtherActionEnd()
    {
        BaseEnterState(RoundState.OtherActionEnd);
        if (CharAnimSystem.Instance != null)
        {
            CharAnimSystem.Instance.RefreshOtherRoundEndPosition();
            CharAnimSystem.Instance.SwitchAttackAndHurtRole();
        }
        
        
    }

    void EnterOtherRoundEnd()
    {
        BaseEnterState(RoundState.OtherRoundEnd);
        // 触发敌方回合卡牌回收与角色轮换
        InitFightRoomSystem.Instance?.OnOtherRoundEnd();

    }
    #endregion

    #region 通用基础逻辑
    void BaseEnterState(RoundState state)
    {
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        currentState = state;
        int stateIndex = (int)state;
        RoundStatusData config = statusConfigList[stateIndex];

        roundStatusText.text = config.statusName;
        roundStatusText.color = stateIndex < 4 ? Color.white : Color.red;
        statusLimitTimeText.color = stateIndex < 4 ? Color.white : Color.red;

        UpdateInteractableState();
        countdownCoroutine = StartCoroutine(CountdownCoroutine(config.limitTime));
    }

    void UpdateInteractableState()
    {
        bool isClickable = currentState == RoundState.SelfWaitAction;
        if (MainCharAndCardSys.Instance != null)
        {
            MainCharAndCardSys.Instance.isClickPlaying = !isClickable;
        }
        roundEndBtn.interactable = isClickable;
    }

    IEnumerator CountdownCoroutine(int totalTime)
    {
        int remaining = totalTime;
        while (remaining > 0)
        {
            statusLimitTimeText.text = remaining.ToString();
            yield return new WaitForSeconds(1f);
            remaining--;
        }
        SwitchToNextState();
    }

    void SwitchToNextState()
    {
        switch (currentState)
        {
            case RoundState.SelfRoundStart: EnterSelfWaitAction(); break;
            case RoundState.SelfWaitAction: EnterSelfActionEnd(); break;
            case RoundState.SelfActionEnd: EnterSelfRoundEnd(); break;
            case RoundState.SelfRoundEnd: EnterOtherRoundStart(); break;
            case RoundState.OtherRoundStart: EnterOtherWaitAction(); break;
            case RoundState.OtherWaitAction: EnterOtherActionEnd(); break;
            case RoundState.OtherActionEnd: EnterOtherRoundEnd(); break;
            case RoundState.OtherRoundEnd:
                currentRound++;
                roundCountText.text = currentRound.ToString();
                EnterSelfRoundStart();
                break;
        }
    }
    #endregion

    #region 事件与工具方法
    void OnRoundEndButtonClicked()
    {
        EnterSelfActionEnd();
    }

    void LoadStatusConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/Round-Status.ini");
        if (!File.Exists(configPath))
        {
            Debug.LogError($"配置文件不存在：{configPath}");
            return;
        }

        string[] lines = File.ReadAllLines(configPath);
        RoundStatusData currentData = new RoundStatusData();
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";")) continue;

            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                if (!string.IsNullOrEmpty(currentData.statusName))
                    statusConfigList.Add(currentData);
                currentData = new RoundStatusData();
                continue;
            }

            string[] kv = trimmed.Split('=');
            if (kv.Length != 2) continue;
            string key = kv[0].Trim();
            string value = kv[1].Trim().Trim('"');

            if (key == "Status") currentData.statusName = value;
            else if (key == "Limit_Time" && int.TryParse(value, out int time))
                currentData.limitTime = time;
        }
        if (!string.IsNullOrEmpty(currentData.statusName))
            statusConfigList.Add(currentData);
    }

    void OnDestroy()
    {
        roundEndBtn.onClick.RemoveListener(OnRoundEndButtonClicked);
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
    }
    #endregion
}