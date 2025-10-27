using Code.Data;
using UnityEngine;
using UnityEngine.UI;   // 为了 RectTransformUtility
using UnityEngine.UI.Extensions; // UILineRenderer

[RequireComponent(typeof(UILineRenderer))]
public class UIBrusher : MonoBehaviour
{
    public Transform target;        // 拖你的物体或鼠标指针
    public float interval = 15f;    // 像素间隔
    public int maxPoint = 200;
    public float lineThickness = 30f;

    private UILineRenderer lr;
    private Vector2[] points;
    private int count = 0;
    private RectTransform maskRT;
    private RectTransform selfRT;
    
    
    [Header("啃食参数")]
    public float baseThickness = 18f;      // 平均粗细
    public float chewAmplitude = 4f;       // 边缘抖动范围（像素）
    public float chewFreq = 0.3f;          // 抖动频率
    public Texture2D chewMask;             // 1×64 随机黑白像素条（点过滤）

    void Awake()
    {
        lr = GetComponent<UILineRenderer>();
        maskRT = transform.parent as RectTransform; // MaskRoot
        selfRT = transform as RectTransform;
        points = new Vector2[maxPoint];
        lr.LineThickness = lineThickness;
        lr.color = Color.white;
        
        CreateAlphaNoise();
        maskU = 0f;
    }

    private Texture2D alphaNoise;   // 1×64
    private float maskU;            // sample

    void CreateAlphaNoise()
    {
        alphaNoise = new Texture2D(1, 64, TextureFormat.Alpha8, false);
        alphaNoise.wrapMode = TextureWrapMode.Repeat;
        alphaNoise.filterMode = FilterMode.Point;
        for (int y = 0; y < 64; y++)
            alphaNoise.SetPixel(0, y, Color.clear * Random.value); 
        alphaNoise.Apply();
        lr.material = new Material(Shader.Find("UI/Default")); 
        lr.material.mainTexture = alphaNoise;
    }
    
    
    void Update()
    {
        if(!Timer.Instance.Y2KStage ||Timer.Instance.Death)
            return;
        
        // 1. 厚度抖动
        //float t = Time.time * chewFreq;
        //float thick = baseThickness + Mathf.PerlinNoise(t, 0) * chewAmplitude;
        //lr.LineThickness = thick;
        
        maskU += Time.deltaTime * chewFreq * 64f; // 在 1×64 条上滚动采样
        if (maskU >= 64f) maskU -= 64f;
        
        
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, target.position);
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(maskRT, screenPos, null, out localPos);

        if (count == 0 || Vector2.Distance(localPos, points[count - 1]) > interval)
        {
            if (count < maxPoint)
            {
                points[count] = localPos;
                count++;
                lr.Points = points[..count];

                // 让 Mask 区域 = 线的外接矩形
                UpdateMaskRect();
            }
        }
    }

    void UpdateMaskRect()
    {
        if (count == 0) return;

        Vector2 min = points[0];
        Vector2 max = points[0];
        for (int i = 1; i < count; i++)
        {
            min = Vector2.Min(min, points[i]);
            max = Vector2.Max(max, points[i]);
        }
        Vector2 size = max - min;
        Vector2 center = (min + max) * 0.5f;

        // 把 MaskRoot 的锚点区域设成外接矩形 + 一点外扩
        float padding = lineThickness;
        maskRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left,   center.x - size.x * 0.5f - padding, size.x + padding * 2);
        maskRT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, center.y - size.y * 0.5f - padding, size.y + padding * 2);
    }
}