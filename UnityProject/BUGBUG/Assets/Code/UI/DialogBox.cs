using System;
using System.Collections.Generic;
using Code.Utils;

namespace Code.UI
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    [Serializable]
    public struct DialogInfo
    {
        public string content;
        
        public DialogInfo(string content) { this.content = content; }

        public  bool Equals(DialogInfo other)
        {
            return content == other.content;
        }
    }
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogBox : SingletonMonoBehaviour<DialogBox>
    {
        [Header("UI")] [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;

        [Header("Param")] [SerializeField] private float fadeDuration = 0.3f; // 淡入/淡出时间
        [SerializeField] private float displayTime = 5f; // 完全显示后等待时间

        private CanvasGroup canvasGroup;
        private bool isPlaying;

        public static readonly DialogInfo EmptyInfo = new DialogInfo(string.Empty);
        
        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
        }

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        
        public static void Show(DialogInfo info)
        {
            Instance.Enqueue(info);
            Instance.lastInfo = info;
        }
        
        public static void ShowNoDupicated(DialogInfo info)
        {
            if (!Instance.lastInfo.Equals(info))
            {
                Instance.Enqueue(info);
                Instance.lastInfo = info;
            }
        }


        private readonly Queue<DialogInfo> queue =
            new Queue<DialogInfo>();

        private DialogInfo lastInfo;

        private void Enqueue(DialogInfo info)
        {
            queue.Enqueue(info);
            if (!isPlaying)
                StartCoroutine(PlayQueue());
        }

        private IEnumerator PlayQueue()
        {
            isPlaying = true;
            while (queue.Count > 0)
            {
                var info = queue.Dequeue();
                yield return StartCoroutine(ShowAndHide(info));
            }

            lastInfo = EmptyInfo;
            isPlaying = false;
        }

        private IEnumerator ShowAndHide(DialogInfo info)
        {
            //titleText.text = info.title;
            contentText.text = info.content;
            // 2. 淡入
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            yield return Fade(0, 1, fadeDuration);
            // 3. 停留
            yield return new WaitForSeconds(displayTime);
            // 4. 淡出
            yield return Fade(1, 0, fadeDuration);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = to;
        }
    }
}