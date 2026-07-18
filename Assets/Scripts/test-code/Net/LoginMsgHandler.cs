using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Login;

public static class LoginMsgHandler
{
    private const int LOGIN_REQ_ID = (int)main.Login * 100 + (int)sub.CmdLoginReq;
    private const int HEART_REQ_ID = (int)main.Login * 100 + (int)sub.CmdHeartReq;

    public static async UniTask SendLoginReq()
    {
        if (GameDataCenter.Instance == null || WebSocketManager.Instance == null)
        {
            GameLogger.Instance?.LogError("发送登录包失败：全局模块未初始化");
            return;
        }
        if (!WebSocketManager.Instance.IsConnected)
        {
            GameLogger.Instance?.LogError("发送登录包失败：连接未建立");
            return;
        }

        LoginReq req = new LoginReq
        {
            Token = GameDataCenter.Instance.LoginToken,
            PlayerId = GameDataCenter.Instance.PlayerId
        };
        byte[] protoBytes = req.ToByteArray();
        byte[] packet = NetProtocol.MakePacket((ushort)LOGIN_REQ_ID, protoBytes);
        await WebSocketManager.Instance.SendPacket(packet);
        GameLogger.Instance.Log("✅ 登录包已发送");
    }

    public static async UniTask SendHeartBeat()
    {
        if (WebSocketManager.Instance == null || !WebSocketManager.Instance.IsConnected)
            return;

        HeartReq req = new HeartReq
        {
            Time = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        byte[] protoBytes = req.ToByteArray();
        byte[] packet = NetProtocol.MakePacket((ushort)HEART_REQ_ID, protoBytes);
        await WebSocketManager.Instance.SendPacket(packet);
    }
}