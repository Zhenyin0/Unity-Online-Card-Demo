using UnityEngine;

/// <summary>
/// 区域计数管理：控制每个区域最多放置1个建筑
/// </summary>
public class RegionZone : MonoBehaviour
{
    [Header("区域配置")]
    [Tooltip("该区域最多可放置的建筑数量（固定为1）")]
    public int maxBuildingCount = 1;

    // 对外只读属性
    public int CurrentCount { get; private set; }
    public int AvailableSlots => maxBuildingCount - CurrentCount;
    public bool HasAvailableSlot => AvailableSlots > 0;

    /// <summary>
    /// 增加建筑计数
    /// </summary>
    public void AddBuilding()
    {
        if (HasAvailableSlot)
        {
            CurrentCount++;
            Debug.Log($"🏗️ 区域 {gameObject.name} 增加建筑，当前数量：{CurrentCount}/{maxBuildingCount}");
        }
        else
        {
            Debug.LogWarning($"🏗️ 区域 {gameObject.name} 已达最大容量！", this);
        }
    }

    /// <summary>
    /// 减少建筑计数
    /// </summary>
    public void RemoveBuilding()
    {
        CurrentCount = Mathf.Max(0, CurrentCount - 1);
        Debug.Log($"🏗️ 区域 {gameObject.name} 减少建筑，当前数量：{CurrentCount}/{maxBuildingCount}");
    }
}