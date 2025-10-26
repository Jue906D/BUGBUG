using System.Runtime.InteropServices;

namespace Code.Utils
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class RawPosButton : MonoBehaviour
    {
        Button btn;
        RectTransform rectTrans;

        void Awake()
        {
            btn = GetComponent<Button>();
            rectTrans = transform as RectTransform;
        }
        Rect GetScreenRect()
        {
            Vector3[] corners = new Vector3[4];
            rectTrans.GetWorldCorners(corners);
            float xMin = float.MaxValue, yMin = float.MaxValue;
            float xMax = float.MinValue, yMax = float.MinValue;
            for (int i = 0; i < 4; ++i)
            {
                Vector3 sp = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[i]);
                xMin = Mathf.Min(xMin, sp.x);
                xMax = Mathf.Max(xMax, sp.x);
                yMin = Mathf.Min(yMin, sp.y);
                yMax = Mathf.Max(yMax, sp.y);
            }
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
        void Update()
        {
            // 当前鼠标屏幕坐标
            Vector2 mouse = Input.mousePosition;
            if (Input.GetKeyDown(KeyCode.Mouse0) || DirectorManager.GetInstance().isClick())
            {
                if ( GetScreenRect().Contains(mouse))
                {
                    Debug.Log($"Click at {Input.mousePosition} {GetScreenRect()}");
                    Debug.Log($"Active");
                    btn.onClick.Invoke();
                }
            }

        }
    }
}