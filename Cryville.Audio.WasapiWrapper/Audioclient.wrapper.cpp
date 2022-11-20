#include <Audioclient.h>

#pragma region IAudioClient
extern "C" __declspec(dllexport) HRESULT IAudioClient_Initialize(IAudioClient* self, AUDCLNT_SHAREMODE ShareMode, DWORD StreamFlags, REFERENCE_TIME hnsBufferDuration, REFERENCE_TIME hnsPeriodicity, const WAVEFORMATEX* pFormat, LPCGUID AudioSessionGuid) {
	return self->Initialize(ShareMode, StreamFlags, hnsBufferDuration, hnsPeriodicity, pFormat, AudioSessionGuid);
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_GetBufferSize(IAudioClient* self, UINT32* pNumBufferFrames) {
	return self->GetBufferSize(pNumBufferFrames);
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_GetStreamLatency(IAudioClient* self, REFERENCE_TIME* phnsLatency) {
	return self->GetStreamLatency(phnsLatency);
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_GetCurrentPadding(IAudioClient* self, UINT32* pNumPaddingFrames) {
	return self->GetCurrentPadding(pNumPaddingFrames);
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_IsFormatSupported(IAudioClient* self, AUDCLNT_SHAREMODE ShareMode, const WAVEFORMATEX* pFormat, WAVEFORMATEX** ppClosestMatch) {
	return self->IsFormatSupported(ShareMode, pFormat, ppClosestMatch);
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_GetMixFormat(IAudioClient* self, WAVEFORMATEX** ppDeviceFormat) {
	return self->GetMixFormat(ppDeviceFormat);
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_GetDevicePeriod(IAudioClient* self, REFERENCE_TIME* phnsDefaultDevicePeriod, REFERENCE_TIME* phnsMinimumDevicePeriod) {
	return self->GetDevicePeriod(phnsDefaultDevicePeriod, phnsMinimumDevicePeriod);
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_Start(IAudioClient* self) {
	return self->Start();
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_Stop(IAudioClient* self) {
	return self->Stop();
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_Reset(IAudioClient* self) {
	return self->Reset();
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_SetEventHandle(IAudioClient* self, HANDLE eventHandle) {
	return self->SetEventHandle(eventHandle);
}
extern "C" __declspec(dllexport) HRESULT IAudioClient_GetService(IAudioClient* self, REFIID riid, void** ppv) {
	return self->GetService(riid, ppv);
}
#pragma endregion

#pragma region IAudioClock
extern "C" __declspec(dllexport) HRESULT IAudioClock_GetFrequency(IAudioClock* self, UINT64* pu64Frequency) {
	return self->GetFrequency(pu64Frequency);
}
extern "C" __declspec(dllexport) HRESULT IAudioClock_GetPosition(IAudioClock* self, UINT64* pu64Position, UINT64* pu64QPCPosition) {
	return self->GetPosition(pu64Position, pu64QPCPosition);
}
extern "C" __declspec(dllexport) HRESULT IAudioClock_GetCharacteristics(IAudioClock* self, DWORD* pdwCharacteristics) {
	return self->GetCharacteristics(pdwCharacteristics);
}
#pragma endregion

#pragma region IAudioRenderClient
extern "C" __declspec(dllexport) HRESULT IAudioRenderClient_GetBuffer(IAudioRenderClient* self, UINT32 NumFramesRequested, BYTE** ppData) {
	return self->GetBuffer(NumFramesRequested, ppData);
}
extern "C" __declspec(dllexport) HRESULT IAudioRenderClient_ReleaseBuffer(IAudioRenderClient* self, UINT32 NumFramesWritten, DWORD dwFlags) {
	return self->ReleaseBuffer(NumFramesWritten, dwFlags);
}
#pragma endregion
