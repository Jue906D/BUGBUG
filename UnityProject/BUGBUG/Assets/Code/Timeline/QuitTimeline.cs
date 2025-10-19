using UnityEngine;
using UnityEngine.Playables;

namespace Code.Timeline
{
    public class QuitTimeline : MonoBehaviour
    {
        void OnEnable()
        {
            GetComponent<PlayableDirector>().Play();
        }
    }
}