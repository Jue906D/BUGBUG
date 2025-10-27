using UnityEngine;

namespace Code.BUG
{
    public class ResetRotOnAnimEnd : MonoBehaviour
    {
        public void ResetRotation()
        {
            transform.rotation = Quaternion.identity; 
        }
    }
}