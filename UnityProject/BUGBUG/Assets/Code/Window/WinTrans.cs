using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class WinTrans : MonoBehaviour
{
    public static WinTrans Instance{ get;private set;}

    [SerializeField] public GameObject fakeDesktop;
    //扩展-普通风格
    const int GWL_EXSTYLE = -20;
    const int GWL_STYLE    = -16;
    
    //允许修改透明度
    const uint WS_EX_LAYERED = 0x00080000;
    //透明度扩散
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
    MARGINS doglass = new MARGINS
    {
        cxLeftWidth = -1,
        cxRightWidth = -1,
        cyTopHeight = -1,
        cyBottomHeight = -1
    };
    MARGINS noglass = new MARGINS
    {
        cxLeftWidth = 0,
        cxRightWidth = 0,
        cyTopHeight = 0,
        cyBottomHeight = 0
    };
    
    //鼠标消息穿透
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint WS_POPUP          = 0x80000000;
    const uint WS_VISIBLE        = 0x10000000;
    const uint LWA_ALPHA = 0x00000002;
    //保持强顶置
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

    //调试用消息框
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, int type);
    
    //当前进程主窗口句柄
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    
    //获取
    [DllImport("user32")] 
    static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
    
    //更改窗口参数
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    
    //更改窗口尺寸、顺序、刷新
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    
    //玻璃效果扩展
    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    
    [DllImport("user32.dll")] 
    private static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
    
    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

    
    [Header("运行时开关")]
    public bool transparent = false;   // 透明
    public bool glass = false; // 玻璃
    public bool clickThrough = false;  // 穿透
    public bool topMost = true;       // 置顶
    private uint   oldExStyle;
    private IntPtr hWnd;

    private void Awake()
    {
        Instance = this;
        Application.runInBackground = true;          
    }
    
    
    public void Start()
    {
        //MessageBox(new IntPtr(0), " HelloBug","BUG?",0);
        
#if !UNITY_EDITOR

        fakeDesktop.SetActive(false);

        hWnd = GetActiveWindow();
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0,0);
        oldExStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        Apply();
        //玻璃效果 
        //DwmExtendFrameIntoClientArea(hWnd, ref margins);
        //允许透明和鼠标穿透
        //SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        //置顶并刷新
        //SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0,0);
#endif
    }
    
    public void Apply()
    {
#if !UNITY_EDITOR
        uint ex = oldExStyle;

        if (transparent)  ex |=  WS_EX_LAYERED;
        else              ex &= ~WS_EX_LAYERED;

        if (clickThrough) ex |=  WS_EX_TRANSPARENT;
        else              ex &= ~WS_EX_TRANSPARENT;

        if(glass) 
            DwmExtendFrameIntoClientArea(hWnd, ref doglass);
        else
            DwmExtendFrameIntoClientArea(hWnd, ref noglass);
        
        SetWindowLong(hWnd, GWL_EXSTYLE, ex);
        
        SetWindowPos(hWnd, topMost?HWND_TOPMOST:HWND_NOTOPMOST, 0, 0, 0, 0,0);
        
        
#endif
        
        //Resolution desk = Screen.currentResolution;
        //MoveWindow(hWnd, 0, 0, desk.width, desk.height, true);
        // 透明通道
        //byte alpha = (byte)(transparent ? 0 : 255);
        //SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);

        // 置顶/取消置顶
        //SetWindowPos(hWnd, topMost ? HWND_TOPMOST : HWND_NOTOPMOST,
        //    0, 0, 0, 0, 0x0020 | 0x0001 | 0x0002); // SWP_FRAMECHANGED|NOSIZE|NOMOVE
    }
    
}
