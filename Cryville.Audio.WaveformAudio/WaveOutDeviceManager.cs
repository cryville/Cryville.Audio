using Microsoft.Windows.Mme;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cryville.Audio.WaveformAudio {
	/// <summary>
	/// An <see cref="IAudioDeviceManager" /> that interacts with WinMM.
	/// </summary>
	public class WaveDeviceManager : IAudioDeviceManager {
		/// <summary>
		/// Creates an instance of the <see cref="WaveDeviceManager" /> class.
		/// </summary>
		public WaveDeviceManager() {
			if (MmeExports.waveOutGetNumDevs() == 0)
				throw new NotSupportedException();
		}

		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Whether the method is being called by user.</param>
		protected virtual void Dispose(bool disposing) { }

		/// <inheritdoc />
		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) {
			switch (dataFlow) {
				case DataFlow.Out: return new WaveOutDevice(MmeExports.WAVE_MAPPER);
				case DataFlow.In: throw new NotImplementedException();
				default: throw new NotImplementedException();
			}
		}

		/// <inheritdoc />
		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) {
			switch (dataFlow) {
				case DataFlow.Out: return new WaveOutDeviceCollection();
				case DataFlow.In: throw new NotImplementedException();
				default: throw new NotSupportedException();
			}
		}

		internal sealed class WaveOutDeviceCollection : IEnumerable<IAudioDevice> {
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

			public Enumerator GetEnumerator() => new Enumerator(this);

			IEnumerator<IAudioDevice> IEnumerable<IAudioDevice>.GetEnumerator() => GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public struct Enumerator : IEnumerator<IAudioDevice> {
				readonly WaveOutDeviceCollection _obj;
				int _index;

				public Enumerator(WaveOutDeviceCollection obj) {
					_obj = obj;
					_index = -1;
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
