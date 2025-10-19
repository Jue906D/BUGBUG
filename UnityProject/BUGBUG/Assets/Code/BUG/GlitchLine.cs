using Code.Data;

namespace Code.BUG
{
    using UnityEngine;
    using UnityEngine.UI;   // 如果要用 Canvas 的 camera

    [RequireComponent(typeof(LineRenderer), typeof(RectTransform))]
    public class GlitchLine : MonoBehaviour
    {
        LineRenderer lr;
        RectTransform rt;
        Camera uiCam;   // 如果是 Overlay 画布，uiCam = null
        Vector3 lastWorldPos;
        const float minGap = 5f;      // 屏幕像素，想密就调小

        void Awake()
        {
            lr = GetComponent<LineRenderer>();
            lr.useWorldSpace = true;
            //lr.startWidth = lr.endWidth = 0.02f; 
            lr.startWidth = 0.15f;
            lr.endWidth   = 0.15f;
            lr.positionCount = 0;

            rt = GetComponent<RectTransform>();

            // 
            Canvas c = GetComponentInParent<Canvas>();
            uiCam = c.renderMode == RenderMode.ScreenSpaceOverlay ? null : c.worldCamera;
        }

        void Update()
        {
            if(!Timer.Instance.Y2KStage ||Timer.Instance.Death)
                return;
            //
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCam, rt.position);

            //
            Vector3 worldPos = (uiCam ? uiCam : Camera.main).ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 1f));

            //
            if (Vector3.Distance(worldPos, lastWorldPos) > minGap * 0.01f) // 0.01 把像素转世界
            {
                lastWorldPos = worldPos;
                lr.positionCount++;
                lr.SetPosition(lr.positionCount - 1, worldPos);
            }
        }
    }
}