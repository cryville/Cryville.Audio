using System;
using System.Runtime.Serialization;

namespace Cryville.Audio {
	/// <summary>
	/// Thrown when an invalid operation is attempted on a disconnected <see cref="AudioClient" />.
	/// </summary>
	[Serializable]
	public class AudioClientDisconnectedException : ObjectDisposedException {
		private const string DEFAULT_MESSAGE = "The audio client has been disconnected.";

		/// <inheritdoc />
		public AudioClientDisconnectedException() : base(DEFAULT_MESSAGE) { }

		/// <inheritdoc />
		public AudioClientDisconnectedException(Exception innerException) : base(DEFAULT_MESSAGE, innerException) { }

		/// <inheritdoc />
		public AudioClientDisconnectedException(string message) : base(message) { }

		/// <inheritdoc />
		public AudioClientDisconnectedException(string message, Exception innerException) : base(message, innerException) { }

		/// <inheritdoc />
		protected AudioClientDisconnectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}