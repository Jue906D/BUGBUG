using System;
using Code.UI;
using Code.Utils;
using TMPro;
using UnityEngine;

namespace Code.Data
{
    public class Timer : SingletonMonoBehaviour<Timer>
    {
        [SerializeField]
        public float RealTimePassed;
        [SerializeField]
        public System.DateTime DateTimeCur;

        public float TimeBorder = 60; 
        public float DeathBorder = 60; 

        [SerializeField]
        private TextMeshProUGUI TimerText;
        
        public bool Y2KStage;
        public bool Y2KTip;
        public bool Death;
        void Start()
        {
            DateTimeCur = new System.DateTime(1999, 12, 31, 23, 59, 0, System.DateTimeKind.Utc);
        }

        void Update()
        {
            RealTimePassed += Time.deltaTime;
            DateTimeCur = DateTimeCur.AddSeconds(Time.deltaTime);
            TimerText.text = DateTimeCur.ToString("yy-MM-dd HH:mm:ss");
            
            if (RealTimePassed > TimeBorder &&!Y2KStage)
            {
                BugChase.Instance.ToY2K();
                Y2KStage = true;
            }
            else if (RealTimePassed > (TimeBorder +5) && !Y2KTip){
                DialogBox.Show(new DialogInfo($"咦？{BugChase.Instance.BugName}的样子……"));
                Y2KTip = true;
            }
            else if (RealTimePassed > (TimeBorder +DeathBorder) && !Death)
            {
                BugChase.Instance.Death();
                Death = true;
            }
            
            
        }
    }
}