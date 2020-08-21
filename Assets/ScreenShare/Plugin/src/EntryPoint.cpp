#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityInterface.h"

#include "RemoteScreenD3D11.h"
#include "LocalScreen.h"

#include <unordered_map>
#include <unordered_set>
#include <iostream>

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces *s_UnityInterfaces = nullptr;
static IUnityGraphics *s_Graphics = nullptr;
static std::unordered_map<int, RemoteScreen *> s_eventToRemoteScreen;
static std::unordered_map<RemoteScreen *, int> s_remoteScreenToEvent;
static int s_maxRemoteScreenId = 0;
static std::unordered_map<int, LocalScreen *> s_eventToLocalScreen;
static std::unordered_map<LocalScreen *, int> s_localScreenToEvent;
static int s_maxLocalScreenId = 0;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces *unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	// TODO ?
}

static void UNITY_INTERFACE_API OnRemoteScreenRenderEvent(int eventId)
{
	s_eventToRemoteScreen[eventId]->Render();
}

static void UNITY_INTERFACE_API OnLocalScreenRenderEvent(int eventId) {
	s_eventToLocalScreen[eventId]->Render();
}

// REMOTE SCREEN PUBLIC INTERFACE:

extern "C" UNITY_INTERFACE_EXPORT void *UNITY_INTERFACE_API NewRemoteScreen(
	char *sessionId, char *peerId, uint32_t width, uint32_t height)
{
	std::cerr << "[ScreenShare] new remote screen" << std::endl;
	auto rs = new RemoteScreen(s_UnityInterfaces, sessionId, peerId, width, height);
	s_eventToRemoteScreen[++s_maxRemoteScreenId] = rs;
	s_remoteScreenToEvent[rs] = s_maxRemoteScreenId;

	return (void *)rs;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API DestroyRemoteScreen(void *rs)
{
	std::cerr << "[ScreenShare] del remote screen" << std::endl;
	if (rs == nullptr)
		return;
	auto remoteScreen = (RemoteScreen *)rs;
	s_eventToRemoteScreen.erase(s_remoteScreenToEvent[remoteScreen]);
	s_remoteScreenToEvent.erase(remoteScreen);
	delete remoteScreen;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ShutdownRemoteScreen(void *rs)
{
	std::cerr << "[ScreenShare] shutdown remote screen" << std::endl;
	if (rs == nullptr)
		return;
	((RemoteScreen *)rs)->Shutdown();
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRemoteScreenRenderEventFunc()
{
	return OnRemoteScreenRenderEvent;
}

extern "C" UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API RemoteScreenToEvent(void *rs)
{
	auto remoteScreen = (RemoteScreen *)rs;
	return s_remoteScreenToEvent[remoteScreen];
}

// LOCAL SCREEN PUBLIC INTERFACE :

extern "C" UNITY_INTERFACE_EXPORT void *UNITY_INTERFACE_API NewLocalScreen(char *sessionId, HWND hWnd, HMONITOR hMon)
{
	std::cerr << "[ScreenShare] new local screen" << std::endl;
	auto ls = new LocalScreen(s_UnityInterfaces, sessionId, hWnd, hMon);
	s_eventToLocalScreen[++s_maxLocalScreenId] = ls;
	s_localScreenToEvent[ls] = s_maxLocalScreenId;
	
	return (void *)ls;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API DestroyLocalScreen(void *ls)
{
	std::cerr << "[ScreenShare] del local screen" << std::endl;
	if (ls == nullptr)
		return;	
	auto localScreen = (LocalScreen *)ls;
	s_eventToLocalScreen.erase(s_localScreenToEvent[localScreen]);
	s_localScreenToEvent.erase(localScreen);
	delete localScreen;
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetLocalScreenRenderEventFunc()
{
	return OnLocalScreenRenderEvent;
}

extern "C" UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API LocalScreenToEvent(void *ls)
{
	auto localScreen = (LocalScreen *)ls;
	return s_localScreenToEvent[localScreen];
}
