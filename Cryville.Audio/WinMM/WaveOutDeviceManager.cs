﻿using Microsoft.Windows.Mme;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cryville.Audio.WinMM {
	public class WaveDeviceManager : IAudioDeviceManager {
		public WaveDeviceManager() { }

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) { }

		public bool IsSupported
			=> Environment.OSVersion.Platform == PlatformID.Win32NT;

		/// <summary>
		/// Gets the default audio device for the specified <paramref name="dataFlow" />.
		/// </summary>
		/// <param name="dataFlow">The data-flow direction. Only <see cref="DataFlow.Out"/> is supported.</param>
		/// <remarks>The <see cref="IAudioDevice.Name" /> of the returned device will be truncated if too long, also it will not be the real name of the device itself.</remarks>
		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			switch (dataFlow) {
				case DataFlow.Out: return new WaveOutDevice(MmeExports.WAVE_MAPPER);
				case DataFlow.In: throw new NotImplementedException();
				default: throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the default audio device for the specified <paramref name="dataFlow" />.
		/// </summary>
		/// <param name="dataFlow">The data-flow direction. Only <see cref="DataFlow.Out"/> is supported.</param>
		/// <remarks>The <see cref="IAudioDevice.Name" /> of the returned device will be truncated if too long.</remarks>
		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			switch (dataFlow) {
				case DataFlow.Out: return new WaveOutDeviceCollection();
				case DataFlow.In: throw new NotImplementedException();
				default: throw new NotImplementedException();
			}
		}

		internal class WaveOutDeviceCollection : IEnumerable<IAudioDevice> {
			public WaveOutDeviceCollection() {
				m_count = MmeExports.waveOutGetNumDevs();
			}

			private uint m_count;
			public uint Count { get => m_count; private set => m_count = value; }

			public WaveOutDevice this[int index] {
				get {
					if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
					return new WaveOutDevice((uint)index);
				}
			}

			public IEnumerator<IAudioDevice> GetEnumerator() => new Enumerator(this);

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			private class Enumerator : IEnumerator<IAudioDevice> {
				int _index = -1;
				readonly WaveOutDeviceCollection _obj;

				public Enumerator(WaveOutDeviceCollection obj) {
					_obj = obj;
				}

				public IAudioDevice Current => _obj[_index];

				object IEnumerator.Current => Current;

				public void Dispose() { }

				public bool MoveNext() => ++_index < _obj.Count;

				public void Reset() => _index = -1;
			}
		}
	}
}
