using Cryville.Common.Platform.Windows;
using Microsoft.Windows.AudioClient;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Wasapi {
	internal class AudioRenderClient : ComInterfaceWrapper<IAudioRenderClient> {
		internal AudioRenderClient(IAudioRenderClient obj) : base(obj) { }
		public void FillBuffer(byte[] buffer, uint frames, uint length) {
			ComObject.GetBuffer(frames, out var result);
			Marshal.Copy(buffer, 0, result, (int)length);
		}
		public void SilentBuffer(uint frames) {
			ComObject.GetBuffer(frames, out _);
			ComObject.ReleaseBuffer(frames, (uint)AUDCLNT_BUFFERFLAGS.SILENT);
		}
		public void ReleaseBuffer(uint frames) {
			ComObject.ReleaseBuffer(frames, 0);
		}
	}
}
