using UnityEngine;
using Cysharp.Threading.Tasks;

public class MapMsgHandler : MonoBehaviour
{
    private bool isConnecting = false;

    async void Start()
    {
        await ConnectMapServer();
    }

    void OnDestroy()
    {
        if (WebSocketManager.Instance != null && WebSocketManager.Instance.IsConnected)
        {
            WebSocketManager.Instance.CloseConnect();
        }
    }

    private async UniTask ConnectMapServer()
    {
        if (isConnecting) return;
        if (GameConfig.Instance == null || WebSocketManager.Instance == null)
        {
            Debug.LogError("连接地图服务器失败：全局配置未初始化");
            return;
        }

        if (WebSocketManager.Instance.IsConnected &&
            WebSocketManager.Instance.CurrentServer == GameConfig.Instance.WsMap)
            return;

        isConnecting = true;
        try
        {
            WebSocketManager.Instance.Connect(GameConfig.Instance.WsMap);

            int waitCount = 0;
            while (!WebSocketManager.Instance.IsConnected && waitCount < 30)
            {
                await UniTask.Delay(100);
                waitCount++;
            }

            if (WebSocketManager.Instance.IsConnected)
            {
                Debug.Log("✅ 地图服务器连接完成");
                await LoginMsgHandler.SendLoginReq();   // ← 加这一行
            }
            else
            {
                Debug.LogError("地图服务器连接超时");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"连接地图服务器异常：{e.Message}");
        }
        finally
        {
            isConnecting = false;
        }
    }
}