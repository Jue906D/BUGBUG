using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class DirectorManager : MonoBehaviour
{
    private static DirectorManager _instance;
    public static DirectorManager GetInstance() => _instance; 
    private PlayableDirector director;
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vKey);
    private const int VK_LBUTTON = 0x01;   // 左键
    private const int VK_RBUTTON = 0x02;   // 右键
    private const int MIN_VK = 0x08;   // 退格
    private const int MAX_VK = 0xFE;   // 最后一个有效值
    
    public TextMeshProUGUI cntText;
    void Awake()
    {
        _instance = this;
        director = GetComponent<PlayableDirector>();
        director.stopped += OnStop;
        director.paused  += OnPaused;
    }

    void OnDestroy()
    {
        director.stopped -= OnStop;
        director.paused  -= OnPaused;
    }
    void OnPaused(PlayableDirector d)
    {
        enabled = true;   // 开始轮询按键
    }
    void OnStop(PlayableDirector d)
    {
        enabled = false;
    }

    void Update()
    {
        bool clicked = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0 ||
                       (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;
        bool inputed = Input.anyKey;
        if (clicked)
        {
            //cntText.text = "Resume";
           //director.time = 2.1; 
            director.Resume();
           // director.Play();   // 继续 Timeline
            enabled = false;   // 停止自己，节省性能
        }
        else
        {
            //cntText.text = (Convert.ToInt32(cntText.text) +1).ToString();
        }
    }
}

