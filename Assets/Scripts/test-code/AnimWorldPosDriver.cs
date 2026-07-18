using UnityEngine;
public class GlobalFlyAnim : MonoBehaviour
{
    [Header("预设的全局终点坐标（手动填你要飞的位置）")]
    public Vector2 targetGlobalPos;
    [Header("动画飞行进度（0=起点，1=终点）")]
    [Range(0, 1)] public float flyProgress;
    private Vector3 _startGlobalPos;
    private bool _isStarted = false;

    // 新增：每次激活时重置所有状态
    private void OnEnable()
    {
        _isStarted = false;
        flyProgress = 0;
        _startGlobalPos = Vector3.zero;
    }

    // 用动画事件调用这个方法，在动画开始时锁定正确的起点
    public void LockStartPosition()
    {
        _startGlobalPos = transform.position;
        _isStarted = true;
    }

    private void LateUpdate()
    {
        if (_isStarted)
        {
            transform.position = Vector3.Lerp(_startGlobalPos, targetGlobalPos, flyProgress);
        }
    }
}