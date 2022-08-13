#include <mmdeviceapi.h>

const CLSID CLSID_MMDeviceEnumerator = __uuidof(MMDeviceEnumerator);
const IID IID_IMMDeviceEnumerator = __uuidof(IMMDeviceEnumerator);

#pragma region IMMDevice
extern "C" __declspec(dllexport) HRESULT IMMDevice_Activate(IMMDevice * self, REFIID iid, DWORD dwClsCtx, PROPVARIANT * pActivationParams, void** ppInterface) {
	return self->Activate(iid, dwClsCtx, pActivationParams, ppInterface);
}
extern "C" __declspec(dllexport) HRESULT IMMDevice_OpenPropertyStore(IMMDevice* self, DWORD stgmAccess, IPropertyStore** ppProperties) {
	return self->OpenPropertyStore(stgmAccess, ppProperties);
}
extern "C" __declspec(dllexport) HRESULT IMMDevice_GetId(IMMDevice* self, LPWSTR* ppstrId) {
	return self->GetId(ppstrId);
}
extern "C" __declspec(dllexport) HRESULT IMMDevice_GetState(IMMDevice* self, DWORD* pdwState) {
	return self->GetState(pdwState);
}
#pragma endregion

#pragma region IMMDeviceCollection
extern "C" __declspec(dllexport) HRESULT IMMDeviceCollection_GetCount(IMMDeviceCollection* self, UINT* pcDevices) {
	return self->GetCount(pcDevices);
}
extern "C" __declspec(dllexport) HRESULT IMMDeviceCollection_Item(IMMDeviceCollection* self, UINT nDevice, IMMDevice** ppDevice) {
	return self->Item(nDevice, ppDevice);
}
#pragma endregion

#pragma region IMMDeviceEnumerator
extern "C" __declspec(dllexport) HRESULT _ctor_IMMDeviceEnumerator(IMMDeviceEnumerator** out) {
    return CoCreateInstance(CLSID_MMDeviceEnumerator, NULL, CLSCTX_ALL, IID_IMMDeviceEnumerator, (void**)out);
}
extern "C" __declspec(dllexport) HRESULT IMMDeviceEnumerator_EnumAudioEndpoints(IMMDeviceEnumerator* self, EDataFlow dataFlow, DWORD dwStateMask, IMMDeviceCollection** ppDevices) {
	return self->EnumAudioEndpoints(dataFlow, dwStateMask, ppDevices);
}
extern "C" __declspec(dllexport) HRESULT IMMDeviceEnumerator_GetDefaultAudioEndpoint(IMMDeviceEnumerator* self, EDataFlow dataFlow, ERole role, IMMDevice** ppEndpoint) {
	return self->GetDefaultAudioEndpoint(dataFlow, role, ppEndpoint);
}
extern "C" __declspec(dllexport) HRESULT IMMDeviceEnumerator_GetDevice(IMMDeviceEnumerator* self, LPCWSTR pwstrId, IMMDevice** ppDevice) {
	return self->GetDevice(pwstrId, ppDevice);
}
extern "C" __declspec(dllexport) HRESULT IMMDeviceEnumerator_RegisterEndpointNotificationCallback(IMMDeviceEnumerator* self, IMMNotificationClient* pClient) {
	return self->RegisterEndpointNotificationCallback(pClient);
}
extern "C" __declspec(dllexport) HRESULT IMMDeviceEnumerator_UnregisterEndpointNotificationCallback(IMMDeviceEnumerator* self, IMMNotificationClient* pClient) {
	return self->UnregisterEndpointNotificationCallback(pClient);
}
#pragma endregion

#pragma region IMMEndpoint
extern "C" __declspec(dllexport) HRESULT IMMEndpoint_GetDataFlow(IMMEndpoint* self, EDataFlow* pDataFlow) {
	return self->GetDataFlow(pDataFlow);
}
#pragma endregion
