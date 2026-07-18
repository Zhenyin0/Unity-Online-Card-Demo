using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharAnimSystem : MonoBehaviour
{
    // 单例
    public static CharAnimSystem Instance;

    [Header("核心定位对象")]
    public RectTransform CharRoundSelf;    // 我方定位对象
    public RectTransform CharRoundOther;   // 敌方定位对象
    public RectTransform PostionAnimalPath;// 角色父节点（CharAnim-1~6）
    public GameObject EffectAnim;          // 受击特效（初始未激活）

    [Header("动画参数")]
    public float attackAnimCheckInterval = 0.1f;
    public float maxAnimTimeout = 5f;
    public float charMoveAnimDuration = 0.3f; // 角色位置移动动画时长

    // 角色坐标配置（Y固定为0）
    private List<Vector2> selfPos_3 = new List<Vector2>() { new(-265, 0), new(-540, 0), new(-800, 0) };
    private List<Vector2> selfPos_2 = new List<Vector2>() { new(-300, 0), new(-700, 0) };
    private List<Vector2> selfPos_1 = new List<Vector2>() { new(-540, 0) };

    // 敌方坐标（镜像对称，X取反）
    private List<Vector2> otherPos_3 = new List<Vector2>() { new(265, 0), new(540, 0), new(800, 0) };
    private List<Vector2> otherPos_2 = new List<Vector2>() { new(300, 0), new(700, 0) };
    private List<Vector2> otherPos_1 = new List<Vector2>() { new(540, 0) };

    // 角色分组缓存（索引0=靠近X=0的第一个角色）
    private List<RectTransform> selfCharList = new List<RectTransform>();
    private List<RectTransform> otherCharList = new List<RectTransform>();

    // 动画组件缓存
    private Animator effectAnimator;
    private RectTransform effectRectTrans;

    // 状态标记
    private bool isBattleAnimPlaying = false;
    private bool hasSelfCharDied = false;  // 我方角色阵亡标记
    private bool hasOtherCharDied = false; // 敌方角色阵亡标记
    private bool isSelfAttackTurn = true; // 当前攻击方标记：true=我方攻击，false=敌方攻击，默认我方先手
    void Awake()
    {
        // 标准单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 缓存特效组件
        if (EffectAnim != null)
        {
            effectAnimator = EffectAnim.GetComponent<Animator>();
            effectRectTrans = EffectAnim.GetComponent<RectTransform>();
            EffectAnim.SetActive(false);
        }
    }

    void Start()
    {
        // 初始化逻辑已迁移至 Init() 方法
    }
    

    public void Init()
    {
        InitCharGroups();
        BindOtherCharClickEvents();
    }

    public IEnumerator CharRoundSelfPostion()
    {
        // 同步我方定位对象到第一个角色位置
        if (CharRoundSelf != null)
        {
            // ========== 修改：保留定位对象原Y轴 ==========
            Vector2 targetPos = selfCharList[0].anchoredPosition;
            targetPos.y = CharRoundSelf.anchoredPosition.y;
            yield return StartCoroutine(SmoothMove(CharRoundSelf, targetPos, charMoveAnimDuration));
            // ===========================================
        }

    }

    #region 初始化与点击事件
    public void InitCharGroups()
    {
        if (PostionAnimalPath == null)
        {
            Debug.LogError("[CharAnimSystem] PostionAnimalPath未赋值！");
            return;
        }
        selfCharList.Clear();
        otherCharList.Clear();

        foreach (RectTransform charObj in PostionAnimalPath)
        {
            Transform rightTrans = charObj.Find("ACT-CharAnimRight");
            Transform leftTrans = charObj.Find("ACT-CharAnimLeft");
            bool rightActive = rightTrans != null && rightTrans.gameObject.activeSelf;
            bool leftActive = leftTrans != null && leftTrans.gameObject.activeSelf;

            if (rightActive && !leftActive)
            {
                selfCharList.Add(charObj);
            }
            else if (leftActive && !rightActive)
            {
                otherCharList.Add(charObj);
            }
            else
            {
                otherCharList.Add(charObj);
                Debug.LogWarning($"[CharAnimSystem] 角色 {charObj.name} 状态异常，已跳过");
            }
        }

        // ========== 新增排序逻辑 ==========
        // 按X轴距离0点的远近排序：绝对值越小（越靠近中心），索引越靠前
        selfCharList.Sort((a, b) => Mathf.Abs(a.anchoredPosition.x).CompareTo(Mathf.Abs(b.anchoredPosition.x)));
        otherCharList.Sort((a, b) => Mathf.Abs(a.anchoredPosition.x).CompareTo(Mathf.Abs(b.anchoredPosition.x)));
        // =================================
        
        StartCoroutine(CharRoundSelfPostion());

        Debug.Log($"[CharAnimSystem] 初始化完成：己方 {selfCharList.Count} 个，敌方 {otherCharList.Count} 个");
    }

    private void BindOtherCharClickEvents()
    {
        foreach (RectTransform charObj in otherCharList)
        {
            Button charButton = charObj.GetComponent<Button>();
            if (charButton != null)
            {
                charButton.onClick.RemoveAllListeners();
                charButton.onClick.AddListener(() => OnOtherCharClick(charObj));
            }
            else
            {
                Debug.LogWarning($"[CharAnimSystem] 敌方角色 {charObj.name} 未挂载Button组件！");
            }
        }
    }

    private void OnOtherCharClick(RectTransform targetChar)
    {
        if (MainCharAndCardSys.Instance == null
            || MainCharAndCardSys.Instance.isAnimPlaying
            || isBattleAnimPlaying)
        {
            return;
        }
        if (CharRoundOther != null && targetChar != null)
        {
            Vector2 newPos = CharRoundOther.anchoredPosition;
            newPos.x = targetChar.anchoredPosition.x;
            CharRoundOther.anchoredPosition = newPos;
        }
    }
    #endregion

    #region 【对外接口】角色阵亡（仅接口，无死亡判定逻辑）
    /// <summary>
    /// 外部调用：通知角色阵亡
    /// 【说明】不实现任何血量/死亡判定逻辑，仅从分组列表移除角色并标记，回合结束统一补位
    /// </summary>
    /// <param name="diedChar">阵亡的角色RectTransform</param>
    public void OnCharacterDied(RectTransform diedChar)
    {
        if (diedChar == null) return;

        if (selfCharList.Contains(diedChar))
        {
            selfCharList.Remove(diedChar);
            hasSelfCharDied = true;
            Debug.Log($"[CharAnimSystem] 己方角色 {diedChar.name} 阵亡，回合结束后自动补位");
        }
        else if (otherCharList.Contains(diedChar))
        {
            otherCharList.Remove(diedChar);
            hasOtherCharDied = true;
            Debug.Log($"[CharAnimSystem] 敌方角色 {diedChar.name} 阵亡，回合结束后自动补位");
        }
        else
        {
            Debug.LogWarning($"[CharAnimSystem] 未找到阵亡角色 {diedChar.name} 所属分组");
        }
    }
    #endregion

    #region 【核心】回合结束位置刷新（我方/敌分分治，对称逻辑）
    /// <summary>
    /// 我方回合结束调用：我方角色轮转/补位 + 敌方定位对象归位
    /// </summary>
    public void RefreshSelfRoundEndPosition()
    {
        StartCoroutine(SelfRoundEndPosCoroutine());
    }

    /// <summary>
    /// 敌方回合结束调用：敌方角色轮转/补位 + 我方定位对象归位（对称逻辑）
    /// </summary>
    public void RefreshOtherRoundEndPosition()
    {
        StartCoroutine(OtherRoundEndPosCoroutine());
    }

    // 我方回合结束位置逻辑
    private IEnumerator SelfRoundEndPosCoroutine()
    {
        // 1. 处理我方角色：轮转 / 阵亡补位
        if (selfCharList.Count > 0)
        {
            if (hasSelfCharDied)
            {
                // 有阵亡：按剩余人数向左补位到标准坐标
                yield return StartCoroutine(CharFillPosition(selfCharList, GetTargetSelfPos(selfCharList.Count)));
                hasSelfCharDied = false;
            }
            else if (selfCharList.Count == 3)
            {
                // 3人满编：按标准坐标循环轮转
                yield return StartCoroutine(RotateThreeSelfChar());
            }
            else
            {
                yield return StartCoroutine(RotateTwoSelfChar());
            }

            // 同步我方定位对象到第一个角色位置
            if (CharRoundSelf != null)
            {
                // ========== 修改：保留定位对象原Y轴 ==========
                Vector2 targetPos = selfCharList[0].anchoredPosition;
                targetPos.y = CharRoundSelf.anchoredPosition.y;
                yield return StartCoroutine(SmoothMove(CharRoundSelf, targetPos, charMoveAnimDuration));
                // ===========================================
            }
        }

        // 2. 处理敌方：仅归位定位对象，不修改敌方角色本身位置
        if (otherCharList.Count > 0 && CharRoundOther != null)
        {
            // ========== 修改：保留定位对象原Y轴 ==========
            Vector2 targetPos = otherCharList[0].anchoredPosition;
            targetPos.y = CharRoundOther.anchoredPosition.y;
            yield return StartCoroutine(SmoothMove(CharRoundOther, targetPos, charMoveAnimDuration));
            // ===========================================
        }
    }

    // 敌方回合结束位置逻辑（对称）
    private IEnumerator OtherRoundEndPosCoroutine()
    {
        // 1. 处理敌方角色：轮转 / 阵亡补位
        if (otherCharList.Count > 0)
        {
            if (hasOtherCharDied)
            {
                Debug.LogWarning($"大聪明1");
                // 有阵亡：按剩余人数向左补位到标准坐标
                yield return StartCoroutine(CharFillPosition(otherCharList, GetTargetOtherPos(otherCharList.Count)));
                hasOtherCharDied = false;
            }
            else if (otherCharList.Count == 3)
            {
                Debug.LogWarning($"大聪明2");
                // 3人满编：按标准坐标循环轮转
                yield return StartCoroutine(RotateThreeOtherChar());
            }

            // 同步敌方定位对象到第一个角色位置
            if (CharRoundOther != null)
            {
                // ========== 修改：保留定位对象原Y轴 ==========
                Vector2 targetPos = otherCharList[0].anchoredPosition;
                targetPos.y = CharRoundOther.anchoredPosition.y;
                yield return StartCoroutine(SmoothMove(CharRoundOther, targetPos, charMoveAnimDuration));
                // ===========================================
            }
            
        }

        // 2. 处理我方：归位定位对象（对称兜底）
        if (selfCharList.Count > 0 && CharRoundSelf != null)
        {
            // ========== 修改：保留定位对象原Y轴 ==========
            Vector2 targetPos = selfCharList[0].anchoredPosition;
            targetPos.y = CharRoundSelf.anchoredPosition.y;
            yield return StartCoroutine(SmoothMove(CharRoundSelf, targetPos, charMoveAnimDuration));
            // ===========================================
        }
    }

    /// <summary>
    /// 攻防身份切换
    /// </summary>
    public void SwitchAttackAndHurtRole()
    {
        isSelfAttackTurn = !isSelfAttackTurn; // 翻转攻防状态
        Debug.Log($"[CharAnimSystem] 切换攻防身份：当前攻击方 = {(isSelfAttackTurn ? "我方" : "敌方")}");
    }
    #endregion

    #region 位置工具方法
    // 根据人数获取我方标准坐标
    private List<Vector2> GetTargetSelfPos(int count)
    {
        switch (count)
        {
            case 3: return new List<Vector2>(selfPos_3);
            case 2: return new List<Vector2>(selfPos_2);
            case 1: return new List<Vector2>(selfPos_1);
            default: return new List<Vector2>();
        }
    }

    // 根据人数获取敌方标准坐标（镜像）
    private List<Vector2> GetTargetOtherPos(int count)
    {
        switch (count)
        {
            case 3: return new List<Vector2>(otherPos_3);
            case 2: return new List<Vector2>(otherPos_2);
            case 1: return new List<Vector2>(otherPos_1);
            default: return new List<Vector2>();
        }
    }

    /// <summary>
    /// 通用UI平滑移动协程
    /// </summary>
    private IEnumerator SmoothMove(RectTransform rt, Vector2 targetPos, float duration)
    {
        float elapsed = 0f;
        Vector2 startPos = rt.anchoredPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        rt.anchoredPosition = targetPos;
    }

    /// <summary>
    /// 我方3角色循环轮转：基于标准坐标 1→3、2→1、3→2
    /// 轮转后同步更新列表顺序，保证索引与位置一一对应
    /// </summary>
    private IEnumerator RotateThreeSelfChar()
    {
        
            RectTransform c1 = selfCharList[0];
            RectTransform c2 = selfCharList[1];
            RectTransform c3 = selfCharList[2];
    
            // 基于固定标准坐标移动，而非当前实时位置
            Vector2 targetPos_c1 = selfPos_3[2]; // 1号角色去3号位
            Vector2 targetPos_c2 = selfPos_3[0]; // 2号角色去1号位
            Vector2 targetPos_c3 = selfPos_3[1]; // 3号角色去2号位
    
            // 并行移动三个角色
            Coroutine move1 = StartCoroutine(SmoothMove(c1, targetPos_c1, charMoveAnimDuration));
            Coroutine move2 = StartCoroutine(SmoothMove(c2, targetPos_c2, charMoveAnimDuration));
            Coroutine move3 = StartCoroutine(SmoothMove(c3, targetPos_c3, charMoveAnimDuration));
    
            yield return move1;
            yield return move2;
            yield return move3;
    
            // 更新列表顺序：新1号位=原2号，新2号位=原3号，新3号位=原1号
            selfCharList = new List<RectTransform> { c2, c3, c1 };
    }

    /// <summary>
    /// 敌方3角色循环轮转：基于标准坐标（对称逻辑）
    /// </summary>
    private IEnumerator RotateThreeOtherChar()
    {
        RectTransform c1 = otherCharList[0];
        RectTransform c2 = otherCharList[1];
        RectTransform c3 = otherCharList[2];

        Vector2 targetPos_c1 = otherPos_3[2];
        Vector2 targetPos_c2 = otherPos_3[0];
        Vector2 targetPos_c3 = otherPos_3[1];

        Coroutine move1 = StartCoroutine(SmoothMove(c1, targetPos_c1, charMoveAnimDuration));
        Coroutine move2 = StartCoroutine(SmoothMove(c2, targetPos_c2, charMoveAnimDuration));
        Coroutine move3 = StartCoroutine(SmoothMove(c3, targetPos_c3, charMoveAnimDuration));

        yield return move1;
        yield return move2;
        yield return move3;

        // 更新列表顺序
        otherCharList = new List<RectTransform> { c2, c3, c1 };
    }

    private IEnumerator RotateTwoSelfChar()
    {
        if (selfCharList.Count == 2)
        {
            RectTransform c1 = selfCharList[0];
            RectTransform c2 = selfCharList[1];
        
            // 基于固定标准坐标移动，而非当前实时位置
            Vector2 targetPos_c1 = selfPos_2[1]; // 1号角色去2号位
            Vector2 targetPos_c2 = selfPos_2[0]; // 2号角色去1号位
        
            // 并行移动三个角色
            Coroutine move1 = StartCoroutine(SmoothMove(c1, targetPos_c1, charMoveAnimDuration));
            Coroutine move2 = StartCoroutine(SmoothMove(c2, targetPos_c2, charMoveAnimDuration));
        
            yield return move1;
            yield return move2;
        
            // 更新列表顺序：新1号位=原2号，新2号位=原3号，新3号位=原1号
            selfCharList = new List<RectTransform> { c2, c1 };
        }
        
    }

    /// <summary>
    /// 角色补位：按标准坐标重新排布（向左靠拢X=0）
    /// </summary>
    private IEnumerator CharFillPosition(List<RectTransform> charList, List<Vector2> targetPosList)
    {
        if (charList.Count != targetPosList.Count || charList.Count == 0) yield break;

        // 并行移动所有角色
        List<Coroutine> moveCoroutines = new List<Coroutine>();
        for (int i = 0; i < charList.Count; i++)
        {
            moveCoroutines.Add(StartCoroutine(SmoothMove(charList[i], targetPosList[i], charMoveAnimDuration)));
        }

        foreach (var cor in moveCoroutines)
        {
            yield return cor;
        }
    }
    #endregion

    #region 原有战斗动画逻辑（完全保留，无修改）
    public void TriggerBattleAnims()
    {
        if (isBattleAnimPlaying) return;
        StartCoroutine(BattleAnimsCoroutine());
    }

    public void isBattleAnimChange( bool IsChang)
    {
        isBattleAnimPlaying = IsChang;
    }

    public void ToFightTriggerBattleAnims()
    {
        StartCoroutine(ToFightBattleAnimsCoroutine());
    }

    private IEnumerator BattleAnimsCoroutine()
    {
        isBattleAnimPlaying = true;
        try
        {
            if (isSelfAttackTurn)
            {
                // 我方回合：我方攻击 + 敌方受击
                yield return StartCoroutine(TriggerSelfAttackAnim());
                yield return StartCoroutine(TriggerOtherHurtAnim());
            }
            else
            {
                // 敌方回合：敌方攻击 + 我方受击
                yield return StartCoroutine(TriggerOtherAttackAnim());
                yield return StartCoroutine(TriggerSelfHurtAnim());
            }
        }
        finally
        {
            isBattleAnimPlaying = false;
        }
    }
    
    private IEnumerator ToFightBattleAnimsCoroutine()
    {
        isBattleAnimPlaying = true;
        try
        {
            if (isSelfAttackTurn)
            {
                // 我方回合：我方攻击 + 敌方受击
                yield return StartCoroutine(TriggerSelfAttackAnim());
                yield return StartCoroutine(TriggerOtherHurtAnim());
            }
            else
            {
                // 敌方回合：敌方攻击 + 我方受击
                yield return StartCoroutine(TriggerOtherAttackAnim());
                yield return StartCoroutine(TriggerSelfHurtAnim());
            }
        }
        finally
        {
            isBattleAnimPlaying = true;
        }
    }

    private IEnumerator TriggerSelfAttackAnim()
    {
        MainCharAndCardSys.Instance.isClickPlaying = true;
        if (CharRoundSelf == null || selfCharList.Count == 0) yield break;

        RectTransform attackChar = null;
        foreach (var charObj in selfCharList)
        {
            if (Mathf.Abs(charObj.anchoredPosition.x - CharRoundSelf.anchoredPosition.x) < 0.5f)
            {
                attackChar = charObj;
                break;
            }
        }
        if (attackChar == null)
        {
            Debug.LogWarning("[CharAnimSystem] 未找到匹配的己方角色");
            yield break;
        }

        Transform rightAnimTrans = attackChar.Find("ACT-CharAnimRight");
        Transform effectaudio = attackChar.Find("EffectAudio");
        AudioSource Audioeffect = effectaudio.GetComponent<AudioSource>();
        if (rightAnimTrans == null)
        {
            Debug.LogError($"[CharAnimSystem] 角色 {attackChar.name} 未找到ACT-CharAnimRight");
            yield break;
        }
        Animator attackAnimator = rightAnimTrans.GetComponent<Animator>();
        if (attackAnimator == null)
        {
            Debug.LogError($"[CharAnimSystem] 角色 {attackChar.name} Animator 缺失");
            yield break;
        }

        attackAnimator.SetTrigger("PlayAttack");
        Audioeffect.Play();
        float startTime = Time.time;
        while (attackAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            if (Time.time - startTime > maxAnimTimeout)
            {
                Debug.LogError("[CharAnimSystem] 攻击动画超时");
                yield break;
            }
            yield return new WaitForSeconds(attackAnimCheckInterval);
        }

        startTime = Time.time;
        while (!attackAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            if (Time.time - startTime > maxAnimTimeout)
            {
                Debug.LogError("[CharAnimSystem] 攻击动画回到Idle超时");
                yield break;
            }
            yield return new WaitForSeconds(attackAnimCheckInterval);
        }
    }

    private IEnumerator TriggerOtherHurtAnim()
    {
        if (CharRoundOther == null || EffectAnim == null || effectRectTrans == null) yield break;

        Vector2 effectPos = effectRectTrans.anchoredPosition;
        effectPos.x = CharRoundOther.anchoredPosition.x;
        effectRectTrans.anchoredPosition = effectPos;

        EffectAnim.SetActive(true);
        if (effectAnimator != null)
        {
            effectAnimator.Play("EffectAnim");
        }

        float startTime = Time.time;
        while (true)
        {
            if (Time.time - startTime > maxAnimTimeout)
            {
                Debug.LogError("[CharAnimSystem] 受击特效动画超时");
                break;
            }
            AnimatorStateInfo stateInfo = effectAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1f)
            {
                break;
            }
            yield return new WaitForSeconds(attackAnimCheckInterval);
        }

        EffectAnim.SetActive(false);
        MainCharAndCardSys.Instance.isClickPlaying = false;
    }
    
    // 敌方角色攻击动画
    private IEnumerator TriggerOtherAttackAnim()
    {
        MainCharAndCardSys.Instance.isClickPlaying = true;
        if (CharRoundOther == null || otherCharList.Count == 0) yield break;

        RectTransform attackChar = null;
        foreach (var charObj in otherCharList)
        {
            if (Mathf.Abs(charObj.anchoredPosition.x - CharRoundOther.anchoredPosition.x) < 0.5f)
            {
                attackChar = charObj;
                break;
            }
        }
        if (attackChar == null)
        {
            Debug.LogWarning("[CharAnimSystem] 未找到匹配的敌方攻击角色");
            yield break;
        }

        // Transform leftAnimTrans = attackChar.Find("ACT-CharAnimLeft");
        // if (leftAnimTrans == null)
        // {
        //     Debug.LogError($"[CharAnimSystem] 敌方角色 {attackChar.name} 未找到ACT-CharAnimLeft");
        //     yield break;
        // }
        // Animator attackAnimator = leftAnimTrans.GetComponent<Animator>();
        // if (attackAnimator == null)
        // {
        //     Debug.LogError($"[CharAnimSystem] 敌方角色 {attackChar.name} Animator 缺失");
        //     yield break;
        // }
        
        Transform middleAnimTrans = attackChar.Find("ACT-CharAnimMiddle");
        Transform effectaudio = attackChar.Find("EffectAudio");
        AudioSource Audioeffect = effectaudio.GetComponent<AudioSource>();
        if (middleAnimTrans == null)
        {
            Debug.LogError($"[CharAnimSystem] 敌方角色 {attackChar.name} 未找到ACT-CharAnimMiddle");
            yield break;
        }
        Animator attackAnimator = middleAnimTrans.GetComponent<Animator>();
        if (attackAnimator == null)
        {
            Debug.LogError($"[CharAnimSystem] 敌方角色 {attackChar.name} Animator 缺失");
            yield break;
        }

        attackAnimator.SetTrigger("PlayAttack");
        Audioeffect.Play();
        float startTime = Time.time;
        while (attackAnimator.GetCurrentAnimatorStateInfo(0).IsName("CharAnimIdle"))
        {
            if (Time.time - startTime > maxAnimTimeout)
            {
                Debug.LogError("[CharAnimSystem] 敌方攻击动画超时");
                yield break;
            }
            yield return new WaitForSeconds(attackAnimCheckInterval);
        }

        startTime = Time.time;
        while (!attackAnimator.GetCurrentAnimatorStateInfo(0).IsName("CharAnimIdle"))
        {
            if (Time.time - startTime > maxAnimTimeout)
            {
                Debug.LogError("[CharAnimSystem] 敌方攻击动画回到Idle超时");
                yield break;
            }
            yield return new WaitForSeconds(attackAnimCheckInterval);
        }
    }

    // 我方角色受击特效
    private IEnumerator TriggerSelfHurtAnim()
    {
        if (CharRoundSelf == null || EffectAnim == null || effectRectTrans == null) yield break;

        Vector2 effectPos = effectRectTrans.anchoredPosition;
        effectPos.x = CharRoundSelf.anchoredPosition.x;
        effectRectTrans.anchoredPosition = effectPos;

        EffectAnim.SetActive(true);
        if (effectAnimator != null)
        {
            effectAnimator.Play("EffectAnim");
        }

        float startTime = Time.time;
        while (true)
        {
            if (Time.time - startTime > maxAnimTimeout)
            {
                Debug.LogError("[CharAnimSystem] 我方受击特效动画超时");
                break;
            }
            AnimatorStateInfo stateInfo = effectAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1f)
            {
                break;
            }
            yield return new WaitForSeconds(attackAnimCheckInterval);
        }

        EffectAnim.SetActive(false);
        MainCharAndCardSys.Instance.isClickPlaying = false;
    }
    
    #endregion

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}