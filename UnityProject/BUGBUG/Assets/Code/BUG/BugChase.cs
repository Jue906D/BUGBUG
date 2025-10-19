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

    void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
        rotateTrans = AnimObject.GetComponent<RectTransform>();
        animator = AnimObject.GetComponent<Animator>();
        nameTMP = TextObject.GetComponent<TextMeshProUGUI>();
        inputField = GetComponentInChildren<TMP_InputField>();
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

    void Update()
    {
        // 1. 延迟阶段
        if (!started && Time.time >= startTime)
            started = true;
        if (!started) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTrans.parent as RectTransform,
            Input.mousePosition,
            null,
            out targetPos);

        Vector2 currentPos = rectTrans.anchoredPosition;
        float dist = Vector2.Distance(currentPos, targetPos);
        bool moving = dist > stopDist;
 
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
