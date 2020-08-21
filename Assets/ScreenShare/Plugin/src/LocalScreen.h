#pragma once

#include "Unity/IUnityInterface.h"
#include "Unity/IUnityGraphicsD3D11.h"
#include "parsec.h"

#include <wrl.h>

#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.System.h>
#include <winrt/Windows.UI.h>
#include <winrt/Windows.UI.Composition.h>
#include <winrt/Windows.UI.Composition.Desktop.h>
#include <winrt/Windows.UI.Popups.h>
#include <winrt/Windows.Graphics.Capture.h>
#include <winrt/Windows.Graphics.DirectX.h>
#include <winrt/Windows.Graphics.DirectX.Direct3d11.h>

#include <windows.ui.composition.interop.h>

using namespace winrt::Windows::Graphics;
using namespace winrt::Windows::Graphics::Capture;
using namespace winrt::Windows::Graphics::DirectX;
using namespace winrt::Windows::Graphics::DirectX::Direct3D11;

class LocalScreen
{
public:
    LocalScreen(IUnityInterfaces *unity, char *sessionId, HWND hWnd, HMONITOR hMon);
    ~LocalScreen();

    void OnFrameArrived(
        Direct3D11CaptureFramePool const &pool,
        winrt::Windows::Foundation::IInspectable const &args);
    void Render();

private:
    void InitGraphicsCapture();
    void InitD3D11();
    void RenderD3D11(Microsoft::WRL::ComPtr<ID3D11Texture2D> texture);
    void RenderParsec(Microsoft::WRL::ComPtr<ID3D11Texture2D> texture);

    Parsec *m_parsec{nullptr};
    char *m_sessionId{nullptr};
    HWND m_hWnd{nullptr};
    HMONITOR m_hMon{nullptr};
    bool m_graphicsInitialized{false};

    ID3D11Device *m_device{nullptr};
    ID3D11DeviceContext *m_deviceContext{nullptr};
    ID3D11VertexShader *m_VS{nullptr};
    ID3D11PixelShader *m_PS{nullptr};
    ID3D11InputLayout *m_inputLayout{nullptr};
    ID3D11Buffer *m_vBuf{nullptr};

    IDirect3DDevice m_winRTDevice{nullptr};
    Direct3D11CaptureFramePool m_framePool{nullptr};
    GraphicsCaptureSession m_session{nullptr};
    GraphicsCaptureItem m_item{nullptr};
};