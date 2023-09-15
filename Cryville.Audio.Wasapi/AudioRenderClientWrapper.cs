using Cryville.Common.Platform.Windows;
using Microsoft.Windows.AudioClient;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Wasapi {
	internal sealed class AudioRenderClientWrapper : ComInterfaceWrapper {
		readonly IAudioRenderClient _internal;
		internal AudioRenderClientWrapper(IAudioRenderClient obj) : base(obj) {
			_internal = obj;
		}
		public void FillBuffer(byte[] buffer, uint frames, uint length) {
			_internal.GetBuffer(frames, out var result);
			Marshal.Copy(buffer, 0, result, (int)length);
		}
		public void SilentBuffer(uint frames) {
			_internal.GetBuffer(frames, out _);
			_internal.ReleaseBuffer(frames, (uint)AUDCLNT_BUFFERFLAGS.SILENT);
		}
		public void ReleaseBuffer(uint frames) {
			_internal.ReleaseBuffer(frames, 0);
		}
	}
}
