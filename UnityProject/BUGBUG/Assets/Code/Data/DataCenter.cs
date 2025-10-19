using Code.Utils;
using UnityEngine;

namespace Code.Data
{
    public class DataCenter : SingletonMonoBehaviour<DataCenter>
    {
        //singleton
        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            m_Timer = FindObjectOfType<Timer>();
        }
        
        //DataZone
        [SerializeField]
        private Timer m_Timer;
        
        
    }
}