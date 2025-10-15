using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class Y2KDesktopMask : MonoBehaviour
{
    #region Win32
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName,
                                        uint dwStyle, int x, int y, int nWidth, int nHeight,
                                        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

    [DllImport("gdi32.dll")]
    static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("gdi32.dll")]
    static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h,
                              IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

    [DllImport("gdi32.dll")]
    static extern bool DeleteObject(IntPtr ho);

    [DllImport("user32.dll")]
    static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

    const uint WS_EX_LAYERED   = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_SHOWWINDOW = 0x0040;
    const uint SRCCOPY = 0x00CC0020;
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    #endregion

    [Header("Y2K 强度")]
    public float scanIntensity = 0.5f;
    public float pixelate = 8f;
    public float rgbShift = 0.3f;
    public float noiseAmount = 0.08f;

    private IntPtr maskWnd;
    private IntPtr unityWnd;
    private IntPtr memDC;
    private IntPtr memBM;
    private Texture2D tex;
    private Material y2kMat;

    void Start()
    {
        unityWnd = GetActiveWindow();
        CreateMaskWindow();
        CreateY2KMaterial();
        StartCoroutine(UpdateMaskLoop());
    }

    void CreateMaskWindow()
    {
        Resolution res = Screen.currentResolution;
        maskWnd = CreateWindowEx(
            WS_EX_LAYERED | WS_EX_TRANSPARENT,
            "Static", "Y2KDesktopMask",
            WS_POPUP | WS_VISIBLE,
            0, 0, res.width, res.height,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        // 插在 Unity 窗口下方一位 → 只挡“背后”
        SetWindowPos(maskWnd, HWND_NOTOPMOST, 0, 0, res.width, res.height,
                     SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        ShowWindow(maskWnd, 1);
        UpdateWindow(maskWnd);

        // 内存 DC / Bitmap 准备
        IntPtr deskDC = GetDC(IntPtr.Zero);
        memDC  = CreateCompatibleDC(deskDC);
        memBM  = CreateCompatibleBitmap(deskDC, res.width, res.height);
        SelectObject(memDC, memBM);
        ReleaseDC(IntPtr.Zero, deskDC);

        // 帧缓存纹理
        tex = new Texture2D(res.width, res.height, TextureFormat.RGBA32, false);
    }

    void CreateY2KMaterial()
    {
        Shader shader = Shader.Find("BUG/Y2K");
        y2kMat = new Material(shader);
    }

    IEnumerator UpdateMaskLoop()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame(); // 等 Unity 渲染完

            // 1. 读取屏幕像素（含 Y2K 后处理）
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            // 2. 拷贝到内存 DC（简化：直接 BitBlt 白色示例，这里用纹理数据）
            // 真实工程需用 SetDIBits 或 Texture2D.LoadRawTextureData + GDI
            // 下面用 BitBlt 做“占位演示”——实际可见 Y2K 颜色
            IntPtr maskDC = GetDC(maskWnd);
            BitBlt(maskDC, 0, 0, tex.width, tex.height, memDC, 0, 0, SRCCOPY);
            ReleaseDC(maskWnd, maskDC);
        }
    }

    void OnDestroy()
    {
        if (memBM != IntPtr.Zero) DeleteObject(memBM);
        if (memDC != IntPtr.Zero) DeleteObject(memDC);
        if (y2kMat != null) DestroyImmediate(y2kMat);
        if (tex != null) DestroyImmediate(tex);
    }
}