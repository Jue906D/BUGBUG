using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIChaseMouseSmooth : MonoBehaviour
{
    [Header("延迟开始（秒）")] public float startDelay = 1.8f;

    [Header("最大速度（像素/秒）")] public float maxSpeed = 400f;

    [Header("停止距离（像素）")] public float stopDist = 2f;

    [Header("缓动曲线")] public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTrans;
    private Vector2 targetPos;
    private float startTime;
    private bool started = false;

    void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
        
    }

    void Start()
    {
        startTime = Time.time + startDelay;
    }

    void Update()
    {
        // 1. 延迟阶段
        if (!started && Time.time >= startTime)
            started = true;
        if (!started) return;

        // 2. 鼠标 → Canvas 空间
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTrans.parent as RectTransform,
            Input.mousePosition,
            null,
            out targetPos);

        Vector2 currentPos = rectTrans.anchoredPosition;
        float dist = Vector2.Distance(currentPos, targetPos);

        // 3. 距离内直接停
        if (dist <= stopDist) return;

        // 4. 用 0~1 映射曲线：0=起步，1=最远
        float t = Mathf.Clamp01(dist / (stopDist + maxSpeed * 0.5f));
        float curveSpeed = speedCurve.Evaluate(t) * maxSpeed;

        // 5. 朝目标移动
        Vector2 dir = (targetPos - currentPos).normalized;
        rectTrans.anchoredPosition = Vector2.MoveTowards(
            currentPos,
            targetPos,
            curveSpeed * Time.deltaTime);
    }
}
