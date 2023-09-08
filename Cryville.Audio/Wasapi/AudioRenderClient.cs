using Cryville.Common.Platform.Windows;
using Microsoft.Windows.AudioClient;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Wasapi {
	internal sealed class AudioRenderClient : ComInterfaceWrapper {
		internal AudioRenderClient(IntPtr obj) : base(obj) { }
		public void FillBuffer(byte[] buffer, uint frames, uint length) {
			IAudioRenderClient.GetBuffer(ComObject, frames, out var result);
			Marshal.Copy(buffer, 0, result, (int)length);
		}
		public void SilentBuffer(uint frames) {
			IAudioRenderClient.GetBuffer(ComObject, frames, out _);
			IAudioRenderClient.ReleaseBuffer(ComObject, frames, (uint)AUDCLNT_BUFFERFLAGS.SILENT);
		}
		public void ReleaseBuffer(uint frames) {
			IAudioRenderClient.ReleaseBuffer(ComObject, frames, 0);
		}
	}
}
