using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class CharCard : MonoBehaviour, IPointerClickHandler
{
    [Header("角色卡ID（1~7）")]
    public int charId;
    
    [Header("布局参数（已按你的要求设置）")]
    public float cardWidth = 293f; // 卡牌宽度
    public float cardHeight = 412f; // 卡牌高度
    public float handCardSpacing = 100f; // 手牌间距
    public float firstHandCardOffset = 280f; // 第一张手牌与角色卡的距离
    public float animationDuration = 0.2f; // 动画时长
    
    [HideInInspector]
    public bool isExpanded = false; // 是否散开
    
    //public List<HandCard> myHandCards = new List<HandCard>(); // 我的手牌列表
    private RectTransform rectTransform;
    public Vector3 originalPosition; // 原始位置
    public bool isInSelectArea = true; // 是否在选卡区
    private bool isCopyOpen = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        
        // 初始化手牌列表（按你的归属关系）
        //InitializeHandCards();
    }
    // 点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    void Start()
    {
        // 初始状态：收拢
        UpdateHandCardPositions(false);
    }
    

    // 左键点击事件
    public void OnLeftClick()
    {
        // 切换散开/合拢状态
        ToggleExpand();
        if (isInSelectArea)
        {
            if (isCopyOpen)
            {
                CopyToUserArea();
                isCopyOpen = false;
            }
            else
            {
                isCopyOpen = true;
            }

            // 已经抬起：复制到用卡区，然后落下
            
        }else
        {
            
            // 用卡区：无特殊效果（和手牌保持一致）
            Debug.Log("用卡区角色卡左键点击");
        }

    }

    // 右键点击事件
    public void OnRightClick()
    {
        if (isInSelectArea)
        {
            isCopyOpen = false;
            
            // 保留你原来的逻辑：收拢+取消所有手牌抬起
            if (isExpanded)
            {
                Collapse();
                CancelAllLifted();
            }
        }
        else
        {
            // 用卡区：删除自己+删除所有关联手牌
            DeleteCharCard();
        }
    }

    // 切换散开/合拢
    public void ToggleExpand()
    {
        if (isExpanded)
        {
            Collapse();
            CancelAllLifted();
        }
        else
        {
            Expand();
        }
    }

    // 散开
    public void Expand()
    {
        isExpanded = true;
        UpdateHandCardPositions(true);
        UpdateAllCharCardPositions(); // 多米诺骨牌效果：更新所有角色卡位置
    }

    // 合拢（修复版：调整时序）
    public void Collapse()
    {
        isExpanded = false;
        // 第一步：先执行多米诺骨牌，更新所有角色卡的目标位置
        UpdateAllCharCardPositions();
        // 第二步：同时让所有抬起的手牌落下（动画）
        CancelAllLifted();
        // 第三步：延迟动画时长后，再更新手牌位置，确保完全同步
        StartCoroutine(DelayUpdateHandCardPositions(false));
    }

   // 延迟执行手牌位置更新（解决时序问题）
    IEnumerator DelayUpdateHandCardPositions(bool isExpanded)
    {
        yield return null; // 延迟1帧
        UpdateHandCardPositions(isExpanded);
    }
    
// 更新手牌位置（新核心逻辑：实时遍历+字典分组，支持传入基准位置）
void UpdateHandCardPositions(bool isExpanded, Vector3? basePosition = null)
{
    // 1. 确定计算基准：传入了就用传入的目标位置，没传就用当前位置
    Vector3 calculateBase = basePosition ?? rectTransform.anchoredPosition;

    // 2. 实时遍历容器，收集所有属于当前角色的手牌
    List<HandCard> allMyHand = new List<HandCard>();
    Transform content = transform.parent;

    foreach (Transform child in content)
    {
        HandCard hc = child.GetComponent<HandCard>();
        if (hc != null && hc.charId == this.charId)
        {
            allMyHand.Add(hc);
        }
    }

    // 3. 按cardId分组（天然去重槽位）
    Dictionary<int, List<HandCard>> idGroup = new Dictionary<int, List<HandCard>>();
    foreach (var card in allMyHand)
    {
        if (!idGroup.ContainsKey(card.cardId))
        {
            idGroup.Add(card.cardId, new List<HandCard>());
        }
        idGroup[card.cardId].Add(card);
    }

    // 4. 展开状态：按分组分配槽位
    if (isExpanded)
    {
        int slotIndex = 0;
        // 先对cardId排序，保证展开顺序正确
        var sortedGroups = idGroup.OrderBy(g => g.Key);
        foreach (var groupPair in sortedGroups)
        {
            // 计算当前cardId的基准坐标（使用传入的基准位置）
            float offsetX = firstHandCardOffset + slotIndex * handCardSpacing;
            Vector3 targetPos = new Vector3(
                calculateBase.x + offsetX, 
                calculateBase.y, 
                0
            );
            // 同cardId的所有手牌全部堆叠到这个坐标
            foreach (var singleCard in groupPair.Value)
            {
                RectTransform handRect = singleCard.GetComponent<RectTransform>();
                StartCoroutine(SmoothMove(handRect, targetPos, animationDuration));
                singleCard.gameObject.SetActive(true);
            }
            slotIndex++;
        }
    }
    // 5. 收拢状态：所有手牌回到角色卡基准位置（修复：抵消抬起高度）
    else
    {
        Vector3 targetPos = calculateBase;
        foreach (var card in allMyHand)
        {
            RectTransform handRect = card.GetComponent<RectTransform>();
        
            // 不管是不是抬起状态，都从当前位置动画移动到目标位置
            // 不要强制修改位置，让动画自己处理
            StartCoroutine(SmoothMove(handRect, targetPos, animationDuration));
        
            // 动画结束后再重置isLifted状态
            StartCoroutine(ResetLiftedStateAfterAnimation(card, animationDuration));
        
            card.gameObject.SetActive(true);
        }
    }
}

// 新增：动画结束后重置抬起状态
IEnumerator ResetLiftedStateAfterAnimation(HandCard card, float duration)
{
    yield return new WaitForSeconds(duration);
    card.isLifted = false;
}

    // 多米诺骨牌效果：更新所有角色卡的位置（通用版，兼容选卡区/用卡区）
    void UpdateAllCharCardPositions()
    {
        Transform content = transform.parent;
        float currentX = 0f; // 第一个角色卡的X位置

        // 1. 收集当前容器下所有的角色卡（不管名字，只要有CharCard组件）
        List<CharCard> allCharCards = new List<CharCard>();
        foreach (Transform child in content)
        {
            CharCard charCard = child.GetComponent<CharCard>();
            if (charCard != null)
            {
                allCharCards.Add(charCard);
            }
        }

        // 2. 按charId从小到大排序，保证顺序正确
        allCharCards.Sort((a, b) => a.charId.CompareTo(b.charId));

        // 3. 遍历所有角色卡，更新位置和手牌
        foreach (CharCard charCard in allCharCards)
        {
            RectTransform charRect = charCard.GetComponent<RectTransform>();
        
            // 设置角色卡目标位置
            Vector3 targetPosition = new Vector3(currentX, charRect.anchoredPosition.y, 0);
            StartCoroutine(SmoothMove(charRect, targetPosition, animationDuration));
        
            // 核心修正：传入角色卡的目标位置作为手牌计算基准，实现完全同步
            charCard.UpdateHandCardPositions(charCard.isExpanded, targetPosition);
        
            // 计算下一个角色卡的位置
            if (charCard.isExpanded)
            {
                // 散开状态：加上总宽度
                currentX += charCard.GetTotalWidth();
            }
            else
            {
                // 收拢状态：只加卡牌宽度
                currentX += cardWidth;
            }
        
            // 角色卡之间的间距（可自定义）
            currentX += 0f;
        }
    }

    // 获取当前角色卡散开后的总宽度
    // 获取当前角色卡散开后的总宽度（动态计算，不再硬编码）
    public float GetTotalWidth()
    {
        if (!isExpanded)
        {
            return cardWidth;
        }

        // 实时统计唯一cardId的数量
        Transform content = transform.parent;
        HashSet<int> uniqueCardIds = new HashSet<int>();

        foreach (Transform child in content)
        {
            HandCard hc = child.GetComponent<HandCard>();
            if (hc != null && hc.charId == this.charId)
            {
                uniqueCardIds.Add(hc.cardId);
            }
        }

        // 总宽度 = 角色卡宽度 + 第一张手牌偏移 + (唯一卡牌数-1)*手牌间距
        if (uniqueCardIds.Count == 0)
        {
            return cardWidth;
        }

        // 修正：移除重复的cardWidth，仅保留角色卡基础宽度 + 手牌偏移
        return cardWidth + firstHandCardOffset + (uniqueCardIds.Count - 1) * handCardSpacing;
    }
    
    // 取消所有抬起的手牌（修复版：区分选卡区/用卡区）
    public void CancelAllLifted()
    {
        Transform targetContent = null;
        if (isInSelectArea)
        {
            targetContent = CardSystemManager.GetSelectContent();
        }
        else
        {
            targetContent = CardSystemManager.GetUserContent();
        }
        if (targetContent == null)
        {
            targetContent = transform.parent;
        }
    
        foreach (Transform child in targetContent)
        {
            HandCard hc = child.GetComponent<HandCard>();
            if (hc != null 
                && hc.charId == this.charId 
                && hc.isLifted 
                && hc.isInSelectArea == this.isInSelectArea)
            {
                // 只调用Lower()，让它自己执行落下动画
                // 不要提前修改位置，不要提前重置状态
                hc.Lower();
            }
        }
    }
    
    // 获取抬起的手牌ID列表
    public List<int> GetLiftedHandCardIds()
    {
        List<int> liftedIds = new List<int>();
        Transform content = transform.parent;
    
        foreach (Transform child in content)
        {
            HandCard hc = child.GetComponent<HandCard>();
            if (hc != null && hc.charId == this.charId && hc.isLifted)
            {
                liftedIds.Add(hc.cardId);
            }
        }
    
        return liftedIds;
    }

    // 加载状态
    // 加载状态
    public void LoadState(bool isExpanded, List<int> liftedHandCardIds)
    {
        this.isExpanded = isExpanded;
        UpdateHandCardPositions(isExpanded);
    
        // 恢复抬起状态
        Transform content = transform.parent;
        foreach (int cardId in liftedHandCardIds)
        {
            foreach (Transform child in content)
            {
                HandCard hc = child.GetComponent<HandCard>();
                if (hc != null && hc.charId == this.charId && hc.cardId == cardId)
                {
                    hc.Lift();
                    break;
                }
            }
        }
    }

    // 平滑移动协程
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
    
    // 复制到用卡区（只复制角色卡，不复制手牌）
    void CopyToUserArea()
{
    // 检查全局角色卡上限
    if (CardSystemManager.Instance.GetUserAreaCharCount() >= CardSystemManager.Instance.cardCountConfig.CharlimitCount)
    {
        Debug.LogWarning($"用卡区角色卡已达上限{CardSystemManager.Instance.cardCountConfig.CharlimitCount}张");
        return;
    }
    
    // 保持原有同角色卡唯一检查
    if (CardSystemManager.Instance.IsCharCardExistInUserArea(charId))
    {
        Debug.LogWarning($"角色卡 {charId} 已在使用中");
        return;
    }
    
    Transform userContent = CardSystemManager.GetUserContent();
    if (userContent == null) return;
    
    float charX = (charId - 1) * 293f;
    Vector3 charTargetPos = new Vector3(charX, 0f, 0f);
    GameObject newCharCard = Instantiate(gameObject, userContent);
    CharCard newCharComp = newCharCard.GetComponent<CharCard>();
    RectTransform newCharRect = newCharComp.rectTransform;
    
    newCharComp.isInSelectArea = false;
    newCharComp.isExpanded = false;
    newCharRect.anchoredPosition = charTargetPos;
    newCharComp.originalPosition = charTargetPos;
    
    Debug.Log($"角色卡 {charId} 已复制到用卡区，位置X={charX}");
}

    // 角色卡被删除时，同时删除对应的手牌
    // 删除角色卡（用卡区右键调用）
    void DeleteCharCard()
{
    Transform userContent = transform.parent;
    foreach (Transform child in userContent)
    {
        HandCard hc = child.GetComponent<HandCard>();
        if (hc != null && hc.charId == this.charId)
        {
            Destroy(hc.gameObject);
        }
    }
    
    // 同步删除存档条目
    if (!isInSelectArea)
    {
        CardSystemManager.Instance.RemoveUserCharCardFromSave(charId);
    }
    
    Destroy(gameObject);
    Debug.Log($"用卡区角色卡 {charId} 及关联手牌已删除");
}
}