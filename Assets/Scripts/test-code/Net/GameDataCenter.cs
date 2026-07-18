using UnityEngine;
using System;
using System.Collections.Generic;

public class GameDataCenter : MonoBehaviour
{
    public static GameDataCenter Instance;

    // 登录基础信息
    public string LoginToken { get; set; }
    public long PlayerId { get; set; }
    public string PlayerName { get; set; }

    // 大厅玩家数据
    public int Level { get; set; }
    public int RankScore { get; set; }
    public int Mana { get; set; }
    public int Stamina { get; set; }

    // 道具列表
    public List<ItemInfo> ItemList { get; set; } = new List<ItemInfo>();

    // 数据更新事件，UI层订阅刷新
    public event Action OnPlayerInfoUpdated;
    public event Action OnItemListUpdated;

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
        }
    }

    public void NotifyPlayerInfoUpdated()
    {
        OnPlayerInfoUpdated?.Invoke();
    }

    public void NotifyItemListUpdated()
    {
        OnItemListUpdated?.Invoke();
    }
}

// 道具数据结构
[Serializable]
public class ItemInfo
{
    public int id;
    public int count;
}
