using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(UILineRenderer))]
public class UITrailStencil : MonoBehaviour
{
    [SerializeField]
    public Transform target;          // 拖你的物体或鼠标指针
    [SerializeField]
    public float interval = 10f;      // 像素间隔
    [SerializeField]
    public int maxPoint = 200;

    private UILineRenderer lr;
    private Vector2[] points;
    private int count = 0;
    private Vector2 last;

    void Awake()
    {
        lr = GetComponent<UILineRenderer>();
        points = new Vector2[maxPoint];
        //lr.color = Color.white;
        lr.LineThickness = 20f;     // 像素单位
        //lr.UseMargins = false;
    }

    void Update()
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, target.position);
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, screenPos, null, out localPos);

        if (count == 0 || Vector2.Distance(localPos, last) > interval)
        {
            if (count < maxPoint)
            {
                points[count] = localPos;
                count++;
                lr.Points = points[..count];
                last = localPos;
            }
        }
    }
}