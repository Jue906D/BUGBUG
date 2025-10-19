using UnityEngine;

namespace Code.Data
{
    public class Timer : MonoBehaviour
    {
        [SerializeField]
        private float realTimePassed;
        void Start()
        {
            
        }

        void Update()
        {
            realTimePassed += Time.deltaTime;
        }
    }
}