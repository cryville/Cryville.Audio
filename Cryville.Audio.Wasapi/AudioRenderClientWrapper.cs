using Microsoft.Windows.AudioClient;
using System;
using System.Runtime.InteropServices;
using UnsafeIL;

namespace Cryville.Audio.Wasapi {
	internal sealed class AudioRenderClientWrapper : IDisposable {
		readonly IAudioRenderClient _internal;
		internal AudioRenderClientWrapper(IAudioRenderClient obj) {
			_internal = obj;
		}
		public void Dispose() {
			Marshal.ReleaseComObject(_internal);
		}
		public unsafe ref byte GetBuffer(uint frames) {
			_internal.GetBuffer(frames, out var result);
			return ref Unsafe.AsRef<byte>((void*)result);
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
