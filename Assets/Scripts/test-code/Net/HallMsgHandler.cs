using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using Cysharp.Threading.Tasks;
using HallserverPlayer;
using HallserverItem;
using Common;

public class HallMsgHandler : MonoBehaviour
{
    private const int PLAYER_INFO_NOTICE = 10180;
    private const int ITEM_LIST_NOTICE = 10280;

    private const int MANA_ITEM_ID = 10001;
    private const int STAMINA_ITEM_ID = 10002;

    [Header("玩家信息UI")]
    public Text nameText;
    public Text idText;
    public Text levelText;
    public Text scoreText;

    [Header("资源UI")]
    public Text manaCountText;
    public Text staminaCountText;

    private bool hasRegistered = false;

    void Awake()
    {
        if (!hasRegistered && MessageDispatcher.Instance != null)
        {
            MessageDispatcher.Instance.Register(PLAYER_INFO_NOTICE, HandlePlayerInfo);
            MessageDispatcher.Instance.Register(ITEM_LIST_NOTICE, HandleItemList);
            hasRegistered = true;
        }

        if (GameDataCenter.Instance != null)
        {
            GameDataCenter.Instance.OnPlayerInfoUpdated += RefreshUI;
            GameDataCenter.Instance.OnItemListUpdated += RefreshUI;
        }
    }

    async void Start()
    {
        await ConnectHallServer();
    }

    void OnDestroy()
    {
        if (MessageDispatcher.Instance != null && hasRegistered)
        {
            MessageDispatcher.Instance.Unregister(PLAYER_INFO_NOTICE, HandlePlayerInfo);
            MessageDispatcher.Instance.Unregister(ITEM_LIST_NOTICE, HandleItemList);
            hasRegistered = false;
        }

        if (GameDataCenter.Instance != null)
        {
            GameDataCenter.Instance.OnPlayerInfoUpdated -= RefreshUI;
            GameDataCenter.Instance.OnItemListUpdated -= RefreshUI;
        }

        if (WebSocketManager.Instance != null && WebSocketManager.Instance.IsConnected)
        {
            WebSocketManager.Instance.CloseConnect();
        }
    }

    private async UniTask ConnectHallServer()
    {
        if (GameConfig.Instance == null || WebSocketManager.Instance == null)
        {
            Debug.LogError("连接大厅失败：全局配置未初始化");
            return;
        }

        WebSocketManager.Instance.Connect(GameConfig.Instance.WsHall);

        int waitCount = 0;
        while (!WebSocketManager.Instance.IsConnected && waitCount < 30)
        {
            await UniTask.Delay(100);
            waitCount++;
        }

        if (WebSocketManager.Instance.IsConnected)
        {
            await LoginMsgHandler.SendLoginReq();
        }
        else
        {
            Debug.LogError("大厅服务器连接超时");
        }
    }

    private void HandlePlayerInfo(byte[] body)
    {
        try
        {
            PlayerInfoNotice info = PlayerInfoNotice.Parser.ParseFrom(body);
            if (GameDataCenter.Instance == null) return;

            GameDataCenter.Instance.PlayerName = info.Nickname;
            GameDataCenter.Instance.Level = info.Level;
            GameDataCenter.Instance.RankScore = info.RankScore;

            GameDataCenter.Instance.NotifyPlayerInfoUpdated();
            Debug.Log($"✅ 玩家信息 | 等级：{info.Level} | 积分：{info.RankScore}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"解析玩家信息失败：{e.Message}");
        }
    }

    private void HandleItemList(byte[] body)
    {
        try
        {
            ItemListNotice data = ItemListNotice.Parser.ParseFrom(body);
            if (GameDataCenter.Instance == null) return;

            int manaValue = 0;
            int staminaValue = 0;

            foreach (Common.Item item in data.ItemList)
            {
                if (item.Id == MANA_ITEM_ID) manaValue = (int)item.Count;
                if (item.Id == STAMINA_ITEM_ID) staminaValue = (int)item.Count;
            }

            GameDataCenter.Instance.Mana = manaValue;
            GameDataCenter.Instance.Stamina = staminaValue;

            GameDataCenter.Instance.NotifyItemListUpdated();
            Debug.Log($"✅ 道具信息 | 数量：{data.ItemList.Count}；魔力：{manaValue}，体力：{staminaValue}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"解析道具列表失败：{e.Message}");
        }
    }

    private void RefreshUI()
    {
        if (GameDataCenter.Instance == null) return;
        var c = GameDataCenter.Instance;

        if (nameText != null) nameText.text = c.PlayerName ?? "";
        if (idText != null) idText.text = c.PlayerId.ToString();
        if (levelText != null) levelText.text = c.Level.ToString();
        if (scoreText != null) scoreText.text = c.RankScore.ToString();
        if (manaCountText != null) manaCountText.text = c.Mana.ToString();
        if (staminaCountText != null) staminaCountText.text = c.Stamina.ToString();
    }
}