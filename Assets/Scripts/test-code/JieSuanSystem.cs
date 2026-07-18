using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JieSuanSystem : MonoBehaviour
{
    [Header("结算UI引用")]
    public Text selfResultText; // 己方结果文本
    public Text otherResultText; // 敌方结果文本

    // Start is called before the first frame update
    void Start()
    {
        InitJieSuanUI();
    }

    // 新增：结算UI初始化
    private void InitJieSuanUI()
    {
        // 从CollectResourceToInitScene读取战斗数据
        CollectResourceToInitScene dataCenter = CollectResourceToInitScene.Instance;
        if (dataCenter == null)
        {
            Debug.LogError("结算系统：未找到CollectResourceToInitScene单例");
            return;
        }

        // 1. 基础数据拼接
        string timeStr = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 具体时间
        string positionStr = dataCenter.battlePositionText; // 地点文本
        string winLoseStr = dataCenter.isBattleWin ? "胜利" : "失败";
        string remainManaStr = dataCenter.remainSelfManaPool.ToString(); // 剩余魔力

        // 2. 填充己方结果
        selfResultText.text = $"{timeStr} {positionStr} 己方角色阵营{winLoseStr} 剩余魔力:{remainManaStr}";

        // 3. 填充敌方结果（对称格式，胜负取反）
        string otherWinLoseStr = !dataCenter.isBattleWin ? "胜利" : "失败";
        otherResultText.text = $"{timeStr} {positionStr} 敌方角色阵营{otherWinLoseStr}";
    }

    // 保留原有空Update
    void Update()
    {
        
    }
}
