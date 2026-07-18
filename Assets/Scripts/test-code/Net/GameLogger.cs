using UnityEngine;
using System;
using System.IO;

public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance;
    private string logRootPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            logRootPath = Path.Combine(Application.persistentDataPath, "logs");
            if (!Directory.Exists(logRootPath))
                Directory.CreateDirectory(logRootPath);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Log(string msg)
    {
        string timeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logContent = $"[{timeStr}] {msg}\n";
        
        Debug.Log(logContent.Trim());
        
        string fileName = GameDataCenter.Instance != null && !string.IsNullOrEmpty(GameDataCenter.Instance.PlayerName)
            ? $"game_client-{GameDataCenter.Instance.PlayerName}.log"
            : "client_common.log";
        
        string fullPath = Path.Combine(logRootPath, fileName);
        File.AppendAllText(fullPath, logContent);
    }

    public void LogError(string msg)
    {
        Log($"❌ 错误：{msg}");
    }
}