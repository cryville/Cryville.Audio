using System;
using System.Globalization;

namespace Cryville.Audio {
	/// <summary>
	/// Provides a set of <see langword="static" /> methods related to <see cref="AudioStream" />.
	/// </summary>
	public static class AudioStreamHelpers {
		/// <summary>
		/// Gets a recommended new buffer size, provided the current buffer size and the target buffer size.
		/// </summary>
		/// <param name="targetBufferSize">The requested minimum buffer size in frames.</param>
		/// <param name="currentBufferSize">The current buffer size in frames.</param>
		/// <returns>The new buffer size in frames.</returns>
		public static int GetNewBufferSize(int targetBufferSize, int currentBufferSize) {
			if (targetBufferSize <= 0)
				targetBufferSize = 256;
			if (targetBufferSize <= currentBufferSize)
				return currentBufferSize;
			currentBufferSize *= 2;
			if (targetBufferSize > currentBufferSize)
				currentBufferSize = targetBufferSize;
			return currentBufferSize;
		}
		/// <summary>
		/// Throws an exception indicating an unexpected audio client status.
		/// </summary>
		/// <param name="unexpectedStatus">The unexpected audio client status.</param>
		/// <param name="messagePrefix">The prefix of the exception message.</param>
		/// <exception cref="AudioClientDisconnectedException"><paramref name="unexpectedStatus" /> is <see cref="AudioClientStatus.Disconnected" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="unexpectedStatus" /> is not <see cref="AudioClientStatus.Disconnected" />.</exception>
		public static void ThrowStatusException(AudioClientStatus unexpectedStatus, string messagePrefix = "Unexpected status") {
			string message = string.Format(CultureInfo.InvariantCulture, "{0}: {1}.", messagePrefix, unexpectedStatus);
			if (unexpectedStatus == AudioClientStatus.Disconnected)
				throw new AudioClientDisconnectedException(message);
			throw new InvalidOperationException(message);
		}
	}
}
