using System;
using UnityEngine;

namespace Code.Timeline
{
    public class QuitDirector : MonoBehaviour
    {
        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}