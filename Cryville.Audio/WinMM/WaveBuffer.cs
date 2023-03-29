using Microsoft.Windows.Mme;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.WinMM {
	internal class WaveBuffer {
		public WAVEHDR Header;
		GCHandle _ptrheader;

		public readonly byte[] Buffer;
		GCHandle _ptrbuffer;
		public readonly IntPtr BufferPtr;

		public bool Filled;
		
		public WaveBuffer(uint bufferSize) {
			Buffer = new byte[bufferSize];
			_ptrbuffer = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
			BufferPtr = _ptrbuffer.AddrOfPinnedObject();

			Header = new WAVEHDR {
				lpData = BufferPtr,
				dwBufferLength = bufferSize,
			};
			_ptrheader = GCHandle.Alloc(Header, GCHandleType.Pinned);
		}

		public void Release() {
			_ptrheader.Free();
			_ptrbuffer.Free();
		}
	}
}
