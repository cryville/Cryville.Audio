using Oboe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAudioClient = Cryville.Audio.AudioClient;
using CAudioStreamBuilder = Oboe.AudioStreamBuilder;

namespace Cryville.Audio.Oboe {
	public class AudioClient : CAudioClient {
		AudioStreamBuilder _builder;
		internal AudioClient(AudioDevice device) {
			m_device = device;
			_builder = device._builder;
		}

		protected override void Dispose(bool disposing) { }

		AudioDevice m_device;
		public override IAudioDevice Device => m_device;

		public override float DefaultBufferDuration => throw new NotSupportedException();

		public override float MinimumBufferDuration => throw new NotSupportedException();

		public override WaveFormat DefaultFormat => throw new NotSupportedException();

		WaveFormat m_waveFormat;
		public override WaveFormat Format => m_waveFormat;

		public override int BufferSize => throw new NotImplementedException();

		public override float MaximumLatency => (float)_builder.Internal.getFramesPerCallback() / _builder.Internal.getSampleRate();

		public override double Position => throw new NotImplementedException();

		public override double BufferPosition => throw new NotImplementedException();

		public override bool IsFormatSupported(WaveFormat format, out WaveFormat? suggestion, AudioShareMode shareMode = AudioShareMode.Shared) {
			throw new NotImplementedException();
		}

		public override void Init(WaveFormat format, float bufferDuration = 0, AudioShareMode shareMode = AudioShareMode.Shared) {
			m_waveFormat = format;
			throw new NotImplementedException();
			/*var ptr = new AudioStreamPtr();
			_builder.Internal
				.setSharingMode(ToInternalSharingMode(shareMode))
				.setFramesPerCallback((int)(bufferDuration  * format.SampleRate))
				.openStream(ptr);*/
		}

		public override void Start() {
			throw new NotImplementedException();
		}

		public override void Pause() {
			throw new NotImplementedException();
		}

		static SharingMode ToInternalSharingMode(AudioShareMode value) {
			switch (value) {
				case AudioShareMode.Shared: return SharingMode.Shared;
				case AudioShareMode.Exclusive: return SharingMode.Exclusive;
				default: throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
	}
}
