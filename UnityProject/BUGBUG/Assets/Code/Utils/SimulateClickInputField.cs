using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

namespace Code.Utils
{
    using UnityEngine;
    using TMPro;

    [RequireComponent(typeof(TMP_InputField))]
    public class SimpleRawPosTMP : MonoBehaviour
    {
        TMP_InputField input;
        private RectTransform rectTrans;
        private bool inCooldown = false;   // 是否处于冷却中
        void Awake()
        {
            input = GetComponent<TMP_InputField>();
            rectTrans = transform as RectTransform;
        }

        /* 一行代码拿到屏幕像素矩形 */
        /* 把 RectTransform 转成屏幕像素矩形 */
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
            if (inCooldown) return;
            if (!Input.GetKeyDown(KeyCode.Mouse0) && ! DirectorManager.GetInstance().isClick()) return;

            bool inside = GetScreenRect().Contains(Input.mousePosition);
            Debug.Log($"Click at {Input.mousePosition} {GetScreenRect() }");
            if (inside && !input.isFocused)
            {
                var ped = new PointerEventData(EventSystem.current)
                {
                    pointerId = -1,
                    position = Input.mousePosition,
                    button = PointerEventData.InputButton.Left
                };
                input.OnPointerClick(ped);   // 会同时 SetSelected + 打开输入
                Debug.Log($"Active");
            }

            if (!inside && input.isFocused)
            {
                input.DeactivateInputField();
                Debug.Log($"Deactive");
            }
        }
        
        private IEnumerator Cooldown(float t)
        {
            yield return new WaitForSeconds(t);
            inCooldown = false;            // 冷却结束，可再次执行
        }
    }
}