using Microsoft.Windows.Mme;
using Microsoft.Windows.MmSysCom;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.WinMM {
	public class WaveOutDevice : IAudioDevice {
		internal WaveOutDevice(uint index) {
			Index = index;
			MmSysComExports.MMR(MmeExports.waveOutGetDevCapsW(index, out Caps, (uint)Marshal.SizeOf(Caps)));
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) { }

		internal readonly uint Index;
		internal readonly WAVEOUTCAPSW Caps;

		/// <summary>
		/// The friendly name of the device.
		/// </summary>
		/// <remarks>Due to technical reason, this field is truncated if it has more than 31 characters.</remarks>
		public string Name => Caps.szPname;

		public DataFlow DataFlow => DataFlow.Out;

		public AudioClient Connect() {
			return new WaveOutClient(this);
		}
	}
}
