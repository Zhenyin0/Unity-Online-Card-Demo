using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public delegate void ClickAction();

public class HandCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("手牌参数")]
    public float liftHeight = 80f;
    public float animationDuration = 0.2f;

    public int charId;
    public int cardId;
    [HideInInspector] public bool isLifted = false;
    [HideInInspector] public bool isInSelectArea = true;

    public RectTransform rectTransform;
    public ClickAction onLeftClick;
    public ClickAction onRightClick;

    private Vector3 originalPosition;
    private Vector3 layoutPosition; // 布局基准位置
    private bool isDragging = false;
    private MainCharAndCardSys sys; // 缓存单例引用，解决性能问题

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        layoutPosition = originalPosition;
        sys = MainCharAndCardSys.Instance; // 一次性获取，不再每帧查找
        isInSelectArea = CardSystemManager.IsInSelectArea(transform);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 动画期间禁用所有点击
        if (sys != null && sys.isAnimPlaying) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    void OnLeftClick()
    {
        // 判断是否是CardPath子对象（自动区分出牌/复制逻辑）
        bool isCardPathChild = transform.parent.name == "CardPath" || (transform.parent.parent != null && transform.parent.parent.name == "CardPath");

        if (isCardPathChild)
        {
            // CardPath子对象：触发MainCharAndCardSys的出牌逻辑
            onLeftClick?.Invoke();
        }
        else if (isInSelectArea)
        {
            // 原选卡区逻辑：复制到用卡区
            if (isLifted)
            {
                CopyToUserArea();
                Lower();
            }
            else
            {
                Lift();
            }
        }
        else
        {
            // 原用卡区逻辑：仅抬起/落下
            if (isLifted)
            {
                Lower();
            }
            else
            {
                Lift();
            }
        }
    }

    void OnRightClick()
    {
        if (sys != null && sys.isAnimPlaying) return;

        bool isCardPathChild = transform.parent.name == "CardPath" || (transform.parent.parent != null && transform.parent.parent.name == "CardPath");

        if (isCardPathChild)
        {
            // CardPath子对象：触发MainCharAndCardSys的落下逻辑
            onRightClick?.Invoke();
        }
        else if (isInSelectArea)
        {
            // 原选卡区逻辑：落下
            if (isLifted)
            {
                Lower();
            }
        }
        else
        {
            // 原用卡区逻辑：删除
            DeleteCard();
        }
    }

    // 原有拖动逻辑完全保留
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (sys != null && sys.isAnimPlaying) return;
        if (!isInSelectArea)
        {
            isDragging = true;
            originalPosition = rectTransform.anchoredPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (sys != null && sys.isAnimPlaying) return;
        if (!isInSelectArea && isDragging)
        {
            isDragging = false;
            if (CardSystemManager.IsInSelectArea(rectTransform))
            {
                DeleteCard();
            }
            else
            {
                StartCoroutine(SmoothMove(rectTransform, originalPosition, animationDuration));
            }
        }
    }

    // 升级后的抬起方法（自动计算角度补偿）
    public void Lift()
    {
        isLifted = true;
        // 计算旋转角度补偿
        float rotationZ = rectTransform.rotation.eulerAngles.z;
        if (rotationZ > 180) rotationZ -= 360;
        float radian = rotationZ * Mathf.Deg2Rad;
        float yIncrement = liftHeight / Mathf.Cos(radian);
        

        Vector3 targetPosition = rectTransform.anchoredPosition + new Vector2(0, yIncrement);
        StartCoroutine(SmoothMove(rectTransform, targetPosition, animationDuration));

        // 原有效果面板逻辑完全保留
        Transform handEffectTransform = transform.Find(CardSystemManager.HAND_CARD_EFFECT);
        if (handEffectTransform != null)
        {
            Text handEffectText = handEffectTransform.GetComponent<Text>();
            if (handEffectText != null && CardSystemManager.globalEffectText != null)
            {
                CardSystemManager.globalEffectText.text = handEffectText.text;
            }
        }
    }

    public void Lower()
    {
        isLifted = false;
        float rotationZ = rectTransform.rotation.eulerAngles.z;
        if (rotationZ > 180) rotationZ -= 360;
        float radian = rotationZ * Mathf.Deg2Rad;
        float yIncrement = liftHeight / Mathf.Cos(radian);

        Vector3 targetPosition = rectTransform.anchoredPosition - new Vector2(0, yIncrement);
        StartCoroutine(SmoothMove(rectTransform, targetPosition, animationDuration));

        if (CardSystemManager.globalEffectText != null)
        {
            CardSystemManager.globalEffectText.text = "";
        }
    }

    // 供MainCharAndCardSys设置布局基准位置
    public void SetLayoutPosition(Vector3 pos)
    {
        layoutPosition = pos;
    }

    // 原有复制到用卡区逻辑完全保留
    void CopyToUserArea()
    {
        if (CardSystemManager.Instance.GetUserAreaHandCardCount() >= CardSystemManager.Instance.cardCountConfig.CardlimitCount)
        {
            Debug.LogWarning("用卡区手牌已达上限");
            return;
        }

        if (CardSystemManager.Instance.GetUserAreaHandCardCountByCharId(charId) >= CardSystemManager.Instance.GetCharHandCardLimit(charId))
        {
            Debug.LogWarning($"角色卡{charId}手牌已达上限");
            return;
        }
        
        // 3. 单卡上限检查
        if (CardSystemManager.Instance.GetUserAreaHandCardCountByCardId(cardId) >= CardSystemManager.Instance.GetCardLimit(cardId))
        {
            Debug.LogWarning($"手牌 {cardId} 已达上限{CardSystemManager.Instance.GetCardLimit(cardId)}张");
            return;
        }

        if (!CardSystemManager.Instance.IsCharCardExistInUserArea(charId))
        {
            Debug.LogWarning($"用卡区未找到角色卡{charId}");
            return;
        }

        Transform userContent = CardSystemManager.GetUserContent();
        if (userContent == null) return;

        GameObject newCard = Instantiate(gameObject, userContent);
        // 卡牌数量计数 +1
        Text cardCountText = CardSystemManager.GetCardCountText();
        if (cardCountText != null && int.TryParse(cardCountText.text, out int count))
        {
            cardCountText.text = (count + 1).ToString();
        }
        HandCard newHandCard = newCard.GetComponent<HandCard>();
        newHandCard.isInSelectArea = false;
        newHandCard.isLifted = false;

        foreach (Transform child in userContent)
        {
            CharCard charCard = child.GetComponent<CharCard>();
            if (charCard != null && charCard.charId == this.charId)
            {
                newHandCard.rectTransform.anchoredPosition = child.GetComponent<RectTransform>().anchoredPosition;
                newHandCard.transform.SetSiblingIndex(charCard.transform.GetSiblingIndex());
                return;
            }
        }

        newHandCard.rectTransform.anchoredPosition = Vector3.zero;
    }

    void DeleteCard()
    {
        // 卡牌数量计数 -1
        Text cardCountText = CardSystemManager.GetCardCountText();
        if (cardCountText != null && int.TryParse(cardCountText.text, out int count))
        {
            cardCountText.text = (count - 1).ToString();
        }
        
        Destroy(gameObject);
    }

    IEnumerator SmoothMove(RectTransform target, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        target.anchoredPosition = targetPosition;
    }

    void Update()
    {
        if (sys != null && sys.isAnimPlaying) return;

        if (isDragging)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent.GetComponent<RectTransform>(),
                Input.mousePosition,
                null,
                out Vector2 localPoint
            );
            rectTransform.anchoredPosition = localPoint;
        }
    }
}
