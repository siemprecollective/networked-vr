#include "RemoteScreenD3D11.h"

#include <chrono>
#include <iostream>

RemoteScreen::RemoteScreen(
    IUnityInterfaces *unity, 
    char *sessionId, char *peerId, uint32_t width, uint32_t height)
{
    m_device = unity->Get<IUnityGraphicsD3D11>()->GetDevice();
    ParsecInit(PARSEC_VER, nullptr, nullptr, &m_parsec);

    m_sessionId = std::string(sessionId);
    m_peerId = std::string(peerId);
    m_width = width;
    m_height = height;
    m_running = true;
    connect_thread = std::thread(&RemoteScreen::ParsecConnect, this);
}

RemoteScreen::~RemoteScreen()
{
    if (connect_thread.joinable())
    {
        connect_thread.join();
    }
    ParsecClientDisconnect(m_parsec);
    ParsecDestroy(m_parsec);
};

void RemoteScreen::Shutdown()
{
    m_running = false;
}

void RemoteScreen::Render()
{
    ID3D11DeviceContext *ctx;
    m_device->GetImmediateContext(&ctx);

    if (!m_parsecGuard.try_lock())
        return;
    ParsecClientD3D11RenderFrame(m_parsec,
                                 m_device, ctx, nullptr, nullptr, nullptr, 0);
    m_parsecGuard.unlock();

    ctx->Release();
}

void RemoteScreen::ParsecConnect()
{
    ParsecStatus status;
    ParsecClientStatus clientStatus;
    while (m_running)
    {
        std::chrono::system_clock::duration elapsed;
        {
            if (!m_parsecGuard.try_lock())
                continue;
            auto startTime = std::chrono::system_clock::now();
            if ((status = ParsecClientGetStatus(m_parsec, &clientStatus)) != PARSEC_OK || clientStatus.networkFailure)
            {
                std::cerr << "NATIVE session " << status << " " << clientStatus.networkFailure << " " << m_sessionId << " " << m_peerId << std::endl;
                ParsecClientDisconnect(m_parsec);
                status = ParsecClientConnect(m_parsec, nullptr, (char *)m_sessionId.c_str(), (char *)m_peerId.c_str());
                std::cerr << "NATIVE connecting " << status << std::endl;
                status = ParsecClientSetDimensions(m_parsec, m_width, m_height, 1.0f);
                std::cerr << "NATIVE set dims " << status << std::endl;
            }
            elapsed = std::chrono::system_clock::now() - startTime;
            m_parsecGuard.unlock();
        }
        auto to_sleep =
            elapsed > std::chrono::seconds(1) ? std::chrono::seconds(0) : std::chrono::seconds(1) - elapsed;
        std::this_thread::sleep_for(to_sleep);
    }
}
