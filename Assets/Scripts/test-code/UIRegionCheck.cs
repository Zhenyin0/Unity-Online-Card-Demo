using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂在UI区域节点上，支持Scene视图拖拽编辑多边形顶点，纯UI区域检测
/// 【修改版】兼容PolygonCollider2D原生编辑，解决编辑模式拖不动顶点问题
/// </summary>
// 自动添加PolygonCollider2D组件，不用手动加
[RequireComponent(typeof(PolygonCollider2D))]
public class UIRegionCheck : MonoBehaviour
{
    [Header("区域类型配置")]
    public bool isIrregularRegion = false; // true=不规则多边形，false=矩形
    public Vector2[] localPolygonPoints;   // 多边形顶点（Scene拖拽自动赋值，不用手动填）

    [Header("区域计数")]
    public RegionZone regionZone;          // 绑定区域计数脚本

    private RectTransform _regionRect;
    private Canvas _parentCanvas;
    // 新增：缓存原生碰撞器（用于编辑顶点）
    private PolygonCollider2D _polyCollider;

    private void Awake()
    {
        _regionRect = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();
        _polyCollider = GetComponent<PolygonCollider2D>();

        // 矩形自动生成顶点
        if (!isIrregularRegion && (localPolygonPoints == null || localPolygonPoints.Length == 0))
        {
            Rect rect = _regionRect.rect;
            localPolygonPoints = new Vector2[]
            {
                new Vector2(rect.xMin, rect.yMin),
                new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMax, rect.yMax),
                new Vector2(rect.xMin, rect.yMax)
            };
            // 同步到原生碰撞器
            if(_polyCollider != null) _polyCollider.points = localPolygonPoints;
        }
    }

    /// <summary>
    /// 判断世界坐标是否在区域内
    /// </summary>
    public bool IsInRegion(Vector2 worldPos)
    {
        if (_regionRect == null) return false;
        Vector2 localPos = _regionRect.InverseTransformPoint(worldPos);
        return IsPointInPolygon(localPos, localPolygonPoints);
    }

    /// <summary>
    /// 射线法多边形判断
    /// </summary>
    private bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        if (polygon == null || polygon.Length < 3) return false;
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                inside = !inside;
        }
        return inside;
    }

    // ====================== 编辑器拖拽顶点（原生+自定义双兼容）======================
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        _regionRect = GetComponent<RectTransform>();
        _polyCollider = GetComponent<PolygonCollider2D>();

        if (!isIrregularRegion || _regionRect == null || _polyCollider == null) return;

        // 核心：同步原生碰撞器的顶点 → 你的localPolygonPoints（拖完自动赋值）
        if (_polyCollider.points.Length >= 3)
        {
            localPolygonPoints = _polyCollider.points;
        }

        UnityEditor.Handles.color = Color.green;

        // 把本地顶点转世界顶点
        Vector3[] worldPoints = new Vector3[localPolygonPoints.Length];
        for (int i = 0; i < localPolygonPoints.Length; i++)
            worldPoints[i] = _regionRect.TransformPoint(localPolygonPoints[i]);

        // 绘制多边形线框
        UnityEditor.Handles.DrawPolyLine(worldPoints);
        UnityEditor.Handles.DrawLine(worldPoints[^1], worldPoints[0]);

        // 保留你原来的绿色可拖拽点
        for (int i = 0; i < localPolygonPoints.Length; i++)
        {
            Vector3 worldPos = _regionRect.TransformPoint(localPolygonPoints[i]);
            var fmh_104_17_639145463397730074 = Quaternion.identity; Vector3 newWorldPos = UnityEditor.Handles.FreeMoveHandle(
                worldPos,
                20f,
                Vector3.zero,
                UnityEditor.Handles.SphereHandleCap
            );
            if (worldPos != newWorldPos)
            {
                localPolygonPoints[i] = _regionRect.InverseTransformPoint(newWorldPos);
                // 同步回原生碰撞器
                _polyCollider.points = localPolygonPoints;
            }
        }
    }
#endif
}
