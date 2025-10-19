using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeWindowState(bool setPet = true)
    {
        Debug.Log(setPet + "ChangeWindowState");
#if !UNITY_EDITOR
        
        WinTrans.Instance.transparent = setPet;
        WinTrans.Instance.clickThrough = setPet;
        WinTrans.Instance.glass = setPet;
        WinTrans.Instance.topMost = setPet;
        WinTrans.Instance.Apply();
#endif
    }
}
