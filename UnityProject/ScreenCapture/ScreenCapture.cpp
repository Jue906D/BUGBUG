// DesktopCapture.cpp
#include <windows.h>
#include <d3d11.h>
#include <dxgi1_2.h>
#include <atomic>

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")

// ---------- 全局对象 ----------
static ID3D11Device* g_pDevice = nullptr;
static ID3D11DeviceContext* g_pContext = nullptr;
static IDXGIOutputDuplication* g_pDup = nullptr;
static ID3D11Texture2D* g_pSharedTex = nullptr;   // 共享纹理
static HANDLE                     g_hShared = nullptr;   // 跨进程句柄
static std::atomic<bool>        g_ready{ false };

// ---------- 导出：初始化 ----------
extern "C" __declspec(dllexport) bool __stdcall InitCapture()
{
    if (g_ready) return true;

    // 1. 创建设备
    D3D_FEATURE_LEVEL lv;
    HRESULT hr = D3D11CreateDevice(
        nullptr,
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        D3D11_CREATE_DEVICE_SINGLETHREADED |
        D3D11_CREATE_DEVICE_BGRA_SUPPORT,
        nullptr, 
        0,
        D3D11_SDK_VERSION, &g_pDevice, &lv, &g_pContext);
    if (FAILED(hr)) return false;

    // 2. DXGI 路径拿到 IDXGIOutput1
    IDXGIDevice* pDXGIDev = nullptr;
    IDXGIAdapter* pAdapter = nullptr;
    IDXGIOutput* pOutput = nullptr;
    IDXGIOutput1* pOutput1 = nullptr;

    hr = g_pDevice->QueryInterface(__uuidof(IDXGIDevice), (void**)&pDXGIDev);
    if (SUCCEEDED(hr)) hr = pDXGIDev->GetParent(__uuidof(IDXGIAdapter), (void**)&pAdapter);
    if (SUCCEEDED(hr)) hr = pAdapter->EnumOutputs(0, &pOutput);          // 0 = 主屏
    if (SUCCEEDED(hr)) hr = pOutput->QueryInterface(__uuidof(IDXGIOutput1), (void**)&pOutput1);
    if (SUCCEEDED(hr)) hr = pOutput1->DuplicateOutput(g_pDevice, &g_pDup);

    SAFE_RELEASE(pDXGIDev);
    SAFE_RELEASE(pAdapter);
    SAFE_RELEASE(pOutput);
    SAFE_RELEASE(pOutput1);

    if (FAILED(hr)) { ReleaseCapture(); return false; }

    g_ready = true;
    return true;
}

// ---------- 导出：释放 ----------
extern "C" __declspec(dllexport) void __stdcall ReleaseScreenCapture()
{
    g_ready = false;
    SAFE_RELEASE(g_pSharedTex);
    SAFE_RELEASE(g_pDup);
    SAFE_RELEASE(g_pContext);
    SAFE_RELEASE(g_pDevice);
    g_hShared = nullptr;
}

// ---------- 导出：抓取一帧 ----------
extern "C" __declspec(dllexport) bool __stdcall AcquireFrame()
{
    if (!g_ready) return false;

    IDXGIResource* pRes = nullptr;
    DXGI_OUTDUPL_FRAME_INFO info{};
    HRESULT hr = g_pDup->AcquireNextFrame(0, &info, &pRes);
    if (hr == DXGI_ERROR_WAIT_TIMEOUT) return true;   // 无新帧
    if (FAILED(hr)) return false;

    // 第一次：创建共享纹理
    if (!g_pSharedTex)
    {
        ID3D11Texture2D* pAcqTex = nullptr;
        hr = pRes->QueryInterface(__uuidof(ID3D11Texture2D), (void**)&pAcqTex);
        if (SUCCEEDED(hr))
        {
            D3D11_TEXTURE2D_DESC desc{};
            pAcqTex->GetDesc(&desc);
            desc.BindFlags = D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET;
            desc.MiscFlags = D3D11_RESOURCE_MISC_SHARED;   // 关键：可共享
            hr = g_pDevice->CreateTexture2D(&desc, nullptr, &g_pSharedTex);
            if (SUCCEEDED(hr))
            {
                IDXGIResource* pSharedRes = nullptr;
                g_pSharedTex->QueryInterface(__uuidof(IDXGIResource), (void**)&pSharedRes);
                pSharedRes->GetSharedHandle(&g_hShared);
                pSharedRes->Release();
            }
            pAcqTex->Release();
        }
    }

    // 拷贝到共享纹理
    if (g_pSharedTex)
        g_pContext->CopyResource(g_pSharedTex, pRes);

    pRes->Release();
    g_pDup->ReleaseFrame();
    return true;
}

// ---------- 导出：拿到共享句柄 ----------
extern "C" __declspec(dllexport) HANDLE __stdcall GetSharedHandle()
{
    return g_hShared;
}

// 辅助宏
template<typename T> void SAFE_RELEASE(T*& p) { if (p) { p->Release(); p = nullptr; } }


//// dllmain.cpp : 定义 DLL 应用程序的入口点。
//#include "pch.h"
//
//BOOL APIENTRY DllMain( HMODULE hModule,
//                       DWORD  ul_reason_for_call,
//                       LPVOID lpReserved
//                     )
//{
//    switch (ul_reason_for_call)
//    {
//    case DLL_PROCESS_ATTACH:
//    case DLL_THREAD_ATTACH:
//    case DLL_THREAD_DETACH:
//    case DLL_PROCESS_DETACH:
//        break;
//    }
//    return TRUE;
//}