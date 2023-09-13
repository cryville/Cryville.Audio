using System;

namespace Cryville.Audio.AAudio {
	public class AAudioStream : AudioClient {
		readonly AAudioStreamBuilder _builder;
		readonly IntPtr _stream;

		internal AAudioStream(AAudioStreamBuilder builder, IntPtr stream) {
			_builder = builder;
			_stream = stream;
		}

		protected override void Dispose(bool disposing) {
			
		}

		public override IAudioDevice Device => _builder;

		public override WaveFormat Format => WaveFormat.Default;

		public override int BufferSize => 0;

		public override float MaximumLatency => 0;

		public override double Position => throw new NotImplementedException();

		public override double BufferPosition => throw new NotImplementedException();

		public override void Start() {
			base.Start();
			throw new NotImplementedException();
		}
	}
}
