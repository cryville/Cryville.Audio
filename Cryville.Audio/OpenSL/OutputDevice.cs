using System;

namespace Cryville.Audio.OpenSL {
	/// <summary>
	/// An <see cref="IAudioDevice" /> that interacts with OpenSL ES.
	/// </summary>
	public class OutputDevice : IAudioDevice {
		Engine _engine;
		internal OutputDevice(Engine engine) {
			_engine = engine;
		}

		/// <inheritdoc />
		~OutputDevice() {
			Dispose(false);
		}

		bool _disposed;
		/// <inheritdoc />
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Whether the method is being called by user.</param>
		protected virtual void Dispose(bool disposing) {
			if (!_disposed) {
				_disposed = true;
			}
		}

		/// <inheritdoc />
		public string Name => "Default";

		/// <inheritdoc />
		public DataFlow DataFlow => DataFlow.Out;

		/// <inheritdoc />
		public AudioClient Connect() => new OutputClient(_engine, this);
	}
}
