using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverReturnInit : MonoBehaviour
{
    public static GameOverReturnInit Instance;
    // 新增：UI文本引用（需在Inspector赋值）
    public Text selfAllCardText; // ALLCard节点下的Text
    public Text selfRoundCardText; // ALLCard→RoundImage下的Text
    public Text otherAllCardText; // OtherAllCard节点下的Text
    public Button toJieSuanBtn; // 前往结算按钮
    public GameObject GameOverPath;
    
    // 新增：结束判定结果
    public bool isGameOver; // 战斗是否结束（触发GameOverPath开关）
    public bool isBattleWin; // 战斗是否胜利

    void Start()
    {
        // 按钮监听：点击后执行数据传递+场景跳转
        toJieSuanBtn.onClick.AddListener(OnToJieSuanClick);
    }
    
    void Awake()
    {
        Instance = this;
    }

    // 新增：战斗结束判定核心方法（供RoomRoundStatusSys调用）
    public void CheckGameOverCondition(int currentRound)
    {
        // 解析UI文本数值
        int selfAllCard = int.TryParse(selfAllCardText.text, out int s1) ? s1 : 0;
        int selfRoundCard = int.TryParse(selfRoundCardText.text, out int s2) ? s2 : 0;
        int otherAllCard = int.TryParse(otherAllCardText.text, out int o1) ? o1 : 0;

        // 判定规则
        bool isLoseByRound = currentRound >= 7; // 条件1：第七回合失败
        bool isWinByEnemyCard = otherAllCard <= 0; // 条件2：敌方卡牌归零胜利
        bool isWinBySelfCard = selfAllCard <= 0; // 条件3：我方卡牌归零胜利

        // 更新结束状态
        isGameOver = isLoseByRound || isWinByEnemyCard || isWinBySelfCard;
        isBattleWin = !isLoseByRound && (isWinByEnemyCard || isWinBySelfCard);

        if (isGameOver)
        {
            // 暂停倒数读秒（通过RoomRoundStatusSys的单例）
            if (RoomRoundStatusSys.Instance != null && RoomRoundStatusSys.Instance.countdownCoroutine != null)
            {
                RoomRoundStatusSys.Instance.StopCoroutine(RoomRoundStatusSys.Instance.countdownCoroutine);
                RoomRoundStatusSys.Instance.countdownCoroutine = null;
            }
            
            // 激活GameOverPath对象（需自行绑定GameOverPath的激活逻辑）
            GameOverPath.SetActive(true);
            
            // 挂起流程：仅等待玩家点击按钮，不自动跳转
            Debug.Log($"战斗结束：{(isBattleWin ? "胜利" : "失败")}，原因：{(isLoseByRound ? "回合数达7" : (isWinByEnemyCard ? "敌方卡牌归零" : "我方卡牌归零"))}");
        }
    }

    // 新增：前往结算按钮点击事件
    private void OnToJieSuanClick()
    {
        // 1. 向CollectResourceToInitScene同步数据
        CollectResourceToInitScene dataCenter = CollectResourceToInitScene.Instance;
        if (dataCenter != null)
        {
            // 从InitFightRoomSystem读取核心数据
            dataCenter.isBattleWin = this.isBattleWin;
            dataCenter.remainSelfManaPool = InitFightRoomSystem.Instance._totalSelfManaPool;
            dataCenter.battlePositionText = InitFightRoomSystem.Instance.localPositionText.text;
        }

        // 2. 场景跳转（需替换为实际的场景名）
        // UnityEngine.SceneManagement.SceneManager.LoadScene("JieSuanScene");
    }

    // 保留原有空Update
    void Update()
    {
        
    }
}
