using UnityEngine;
using System.IO;

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance;

    // 配置文件路径 移除了无效的[Header]特性
    private string ConfigPath => Path.Combine(Application.streamingAssetsPath, "Scenes/assert-config/Connect-Config.ini");

    // 所有服务地址
    public string BaseAuth { get; private set; }
    public string WsHall { get; private set; }
    public string WsMap { get; private set; }
    public string WsBag { get; private set; }
    public string WsCard { get; private set; }
    public string WsFight { get; private set; }
    public string WsBattle { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 原生解析INI配置 无需第三方库
    /// </summary>
    void LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            Debug.LogError($"[GameConfig] 配置文件不存在：{ConfigPath}");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(ConfigPath);
            bool inNetworkSection = false;

            foreach (string line in lines)
            {
                string trimLine = line.Trim();
                // 跳过空行、注释
                if (string.IsNullOrEmpty(trimLine) || trimLine.StartsWith(";") || trimLine.StartsWith("#"))
                    continue;

                // 识别节点
                if (trimLine == "[network]")
                {
                    inNetworkSection = true;
                    continue;
                }
                if (trimLine.StartsWith("["))
                {
                    inNetworkSection = false;
                    continue;
                }

                // 解析键值对
                if (inNetworkSection && trimLine.Contains("="))
                {
                    string[] kv = trimLine.Split('=');
                    if (kv.Length != 2) continue;
                    string key = kv[0].Trim();
                    string value = kv[1].Trim();

                    switch (key)
                    {
                        case "BASE_AUTH": BaseAuth = value; break;
                        case "WS_BIZ_hall": WsHall = value; break;
                        case "WS_BIZ_map": WsMap = value; break;
                        case "WS_BIZ_bag": WsBag = value; break;
                        case "WS_BIZ_card": WsCard = value; break;
                        case "WS_BIZ_fight": WsFight = value; break;
                        case "WS_BIZ_battle": WsBattle = value; break;
                    }
                }
            }
            Debug.Log("[GameConfig] 网络配置加载完成");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameConfig] 配置解析失败：{e.Message}");
        }
    }
}