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
		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) => dataFlow switch {
			DataFlow.Out => new WaveOutDevice(MmeExports.WAVE_MAPPER),
			DataFlow.In => throw new NotImplementedException(),
			_ => throw new NotImplementedException(),
		};

		/// <inheritdoc />
		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) => dataFlow switch {
			DataFlow.Out => new WaveOutDeviceCollection(),
			DataFlow.In => throw new NotImplementedException(),
			_ => throw new NotSupportedException(),
		};

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

			public Enumerator GetEnumerator() => new(this);
			IEnumerator<IAudioDevice> IEnumerable<IAudioDevice>.GetEnumerator() => GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public struct Enumerator(WaveOutDeviceCollection obj) : IEnumerator<IAudioDevice> {
				int _index = -1;

				public readonly IAudioDevice Current => obj[_index];
				readonly object IEnumerator.Current => Current;

				public readonly void Dispose() { }

				public bool MoveNext() => ++_index < obj.Count;

				public void Reset() => _index = -1;
			}
		}
	}
}
