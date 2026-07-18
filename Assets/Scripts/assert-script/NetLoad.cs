using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// NL：纯网络层 → 只负责联网，不碰UI
/// 完全对齐Python成功的接口逻辑
/// </summary>
public class NetLoad : MonoBehaviour
{
    public static NetLoad Instance;

    [Header("服务端配置")]
    public string baseUrl;
    private readonly string HEADER_CONTENT_TYPE = "application/json";

    // 响应结构体（Newtonsoft 无需 [Serializable]）
    public class ServerResponse
    {
        public int code;
        public string message;
        public ResponseData data;
    }

    public class ResponseData
    {
        public string host;
        public string token;
        public long player_id;
    }

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
    
    void Start()
    {
        // 从全局配置读取认证地址
        if (GameConfig.Instance != null)
        {
            baseUrl = GameConfig.Instance.BaseAuth;
        }
    }

    #region 注册请求
    public void DoRegister(string account, string pwd, Action<bool, string> callback)
    {
        if (Instance == null)
        {
            callback?.Invoke(false, "网络层未初始化");
            return;
        }
        StartCoroutine(RegisterRequest(account, pwd, callback));
    }

    IEnumerator RegisterRequest(string account, string pwd, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/user/signup";
        var postData = new { account = account, password = pwd, channel = 1 };
        string jsonData = JsonConvert.SerializeObject(postData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // using 自动释放请求，避免资源泄漏
        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", HEADER_CONTENT_TYPE);
            req.timeout = 10;

            // 【关键修复】yield 放在 try-catch 外，彻底解决语法错误
            yield return req.SendWebRequest();

            // 仅将「处理逻辑」包裹在 try-catch 中
            try
            {
                if (req.result != UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(false, $"网络错误：{req.error}");
                    yield break;
                }

                string responseText = req.downloadHandler.text;
                if (string.IsNullOrEmpty(responseText))
                {
                    callback?.Invoke(false, "服务端无响应");
                    yield break;
                }

                JObject jo = JObject.Parse(responseText);
                int code = (int)jo["code"];
                string msg = jo["message"]?.ToString() ?? string.Empty;

                if (code == 20000)
                    callback?.Invoke(true, "注册成功");
                else
                    callback?.Invoke(false, $"注册失败：{msg}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"【注册异常】{ex.Message}");
                callback?.Invoke(false, $"程序异常：{ex.Message}");
            }
        }
    }
    #endregion

    #region 登录请求（彻底解决语法错误+卡死问题）
    public void DoLogin(string account, string pwd, Action<bool, string> callback)
    {
        if (Instance == null)
        {
            callback?.Invoke(false, "网络层未初始化");
            return;
        }
        StartCoroutine(LoginRequest(account, pwd, callback));
    }

    IEnumerator LoginRequest(string account, string pwd, Action<bool, string> callback)
    {
        string url = $"{baseUrl}/user/login";
        var postData = new { account = account, password = pwd };
        string jsonData = JsonConvert.SerializeObject(postData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", HEADER_CONTENT_TYPE);
            req.timeout = 10;

            // 【关键修复】yield 放在 try-catch 外，彻底解决语法错误
            yield return req.SendWebRequest();

            // 仅将「处理逻辑」包裹在 try-catch 中
            try
            {
                if (req.result != UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(false, $"网络错误：{req.error}");
                    yield break;
                }

                string responseText = req.downloadHandler.text;
                if (string.IsNullOrEmpty(responseText))
                {
                    callback?.Invoke(false, "服务端返回空数据");
                    yield break;
                }

                ServerResponse res = JsonConvert.DeserializeObject<ServerResponse>(responseText);
                if (res == null)
                {
                    callback?.Invoke(false, "服务端数据格式错误");
                    yield break;
                }

                if (res.code == 20000)
                {
                    // 写入全局数据中心
                    GameDataCenter.Instance.LoginToken = res.data.token;
                    GameDataCenter.Instance.PlayerId = res.data.player_id;
                    // 若登录接口返回玩家名称 也一并写入
                    // GameDataCenter.Instance.PlayerName = res.data.player_name;
                    
                    
                    callback?.Invoke(true, "登录成功");
                }
                else
                {
                    callback?.Invoke(false, $"登录失败：{res.message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"【登录异常】{ex.Message}");
                callback?.Invoke(false, $"程序异常：{ex.Message}");
            }
        }
    }
    #endregion
}
