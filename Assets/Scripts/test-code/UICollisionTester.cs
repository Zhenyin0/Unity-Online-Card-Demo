using UnityEngine;

// 挂在你的建筑UI上（build-A）
public class UICollisionTester : MonoBehaviour
{
    // 拖拽赋值：场景里的区域UI
    public UIRegionCheck targetRegion;
    private RectTransform _buildRect;
    private bool _isInRegion = false;

    private void Awake()
    {
        _buildRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // 检测建筑中心点是否在区域内
        bool nowInRegion = targetRegion.IsInRegion(_buildRect.position);

        // 状态变化才打印（避免刷屏）
        if (nowInRegion != _isInRegion)
        {
            _isInRegion = nowInRegion;
            if (_isInRegion)
            {
                Debug.Log("✅ 建筑UI已进入区域UI！");
            }
            else
            {
                Debug.Log("❌ 建筑UI已离开区域UI！");
            }
        }
    }
}