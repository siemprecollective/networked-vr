#include "LocalScreen.h"

#include <windows.graphics.capture.h>
#include <windows.graphics.capture.interop.h>
#include <windows.graphics.directx.direct3d11.interop.h>

#include <d3d11.h>
#include <dxgi1_2.h>
#include <d3dcompiler.h>

#include <iostream>

struct VERTEX
{
    FLOAT X, Y, Z;
    FLOAT U, V;
};

const char *shader = R"""(
Texture2D ScreenTexture;
SamplerState ScreenTextureSampler {
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VOut
{
    float4 position : SV_POSITION;
    float2 texCoord : TEXCOORD; 
};

VOut VShader(float4 position : POSITION, float2 texCoord : TEXCOORD)
{
    VOut output;

    output.position = position;
    output.texCoord = texCoord;

    return output;
}

float4 PShader(float4 position : SV_POSITION, float2 texCoord : TEXCOORD) : SV_TARGET
{
    float4 texColor = ScreenTexture.Sample(ScreenTextureSampler, texCoord);
    texColor.a = 1;
    return texColor;
}
)""";

LocalScreen::LocalScreen(IUnityInterfaces *unity, char *sessionId, HWND hWnd, HMONITOR hMon)
{
    // validate args
    if (hWnd == nullptr && hMon == nullptr)
    {
        std::cerr << "[ScreenShare] ERROR no capture item specified" << std::endl;
        throw std::runtime_error("no capture item specified");
    }
    m_hWnd = hWnd;
    m_hMon = hMon;

    // start parsec
    std::cerr << "[ScreenShare] HOST_GAME " << hMon << " " << hWnd << std::endl;
    ParsecInit(PARSEC_VER, nullptr, nullptr, &m_parsec);
    ParsecHostStart(m_parsec, HOST_GAME, nullptr, sessionId);
    
    // get d3d context
    m_device = unity->Get<IUnityGraphicsD3D11>()->GetDevice();
    std::cerr << "[ScreenShare] got device" << std::endl;

    // init windows.graphics.capture
    InitGraphicsCapture();

    // init our own rendering
    InitD3D11();
}

LocalScreen::~LocalScreen()
{
    std::cerr << "[ScreenShare] destroying session" << std::endl;
    if (m_session) m_session.Close();
    std::cerr << "[ScreenShare] destroying framepool" << std::endl;
    if (m_framePool) m_framePool.Close();
    std::cerr << "[ScreenShare] destroying device context" << std::endl;
    if (m_deviceContext) m_deviceContext->Release();
    std::cerr << "[ScreenShare] destroying device" << std::endl;
    if (m_device) m_device->Release();
    if (m_parsec)
    {
        std::cerr << "[ScreenShare] destroying parsec" << std::endl;
        ParsecHostStop(m_parsec);
        ParsecDestroy(m_parsec);
    }
    std::cerr << "[ScreenShare] done destroying localscreen" << std::endl;
}

void LocalScreen::Render()
{
    if (!m_graphicsInitialized)
    {
        m_graphicsInitialized = true;
    }

    // get frame from pool
    auto frame = m_framePool.TryGetNextFrame();
    if (!frame)
        return;

    // get an ID3D11Texture2D out of the frame
    Microsoft::WRL::ComPtr<IDXGISurface2> dxgiSurface;
    winrt::check_hresult(frame.Surface()
                             .as<Windows::Graphics::DirectX::Direct3D11::IDirect3DDxgiInterfaceAccess>()
                             ->GetInterface(IID_PPV_ARGS(&dxgiSurface)));
    Microsoft::WRL::ComPtr<ID3D11Texture2D> texture;
    UINT32 subresourceIndex = 0;
    winrt::check_hresult(dxgiSurface->GetResource(IID_PPV_ARGS(&texture), &subresourceIndex));

    RenderD3D11(texture);
    RenderParsec(texture);

    frame.Close();
}

inline auto CreateCaptureItemForMonitor(HMONITOR hmon)
{
    auto interop_factory = winrt::get_activation_factory<winrt::Windows::Graphics::Capture::GraphicsCaptureItem, IGraphicsCaptureItemInterop>();
    winrt::Windows::Graphics::Capture::GraphicsCaptureItem item = {nullptr};
    winrt::check_hresult(interop_factory->CreateForMonitor(hmon, winrt::guid_of<ABI::Windows::Graphics::Capture::IGraphicsCaptureItem>(), winrt::put_abi(item)));
    return item;
}

inline auto CreateCaptureItemForWindow(HWND hwnd)
{
    auto activation_factory = winrt::get_activation_factory<GraphicsCaptureItem>();
    auto interop_factory = activation_factory.as<IGraphicsCaptureItemInterop>();
    GraphicsCaptureItem item = {nullptr};
    interop_factory->CreateForWindow(hwnd, winrt::guid_of<ABI::Windows::Graphics::Capture::IGraphicsCaptureItem>(), reinterpret_cast<void **>(winrt::put_abi(item)));
    return item;
}

void LocalScreen::InitGraphicsCapture()
{
    // get capture item
    if (m_hWnd != nullptr)
    {
        m_item = CreateCaptureItemForWindow(m_hWnd);
        std::cerr << "[ScreenShare] Created CaptureItem for window " << m_hWnd << std::endl;
    }
    else if (m_hMon != nullptr)
    {
        m_item = CreateCaptureItemForMonitor(m_hMon);
        std::cerr << "[ScreenShare] Created CaptureItem for monitor " << m_hMon << std::endl;
    }

    std::cerr << "[ScreenShare] setting interface" << std::endl;
    IDXGIDevice *pDXGIDevice;
    // TODO: IID_PPV_ARGS ?
    m_device->QueryInterface(__uuidof(IDXGIDevice), (void **)&pDXGIDevice);
    std::cerr << "[ScreenShare] got DXGIDevice" << std::endl;

    CreateDirect3D11DeviceFromDXGIDevice(pDXGIDevice,
                                         reinterpret_cast<::IInspectable **>(winrt::put_abi(m_winRTDevice)));
    std::cerr << "[ScreenShare] created IDirect3DDevice" << std::endl;

    // create frame pool
    m_framePool = Direct3D11CaptureFramePool::CreateFreeThreaded(
        m_winRTDevice,
        DirectXPixelFormat::B8G8R8A8UIntNormalized,
        2,
        m_item.Size());
    std::cerr << "[ScreenShare] Created CaptureFramePool" << std::endl;

    // create session, register callback, start session
    m_session = m_framePool.CreateCaptureSession(m_item);
    //m_framePool.FrameArrived({this, &LocalScreen::OnFrameArrived});
    std::cerr << "[ScreenShare] Registered frame listener " << std::endl;
    m_session.StartCapture();
    std::cerr << "[ScreenShare] capture started" << std::endl;
    std::cerr << "[ScreenShare] item " << !!(m_item) << m_item.DisplayName().c_str() << std::endl;
}

void LocalScreen::InitD3D11()
{
    // load and compile the two shaders
    ID3DBlob *VS, *PS, *errors;
    HRESULT result;
    result = D3DCompile(shader, strlen(shader), nullptr, nullptr, nullptr, "VShader", "vs_4_0", 0, 0, &VS, &errors);
    if (FAILED(result))
    {
        std::cerr << (char *)errors->GetBufferPointer() << std::endl;
        throw std::runtime_error("unable to compile vertex shader");
    }
    result = D3DCompile(shader, strlen(shader), nullptr, nullptr, nullptr, "PShader", "ps_4_0", 0, 0, &PS, &errors);
    if (FAILED(result))
    {
        std::cerr << (char *)errors->GetBufferPointer() << std::endl;
        throw std::runtime_error("unable to compile pixel shader");
    }
    std::cerr << "[ScreenShare] compiled shaders" << std::endl;

    // encapsulate both shaders into shader objects
    winrt::check_hresult(
        m_device->CreateVertexShader(VS->GetBufferPointer(), VS->GetBufferSize(), NULL, &m_VS));
    winrt::check_hresult(
        m_device->CreatePixelShader(PS->GetBufferPointer(), PS->GetBufferSize(), NULL, &m_PS));
    
    std::cerr << "[ScreenShare] created shaders" << std::endl;

    // create the input layout object
    D3D11_INPUT_ELEMENT_DESC ied[] =
        {
            {"POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0},
            {"TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0},
        };

    winrt::check_hresult(
        m_device->CreateInputLayout(ied, 2, VS->GetBufferPointer(), VS->GetBufferSize(), &m_inputLayout));

    std::cerr << "[ScreenShare] created input layout" << std::endl;
    
    // create the vertex buffer
    D3D11_BUFFER_DESC bd;
    ZeroMemory(&bd, sizeof(bd));

    bd.Usage = D3D11_USAGE_DYNAMIC;             // write access access by CPU and GPU
    bd.ByteWidth = sizeof(VERTEX) * 4;          // size is the VERTEX struct * 4
    bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;    // use as a vertex buffer
    bd.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE; // allow CPU to write in buffer

    winrt::check_hresult(
        m_device->CreateBuffer(&bd, NULL, &m_vBuf)); // create the buffer
    
    std::cerr << "[ScreenShare] created buffer" << std::endl;
}

void LocalScreen::RenderD3D11(Microsoft::WRL::ComPtr<ID3D11Texture2D> texture)
{
    ID3D11DeviceContext *ctx;
    m_device->GetImmediateContext(&ctx);

    // get a resource view for the texture
    ID3D11ShaderResourceView *resourceView;
    winrt::check_hresult(
        m_device->CreateShaderResourceView(texture.Get(), nullptr, &resourceView));
    
    // set the shader objects
    ctx->VSSetShader(m_VS, 0, 0);
    ctx->PSSetShader(m_PS, 0, 0);
    ctx->PSSetShaderResources(0, 1, &resourceView);
    // set input layout
    ctx->IASetInputLayout(m_inputLayout);
    
    // create a triangle using the VERTEX struct
    VERTEX OurVertices[] =
        {
            {-1.0f, -1.0f, 0.0f, 0.0f, 1.0f},
            {-1.0f, 1.0f, 0.0f, 0.0f, 0.0f},
            {1.0f, -1.0f, 0.0f, 1.0f, 1.0f},
            {1.0f, 1.0f, 0.0f, 1.0f, 0.0f}};
    // copy the vertices into the buffer
    D3D11_MAPPED_SUBRESOURCE ms;
    winrt::check_hresult(
        ctx->Map(m_vBuf, NULL, D3D11_MAP_WRITE_DISCARD, NULL, &ms)); // map the buffer
    memcpy(ms.pData, OurVertices, sizeof(OurVertices));              // copy the data
    ctx->Unmap(m_vBuf, NULL);
    
    // select which vertex buffer to display
    UINT stride = sizeof(VERTEX);
    UINT offset = 0;
    ctx->IASetVertexBuffers(0, 1, &m_vBuf, &stride, &offset);
    // select which primtive type we are using
    ctx->IASetPrimitiveTopology(D3D10_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP);
    // draw the vertex buffer to the back buffer
    ctx->Draw(4, 0);

    ctx->Release();
}

void LocalScreen::RenderParsec(Microsoft::WRL::ComPtr<ID3D11Texture2D> texture) {
    // submit frame to parsec
    ID3D11DeviceContext *ctx;
    m_device->GetImmediateContext(&ctx);
    ParsecHostD3D11SubmitFrame(m_parsec, m_device, ctx, texture.Get());
    ctx->Release();
}