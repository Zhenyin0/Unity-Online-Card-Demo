using UnityEngine;
using Cysharp.Threading.Tasks;

public class FightMsgHandler : MonoBehaviour
{
    private bool isConnecting = false;

    async void Start()
    {
        await ConnectFightServer();
    }

    void OnDestroy()
    {
        if (WebSocketManager.Instance != null && WebSocketManager.Instance.IsConnected)
        {
            WebSocketManager.Instance.CloseConnect();
        }
    }

    private async UniTask ConnectFightServer()
    {
        if (isConnecting) return;
        if (GameConfig.Instance == null || WebSocketManager.Instance == null)
        {
            Debug.LogError("连接战斗服务器失败：全局配置未初始化");
            return;
        }

        if (WebSocketManager.Instance.IsConnected &&
            WebSocketManager.Instance.CurrentServer == GameConfig.Instance.WsFight)
            return;

        isConnecting = true;
        try
        {
            WebSocketManager.Instance.Connect(GameConfig.Instance.WsFight);

            int waitCount = 0;
            while (!WebSocketManager.Instance.IsConnected && waitCount < 30)
            {
                await UniTask.Delay(100);
                waitCount++;
            }

            if (WebSocketManager.Instance.IsConnected)
            {
                Debug.Log("✅ 战斗服务器连接完成");
                await LoginMsgHandler.SendLoginReq();   // ← 加这一行
            }
            else
            {
                Debug.LogError("战斗服务器连接超时");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"连接战斗服务器异常：{e.Message}");
        }
        finally
        {
            isConnecting = false;
        }
    }
}
