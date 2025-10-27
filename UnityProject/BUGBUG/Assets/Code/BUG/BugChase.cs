using Code.BUG;
using Code.Data;
using Code.UI;
using Code.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BugChase : SingletonMonoBehaviour<BugChase>
{
    [Header("动画对象")]public Animator AnimObject;
    [Header("名字对象")]public GameObject TextObject;
    [Header("延迟开始（秒）")] public float startDelay = 1.8f;

    [Header("最大速度（像素/秒）")] public float maxSpeed = 400f;
    [Header("停止距离（像素）")] public float stopDist = 2f;
    [Header("移动缓动曲线")] public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("旋转缓动曲线")] public AnimationCurve rotateCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Header("最大旋转速度（度/秒）")] public float maxRotateSpeed = 720f;

    [Header("点击显示内容")] 
    [SerializeField]
    public DialogInfo info;

    [Header("当前虫名")]
    [SerializeField]
    public string BugName = "DefaultBug";
    
    public Button bugButton;

    [SerializeField] public Camera uiCam;
    
    //anim
    [SerializeField]
    public RuntimeAnimatorController LadybugAnimator;
    [SerializeField]
    public RuntimeAnimatorController Y2KBugAnimator;
    
    //运动相关
    private RectTransform rectTrans;
    private RectTransform rotateTrans;
    private Vector2 targetPos;
    private float startTime;
    private bool started = false;

    [SerializeField] public float PassedDistance;
    [SerializeField] public float PassedBorder = 20;
    private bool bordered = false;
    
    /* 旋转相关 */
    private float currentAngle;          // 当前欧拉角（z）
    private bool wasMoving = true;       // 上一帧是否在移动
    private Animator animator;
    private TextMeshProUGUI nameTMP;
    private TMP_InputField inputField;

    [Header("贴边爬行 未完成")]
    public float edgeSpeed = 300f;          // 沿边速度
    public bool  clockwise = true;          //  true=顺时针  false=逆时针
    private bool onEdge = false;            // 是否正在贴边
    private int  edgeDir;                   // 当前在哪条边 0左 1上 2右 3下
    private Vector2 edgeTangent;            // 切线方向

    private float dist;
    
    [Header("虫子爬行模拟")]
    public bool  randomCrawl = true;        // 开关虫子模式
    public float nextTargetDist = 10f;      // 到达目标后重新随机（像素）
    public float idleChance = 0.25f;        // 概率呆滞、时长
    public float idleTimeMin = 0.2f;
    public float idleTimeMax = 0.8f;

    private float idleEndTime = 0f;         // 发呆结束时间
    
    void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
        rotateTrans = AnimObject.GetComponent<RectTransform>();
        animator = AnimObject.GetComponent<Animator>();
        nameTMP = TextObject.GetComponent<TextMeshProUGUI>();
        inputField = GetComponentInChildren<TMP_InputField>();
        idleEndTime = Time.time + idleTimeMin;
    }

    void Start()
    {
        startTime = Time.time + startDelay;
        nameTMP.text = BugName;
    }

    public void ChangeName()
    {
        if (BugName != nameTMP.text)
        {
            BugName = nameTMP.text;
            DialogBox.Show(new DialogInfo($"新名字：{BugName}"));
        }
            
    }

    void RandomNextTarget()
    {
        Vector2 half = new Vector2(1920 / 2, 1080 / 2);

        // 随机点（留 30 像素边距）
        float rx = Random.Range(-half.x + 30f, half.x - 30f);
        float ry = Random.Range(-half.y + 30f, half.y - 30f);
        targetPos = new Vector2(rx, ry);

        // 随机发呆
        if (Random.value < idleChance)
            idleEndTime = Time.time + Random.Range(idleTimeMin, idleTimeMax);
    }
    
    
    void Update()
    {
        // 1. 延迟阶段
        if (!started && Time.time >= startTime)
            started = true;
        if (!started) return;

        
        Vector2 currentPos = rectTrans.anchoredPosition;
        dist = Vector2.Distance(currentPos, targetPos);
        // ===== 1. 随机爬行模式 =====
        if (randomCrawl)
        {
            if (Time.time < idleEndTime)      // 发呆中
                return;

            
            if (dist <= nextTargetDist)       // 到达 or 第一次
                RandomNextTarget();
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTrans.parent as RectTransform,
                Input.mousePosition,
                uiCam,
                out targetPos);
        }
        
        bool moving = dist > stopDist;
        
        /*/* ===== 贴边爬行独立管道 ===== #1#
        if (onEdge)
        {
            // 一直沿切线走
            Vector2 next = rectTrans.anchoredPosition + edgeTangent * (maxSpeed * Time.deltaTime);

            // 走到角落后自动拐到下一个边
            RectTransform prt = rectTrans.parent as RectTransform;
            Vector2 half = prt.sizeDelta * 0.5f;
            bool turn = false;
            switch (edgeDir)
            {
                case 0: if (next.y >= half.y)  { edgeDir = 1; turn = true; } else if (next.y <= -half.y) { edgeDir = 3; turn = true; } break;
                case 1: if (next.x >= half.x)  { edgeDir = 2; turn = true; } else if (next.x <= -half.x) { edgeDir = 0; turn = true; } break;
                case 2: if (next.y <= -half.y) { edgeDir = 3; turn = true; } else if (next.y >= half.y)  { edgeDir = 1; turn = true; } break;
                case 3: if (next.x <= -half.x) { edgeDir = 0; turn = true; } else if (next.x >= half.x)  { edgeDir = 2; turn = true; } break;
            }
            if (turn)
                edgeTangent = clockwise ? new Vector2(-edgeTangent.y, edgeTangent.x)
                    : new Vector2(edgeTangent.y, -edgeTangent.x);

            rectTrans.anchoredPosition = next;

            // 旋转：让头部朝切线方向
            float tgtAngle = Vector2.SignedAngle(Vector2.up, edgeTangent);
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, tgtAngle, maxRotateSpeed * Time.deltaTime);
            rotateTrans.localRotation = Quaternion.Euler(0, 0, currentAngle);

            animator.SetBool("Moving", true);
            animator.SetFloat("Speed", 1f);
            return;   // 贴边期间不走原始逻辑
        }*/
        
        
        
        
        // mov
        if (moving && !Timer.Instance.Death)
        {
            float t = Mathf.Clamp01(dist / (stopDist + maxSpeed * 0.5f));
            float curveSpeed = speedCurve.Evaluate(t) * maxSpeed;
            Vector2 dir = (targetPos - currentPos).normalized;
            rectTrans.anchoredPosition = Vector2.MoveTowards(
                currentPos, targetPos, curveSpeed * Time.deltaTime);
            if (Timer.Instance.Y2KStage)
            {
                PassedDistance +=  curveSpeed * Time.deltaTime;
                if (!bordered && PassedDistance >= PassedBorder)
                {
                    bordered = true;
                    DialogBox.Show(new DialogInfo("它在吃桌面？？？"));
                }
            }
            
        }

        // rot
        if (moving&& !Timer.Instance.Death)
        {
            // angle
            Vector2 dir = (targetPos - currentPos).normalized;
            float targetAngle = Vector2.SignedAngle(Vector2.up, dir); 
            float delta = Mathf.DeltaAngle(currentAngle, targetAngle);

            // 缓动
            float rotateT = Mathf.Clamp01(Mathf.Abs(delta) / 180f);
            float rotateSpeed = rotateCurve.Evaluate(rotateT) * maxRotateSpeed;
            float maxStep = rotateSpeed * Time.deltaTime;

            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxStep);
            rotateTrans.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }

        wasMoving = moving;

        if (moving)
        {
            animator.SetFloat("Speed", Mathf.Abs(speedCurve.Evaluate(currentAngle)) / maxSpeed);
            animator.SetBool("Moving", moving);
        }
        else
        {
            animator.SetFloat("Speed",0);
            animator.SetBool("Moving", false);
        }
            
    }
    
    /*void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent(out BorderLine _)) return;   // 只认边界
        if (onEdge) return;                                   // 已经在贴边就不管

        // 计算当前在哪条边
        RectTransform parentRT = rectTrans.parent as RectTransform;
        Vector2 p = rectTrans.anchoredPosition;
        Vector2 half = parentRT.sizeDelta * 0.5f;
        float dx = half.x - Mathf.Abs(p.x);
        float dy = half.y - Mathf.Abs(p.y);

        if (dx < dy)            // 左右边
        {
            edgeDir = p.x < 0 ? 0 : 2;
            edgeTangent = clockwise ? Vector2.up : Vector2.down;
        }
        else                    // 上下边
        {
            edgeDir = p.y > 0 ? 1 : 3;
            edgeTangent = clockwise ? Vector2.right : Vector2.left;
        }

        onEdge = true;
        animator.SetFloat("Edge", 1f);      // 切到贴边动画
    }*/

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.TryGetComponent(out BorderLine _)) return;
        onEdge = false;
        animator.SetFloat("Edge", 0f);      // 回到飞行动画
    }

    
    
    public void OnClickSetBox()
    {
        //DialogBox.Show(new DialogInfo($"咦？{BugName}的样子……"));
        if (Timer.Instance.Y2KStage)
        {
            DialogBox.ShowNoDupicated(new DialogInfo("这到底是什么玩意儿？bug了吗？"));
        }
        else
        {
            DialogBox.ShowNoDupicated(new DialogInfo("这是什么东西？桌宠？"));
        }
    }

    public void ToY2K()
    {
        AnimObject.runtimeAnimatorController = Y2KBugAnimator;
        inputField.interactable = false;
    }

    public void Death()
    {
        AnimObject.SetBool("Death",true);
        bugButton.onClick.RemoveAllListeners();
        bugButton.onClick.AddListener(FileManager.Instance.MoveLogAndOpenFolder);
    }
    
}
