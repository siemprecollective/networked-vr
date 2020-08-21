#pragma once
#include "Unity/IUnityGraphicsD3D11.h"
#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityInterface.h"
#include "parsec.h"

#include <d3d11.h>

#include <thread>
#include <string>
#include <mutex>

class RemoteScreen
{
public:
    RemoteScreen(
        IUnityInterfaces *unity, 
        char *sessionId, char *peerId, uint32_t width, uint32_t height);
    ~RemoteScreen();

    void Render();
    void Shutdown();

private:
    Parsec *m_parsec = nullptr;
    std::mutex m_parsecGuard;
    std::string m_sessionId;
    std::string m_peerId;
    int m_height = 0;
    int m_width = 0;
    bool m_running = false;
    std::thread connect_thread;

    ID3D11Device *m_device = nullptr;
    ID3D11Resource *m_outTex = nullptr;

    void ParsecConnect();
};