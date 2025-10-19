using UnityEngine;

namespace Code.Data
{
    public class Timer : MonoBehaviour
    {
        [SerializeField]
        public float RealTimePassed;
        [SerializeField]
        public System.DateTime DateTimeCur;
        void Start()
        {
            DateTimeCur = new System.DateTime(1999, 12, 31, 23, 59, 0, System.DateTimeKind.Utc);
        }

        void Update()
        {
            RealTimePassed += Time.deltaTime;
            DateTimeCur.AddSeconds(Time.deltaTime);
        }
    }
}