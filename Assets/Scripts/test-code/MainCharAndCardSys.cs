using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    public int cardId;
    public int charId;
    public RectTransform rectTrans;
    public float rotateZ;
    public Vector2 basePos;
    public bool isLifted;
    public HandCard handCard;
    // 新增：记录卡牌原始尺寸（宽286，高414）
    public Vector2 cardSize = new Vector2(286, 414);
}

public class MainCharAndCardSys : MonoBehaviour
{
    // 单例（解决HandCard性能问题）
    public static MainCharAndCardSys Instance;

    [Header("核心引用")]
    public RectTransform littleCard;
    public GameObject SelfRoundCard;
    public GameObject CardPath;
    [HideInInspector] public List<HandCard> cardList = new List<HandCard>(); // 自动从CardPath子对象收集

    [Header("动画参数")]
    public float guideAnimDuration = 1f;
    public float layoutAnimDuration = 0.3f;
    public float playCardAnimDuration = 0.3f;
    public float liftHeightBase = 80f;

    // 全局布局参数（完整覆盖1-5张牌）
    private Dictionary<int, List<Vector2>> cardPosDict = new Dictionary<int, List<Vector2>>()
    {
        {5, new List<Vector2>(){new(-414,50), new(-200,50), new(0,50), new(200,50), new(414,50)}},
        {4, new List<Vector2>(){new(-343,50), new(-143,50), new(57,50), new(271,50)}},
        {3, new List<Vector2>(){new(-200,50), new(0,50), new(200,50)}},
        {2, new List<Vector2>(){new(-143,50), new(57,50)}},
        {1, new List<Vector2>(){new(0,50)}}
    };

    private Dictionary<int, List<Vector2>> cardPivotDict = new Dictionary<int, List<Vector2>>()
    {
        {5, new List<Vector2>(){new(1,0), new(0.5f,0), new(0.5f,0.5f), new(0.5f,0), new(0,0)}},
        {4, new List<Vector2>(){new(1,0), new(0.5f,0), new(0.5f,0), new(0,0)}},
        {3, new List<Vector2>(){new(0.5f,0), new(0.5f,0.5f), new(0.5f,0)}},
        {2, new List<Vector2>(){new(0.5f,0), new(0.5f,0)}},
        {1, new List<Vector2>(){new(0.5f,0.5f)}}
    };

    private Dictionary<int, List<float>> cardRotateDict = new Dictionary<int, List<float>>()
    {
        {5, new List<float>(){10f,5f,0f,-5f,-10f}},
        {4, new List<float>(){8f,4f,-4f,-8f}},
        {3, new List<float>(){5f,0f,-5f}},
        {2, new List<float>(){4f,-4f}},
        {1, new List<float>(){0f}}
    };

    private List<CardData> currentCardDatas = new List<CardData>();
    [HideInInspector] public bool isAnimPlaying = false; // 公开供HandCard访问
    [HideInInspector] public bool isClickPlaying = false; 
    
    // 核心工具方法：计算轴心变化时的位置补偿（保证视觉位置不变）
    private Vector2 CalculatePosOffsetForPivotChange(CardData card, Vector2 oldPivot, Vector2 newPivot)
    {
        float offsetX = (newPivot.x - oldPivot.x) * card.cardSize.x;
        float offsetY = (newPivot.y - oldPivot.y) * card.cardSize.y;
        return new Vector2(offsetX, offsetY);
    }

    // 新增：单张卡牌轴心缓动动画（同步补偿位置）
    IEnumerator AnimateSingleCardPivot(CardData card, Vector2 targetPivot, float duration)
    {
        float elapsed = 0f;
        Vector2 startPivot = card.rectTrans.pivot;
        Vector2 startPos = card.rectTrans.anchoredPosition;
        
        // 计算目标位置（轴心变化后的补偿位置）
        Vector2 posOffset = CalculatePosOffsetForPivotChange(card, startPivot, targetPivot);
        Vector2 targetPos = startPos + posOffset;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // 同步插值轴心和位置
            card.rectTrans.pivot = Vector2.Lerp(startPivot, targetPivot, t);
            card.rectTrans.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 最终修正
        card.rectTrans.pivot = targetPivot;
        card.rectTrans.anchoredPosition = targetPos;
    }

    // 新增：批量卡牌轴心缓动动画（同步补偿位置）
    IEnumerator AnimateCardsToPivot(List<Vector2> targetPivotList, float duration)
    {
        float elapsed = 0f;
        Dictionary<CardData, Vector2> startPivot = new();
        Dictionary<CardData, Vector2> startPos = new();
        Dictionary<CardData, Vector2> targetPos = new();

        for (int i = 0; i < currentCardDatas.Count; i++)
        {
            CardData card = currentCardDatas[i];
            Vector2 sPivot = card.rectTrans.pivot;
            Vector2 tPivot = targetPivotList[i];
            
            startPivot.Add(card, sPivot);
            startPos.Add(card, card.rectTrans.anchoredPosition);
            // 计算每张卡的目标位置（轴心补偿后）
            Vector2 offset = CalculatePosOffsetForPivotChange(card, sPivot, tPivot);
            targetPos.Add(card, startPos[card] + offset);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < currentCardDatas.Count; i++)
            {
                CardData card = currentCardDatas[i];
                // 同步插值轴心和位置
                card.rectTrans.pivot = Vector2.Lerp(startPivot[card], targetPivotList[i], t);
                card.rectTrans.anchoredPosition = Vector2.Lerp(startPos[card], targetPos[card], t);
            }
            yield return null;
        }

        // 最终修正
        for (int i = 0; i < currentCardDatas.Count; i++)
        {
            CardData card = currentCardDatas[i];
            card.rectTrans.pivot = targetPivotList[i];
            card.rectTrans.anchoredPosition = targetPos[card];
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 初始化逻辑已迁移至 Init() 方法
    }

    public void Init()
    {
        // 初始状态
        SelfRoundCard.SetActive(true);
        //CardPath.SetActive(false);
        //littleCard.gameObject.SetActive(false);

        // // 初始化卡牌数据（修正ID配置）
        // InitCardDatas();
        //
        // // 启动引导动画
        // StartCoroutine(GuidanceAnimationCoroutine());
    } 
    
    // 修正：正确初始化卡牌ID和尺寸
    public void InitCardDatas()
    {
        currentCardDatas.Clear();
        cardList.Clear();
        //CardPath.SetActive(true);

        // ========== 新增：自动从CardPath子对象收集HandCard ==========
        if (CardPath != null)
        {
            // 参数 true 表示包含未激活的子物体，适配初始隐藏状态
            HandCard[] childHandCards = CardPath.GetComponentsInChildren<HandCard>(true);
            cardList.AddRange(childHandCards);
        }
        else
        {
            Debug.LogWarning("CardPath 未赋值，无法收集手牌组件");
            return;
        }
        // ==========================================================

        int[] charIds = {1,1,10,11,1};
        int[] cardIds = {1,1,10,11,1};
        for (int i = 0; i < cardList.Count && i < 5; i++)
        {
            CardData data = new CardData();
            data.cardId = cardIds[i];
            data.charId = charIds[i];
            data.handCard = cardList[i];
            data.rectTrans = cardList[i].rectTransform;
            data.rotateZ = 0;
            data.isLifted = false;
            data.cardSize = new Vector2(286, 414);
            currentCardDatas.Add(data);
        }

        RebindAllCardEvents();
    }

    // 核心修复：重新绑定所有卡牌事件（解决出牌后索引错位）
    void RebindAllCardEvents()
    {
        for (int i = 0; i < currentCardDatas.Count; i++)
        {
            int currentIndex = i; // 局部变量捕获，彻底解决闭包问题
            currentCardDatas[i].handCard.onLeftClick = () => OnCardLeftClick(currentIndex);
            currentCardDatas[i].handCard.onRightClick = () => OnCardRightClick(currentIndex);
        }
    }

    // 引导动画（抛物线）
    public IEnumerator GuidanceAnimationCoroutine()
    {
        isAnimPlaying = true;
        littleCard.gameObject.SetActive(true);
        littleCard.anchoredPosition = Vector2.zero;

        Vector2 start = Vector2.zero;
        Vector2 mid = new(400,150);
        Vector2 end = new(800,0);
        float elapsed = 0f;

        while (elapsed < guideAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / guideAnimDuration);
            Vector2 p1 = Vector2.Lerp(start, mid, t);
            Vector2 p2 = Vector2.Lerp(mid, end, t);
            littleCard.anchoredPosition = Vector2.Lerp(p1, p2, t);
            yield return null;
        }

        littleCard.gameObject.SetActive(false);
        CardPath.SetActive(true);
        InitCardDatas();

        // 激活所有卡牌并重置状态
        foreach (var data in currentCardDatas)
        {
            data.rectTrans.gameObject.SetActive(true);
            data.rectTrans.anchoredPosition = Vector2.zero;
            data.rectTrans.pivot = new(0.5f,0.5f);
            data.rectTrans.rotation = Quaternion.identity;
        }

        // 启动初始布局
        StartCoroutine(LayoutAnimationCoroutine(currentCardDatas.Count));
    }

    // 通用布局动画（支持1-5张牌，自动匹配参数）
    IEnumerator LayoutAnimationCoroutine(int cardCount)
    {
        isAnimPlaying = true;
        var posList = cardPosDict[cardCount];
        var pivotList = cardPivotDict[cardCount];
        var rotateList = cardRotateDict[cardCount];

        // 阶段0：上移到(0,50)（轴心保持0.5,0.5）
        yield return StartCoroutine(AnimateAllCardsToPos(new Vector2(0,50), layoutAnimDuration));

        // 阶段1：横向散开（轴心仍为0.5,0.5，位置先到位）
        yield return StartCoroutine(AnimateCardsToPosList(posList, layoutAnimDuration));

        // 阶段2：轴心过渡动画（同步补偿位置）
        yield return StartCoroutine(AnimateCardsToPivot(pivotList, layoutAnimDuration));

        // 阶段3：旋转动画
        yield return StartCoroutine(AnimateCardsToRotate(rotateList, layoutAnimDuration));

        // 保存基准数据（保存补偿后的最终位置）
        for (int i = 0; i < currentCardDatas.Count; i++)
        {
            currentCardDatas[i].basePos = currentCardDatas[i].rectTrans.anchoredPosition;
            currentCardDatas[i].rotateZ = rotateList[i];
            currentCardDatas[i].handCard.SetLayoutPosition(currentCardDatas[i].basePos);
        }

        // 重新绑定事件（确保索引正确）
        RebindAllCardEvents();

        isAnimPlaying = false;

        
    }

    // 出牌+补位动画
    public void OnCardPlayed(int index)
    {
        StartCoroutine(PlayCardCoroutine(index));
    }

    IEnumerator PlayCardCoroutine(int index)
    {
        InitFightRoomSystem.Instance.OnSelfCardPlayed(2);
        
        isAnimPlaying = true;
        CardData playedCard = currentCardDatas[index];
        List<CardData> remainCards = new(currentCardDatas);
        remainCards.RemoveAt(index);
        int newCount = remainCards.Count;
        
        // ========== 出牌卡牌动画 ==========
        // 阶段1：旋转归0
        yield return StartCoroutine(AnimateSingleCardRotate(playedCard, 0f, 0.1f));
        // 阶段2：轴心过渡到中心（同步补偿位置回中心）
        yield return StartCoroutine(AnimateSingleCardPivot(playedCard, new(0.5f, 0.5f), 0.1f));
        // 阶段3：移动到出牌位置
        yield return StartCoroutine(AnimateSingleCardPos(playedCard, new(0,540), playCardAnimDuration));

        // 更新当前卡牌列表
        currentCardDatas = remainCards;
        
        if (currentCardDatas.Count == 0)
        {
            if (CharAnimSystem.Instance != null)
            {
                CharAnimSystem.Instance.TriggerBattleAnims();
            }
            yield return new WaitForSeconds(1f);
            Destroy(playedCard.rectTrans.gameObject);
            yield break;
        }
        
        // ========== 剩余卡牌补位 ==========
        // 阶段1：旋转归0
        List<float> zeroRotates = new();
        for (int i = 0; i < remainCards.Count; i++) zeroRotates.Add(0f);
        yield return StartCoroutine(AnimateCardsToRotate(zeroRotates, 0.1f));
        
        // 阶段2：轴心改回中心（同步补偿位置）
        List<Vector2> centerPivots = new();
        for (int i = 0; i < remainCards.Count; i++) centerPivots.Add(new(0.5f, 0.5f));
        yield return StartCoroutine(AnimateCardsToPivot(centerPivots, 0.1f));
        
        // 阶段3：补位到新位置（轴心保持0.5,0.5）
        yield return StartCoroutine(AnimateCardsToPosList(cardPosDict[newCount], layoutAnimDuration));
        

        if (currentCardDatas.Count > 0)
        {           
             if (CharAnimSystem.Instance != null)
             {
                  CharAnimSystem.Instance.TriggerBattleAnims();
             }
             
            yield return  StartCoroutine(LayoutAnimationCoroutine(currentCardDatas.Count));
            // 销毁打出的卡牌（布局前销毁，完全匹配流程）
            Destroy(playedCard.rectTrans.gameObject);
            
            // ========== 新增：调用战斗动画 ==========
        }
        
    }

    // ========== 通用动画协程 ==========
    IEnumerator AnimateAllCardsToPos(Vector2 targetPos, float duration)
    {
        float elapsed = 0f;
        Dictionary<CardData, Vector2> startPos = new();
        foreach (var card in currentCardDatas)
        {
            startPos.Add(card, card.rectTrans.anchoredPosition);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            foreach (var card in currentCardDatas)
            {
                card.rectTrans.anchoredPosition = Vector2.Lerp(startPos[card], targetPos, t);
            }
            yield return null;
        }

        foreach (var card in currentCardDatas)
        {
            card.rectTrans.anchoredPosition = targetPos;
        }
    }

    IEnumerator AnimateCardsToPosList(List<Vector2> targetPosList, float duration)
    {
        float elapsed = 0f;
        Dictionary<CardData, Vector2> startPos = new();
        for (int i = 0; i < currentCardDatas.Count; i++)
        {
            startPos.Add(currentCardDatas[i], currentCardDatas[i].rectTrans.anchoredPosition);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < currentCardDatas.Count; i++)
            {
                currentCardDatas[i].rectTrans.anchoredPosition = Vector2.Lerp(startPos[currentCardDatas[i]], targetPosList[i], t);
            }
            yield return null;
        }

        for (int i = 0; i < currentCardDatas.Count; i++)
        {
            currentCardDatas[i].rectTrans.anchoredPosition = targetPosList[i];
        }
    }

    IEnumerator AnimateCardsToRotate(List<float> targetRotateList, float duration)
    {
        float elapsed = 0f;
        Dictionary<CardData, float> startRot = new();
        for (int i = 0; i < currentCardDatas.Count; i++)
        {
            float rot = currentCardDatas[i].rectTrans.rotation.eulerAngles.z;
            if (rot > 180) rot -= 360;
            startRot.Add(currentCardDatas[i], rot);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < currentCardDatas.Count; i++)
            {
                float currentRot = Mathf.Lerp(startRot[currentCardDatas[i]], targetRotateList[i], t);
                currentCardDatas[i].rectTrans.rotation = Quaternion.Euler(0,0,currentRot);
            }
            yield return null;
        }

        for (int i = 0; i < currentCardDatas.Count; i++)
        {
            currentCardDatas[i].rectTrans.rotation = Quaternion.Euler(0,0,targetRotateList[i]);
        }
    }

    IEnumerator AnimateSingleCardPos(CardData card, Vector2 targetPos, float duration)
    {
        float elapsed = 0f;
        Vector2 startPos = card.rectTrans.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            card.rectTrans.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        card.rectTrans.anchoredPosition = targetPos;
    }

    IEnumerator AnimateSingleCardRotate(CardData card, float targetRot, float duration)
    {
        float elapsed = 0f;
        float startRot = card.rectTrans.rotation.eulerAngles.z;
        if (startRot > 180) startRot -= 360;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentRot = Mathf.Lerp(startRot, targetRot, t);
            card.rectTrans.rotation = Quaternion.Euler(0,0,currentRot);
            yield return null;
        }

        card.rectTrans.rotation = Quaternion.Euler(0,0,targetRot);
    }

    // ========== 卡牌点击逻辑 ==========
    void OnCardLeftClick(int index)
    {
        if (isAnimPlaying|| isClickPlaying || index < 0 || index >= currentCardDatas.Count) return;

        CardData card = currentCardDatas[index];
        if (!card.isLifted)
        {
            // 第一次点击：抬起（调用HandCard原有Lift方法，保留所有原有功能）
            card.isLifted = true;
            card.handCard.Lift();
        }
        else
        {
            // 第二次点击：出牌（直接从抬起状态进入动画，不先落下）
            card.isLifted = false;
        
            // 手动清空全局效果面板（原Lower()方法中的逻辑，保留功能）
            if (CardSystemManager.globalEffectText != null)
            {
                CardSystemManager.globalEffectText.text = "";
            }
        
            // 直接触发出牌动画
            OnCardPlayed(index);
        }
    }

    void OnCardRightClick(int index)
    {
        if (isAnimPlaying || isClickPlaying || index < 0 || index >= currentCardDatas.Count || !currentCardDatas[index].isLifted) return;

        // 右键：落下（调用HandCard原有Lower方法）
        CardData card = currentCardDatas[index];
        card.isLifted = false;
        card.handCard.Lower();
    }
}