using UnityEngine;
using NativeWebSocket;
using System;
using Cysharp.Threading.Tasks;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance;
    private WebSocket ws;
    private byte[] receiveBuffer = new byte[1024 * 64];
    private int bufferOffset = 0;

    public bool IsConnected => ws != null && ws.State == WebSocketState.Open;
    public string CurrentServer { get; private set; } = "";

    public event Action OnConnected;
    public event Action<string> OnDisconnected;
    public event Action<string> OnError;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        ws?.DispatchMessageQueue();
    }

    // ========== 新增：游戏退出时主动断开 ==========
    void OnApplicationQuit()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            CloseConnect();
        }
    }
    
    public void Connect(string serverUrl)
    {
        if (IsConnected && CurrentServer == serverUrl) return;
        CloseConnect();

        CurrentServer = serverUrl;
        Debug.Log($"正在连接服务器：{serverUrl}");

        ws = new WebSocket(serverUrl);
        ws.OnOpen += OnWsOpen;
        ws.OnError += OnWsError;
        ws.OnClose += OnWsClose;
        ws.OnMessage += OnWsMessage;

        _ = ws.Connect();
    }

    public void CloseConnect()
    {
        if (ws != null)
        {
            ws.OnOpen -= OnWsOpen;
            ws.OnError -= OnWsError;
            ws.OnClose -= OnWsClose;
            ws.OnMessage -= OnWsMessage;
            _ = ws.Close();
            ws = null;
        }
        bufferOffset = 0;
        CurrentServer = "";
    }

    public async UniTask SendPacket(byte[] packet)
    {
        if (!IsConnected) { Debug.LogError("发送失败：未连接"); return; }
        try { await ws.Send(packet); }
        catch (Exception e) { Debug.LogError($"发送失败：{e.Message}"); }
    }

    private void OnWsOpen()
    {
        Debug.Log("✅ 已连接游戏服务器(11012)");
        OnConnected?.Invoke();
        StartHeartBeat().Forget();
    }

    private void OnWsError(string errorMsg)
    {
        Debug.LogError($"连接错误：{errorMsg}");
        OnError?.Invoke(errorMsg);
    }

    private void OnWsClose(WebSocketCloseCode closeCode)
    {
        Debug.Log($"连接关闭：{closeCode}");
        OnDisconnected?.Invoke(closeCode.ToString());
    }

    private void OnWsMessage(byte[] data)
    {
        if (bufferOffset + data.Length > receiveBuffer.Length)
            Array.Resize(ref receiveBuffer, receiveBuffer.Length * 2);

        Buffer.BlockCopy(data, 0, receiveBuffer, bufferOffset, data.Length);
        bufferOffset += data.Length;

        while (true)
        {
            if (!NetProtocol.TryParsePacket(receiveBuffer, bufferOffset, out ushort packId, out byte[] body, out int totalLen))
                break;

            MessageDispatcher.Instance.Dispatch(packId, body);

            int remain = bufferOffset - totalLen;
            if (remain > 0)
                Buffer.BlockCopy(receiveBuffer, totalLen, receiveBuffer, 0, remain);
            bufferOffset = remain;
        }
    }

    private async UniTaskVoid StartHeartBeat()
    {
        while (IsConnected)
        {
            await UniTask.Delay(5000);
            if (!IsConnected) break;
            await LoginMsgHandler.SendHeartBeat();
        }
    }
}