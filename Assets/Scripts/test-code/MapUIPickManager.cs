using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUIPickManager : MonoBehaviour
{
    // 需要在Inspector面板挂载的UI对象
    public GameObject bagOpen;         // 背包打开按钮
    public GameObject EnterFightRoomPath; // 战斗房间路径面板
    public GameObject BagItemPath;     // 背包内容面板
    public GameObject PathClose;       // 背包关闭按钮

    [Header("动画参数（可在Inspector调整）")]
    [Tooltip("动画总时长，单位秒")]
    public float animationDuration = 0.3f;
    [Tooltip("弹性回弹强度，0=无回弹，1=强回弹")]
    [Range(0f, 1f)] public float bounceStrength = 0.15f;

    // 动画状态锁，防止重复点击
    private bool isAnimating = false;
    // 背包面板的原始位置（屏幕外上方）
    private Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        // 记录背包面板的原始位置（初始在屏幕外上方）
        if (BagItemPath != null)
        {
            originalPosition = BagItemPath.transform.position;
            BagItemPath.SetActive(false);
        }

        // 绑定背包打开按钮点击事件
        if (bagOpen != null && bagOpen.GetComponent<Button>() != null)
        {
            bagOpen.GetComponent<Button>().onClick.AddListener(OnBagOpenClick);
        }
        else
        {
            Debug.LogWarning("bagOpen按钮未正确挂载或缺少Button组件");
        }

        // 绑定背包关闭按钮点击事件
        if (PathClose != null && PathClose.GetComponent<Button>() != null)
        {
            PathClose.GetComponent<Button>().onClick.AddListener(OnPathCloseClick);
        }
        else
        {
            Debug.LogWarning("PathClose按钮未正确挂载或缺少Button组件");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 背包打开按钮点击处理
    private void OnBagOpenClick()
    {
        // 如果正在动画中，直接返回
        if (isAnimating) return;

        // 空值检测
        if (EnterFightRoomPath == null || BagItemPath == null)
        {
            Debug.LogWarning("必要的UI对象未挂载");
            return;
        }

        // 检测条件1：战斗房间面板必须未激活
        bool isFightRoomInactive = !EnterFightRoomPath.activeSelf;
        
        // 检测条件2：战斗房间面板轴心点必须在(1920, 1080)位置
        bool isFightRoomPosValid = EnterFightRoomPath.transform.position.x >= 0f &&
                                   EnterFightRoomPath.transform.position.x <= 1920f &&
                                   EnterFightRoomPath.transform.position.y >= 0f &&
                                   EnterFightRoomPath.transform.position.y <= 1080f;

        // 两个条件同时满足才执行打开逻辑
        if (isFightRoomInactive && isFightRoomPosValid)
        {
            StartCoroutine(OpenBagAnimation());
        }
    }

    // 背包关闭按钮点击处理
    private void OnPathCloseClick()
    {
        // 如果正在动画中，直接返回
        if (isAnimating) return;

        if (BagItemPath == null)
        {
            Debug.LogWarning("BagItemPath对象未挂载");
            return;
        }

        StartCoroutine(CloseBagAnimation());
    }

    // 打开背包的平滑动画协程
    private IEnumerator OpenBagAnimation()
    {
        isAnimating = true;
        BagItemPath.SetActive(true);

        Vector3 startPos = originalPosition;
        Vector3 targetPos = originalPosition - new Vector3(0, 1080f, 0);
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // 使用弹性缓动曲线，让动画更生动
            float easedT = EaseOutBounce(t, bounceStrength);
            
            BagItemPath.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            yield return null;
        }

        // 确保最终位置准确
        BagItemPath.transform.position = targetPos;
        isAnimating = false;
    }

    // 关闭背包的平滑动画协程
    private IEnumerator CloseBagAnimation()
    {
        isAnimating = true;

        Vector3 startPos = BagItemPath.transform.position;
        Vector3 targetPos = originalPosition;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // 使用先快后慢的缓动曲线
            float easedT = EaseInOutQuad(t);
            
            BagItemPath.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            yield return null;
        }

        // 确保最终位置准确并隐藏面板
        BagItemPath.transform.position = targetPos;
        BagItemPath.SetActive(false);
        isAnimating = false;
    }

    // 弹性缓动函数（先加速后回弹）
    private float EaseOutBounce(float t, float bounce)
    {
        if (t < 1f)
        {
            return 1f - Mathf.Pow(1f - t, 3) * (1f - bounce) + Mathf.Sin(t * Mathf.PI * 2f) * bounce;
        }
        return 1f;
    }

    // 平滑缓动函数（先慢后快再慢）
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}
