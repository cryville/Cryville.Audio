using System;
using System.Globalization;
using System.IO;

namespace Cryville.Audio.Source.Resample {
	/// <summary>
	/// An <see cref="AudioStream" /> that resamples another <see cref="AudioStream" />.
	/// </summary>
	/// <remarks>
	/// <para>This class is able to convert the sample rate and sample format of an audio stream to the specified sample rate and sample format. It is not able to convert the channel count and channel layout.</para>
	/// </remarks>
	public class ResampledAudioSource : AudioStream {
		readonly AudioStream _source;
		readonly bool _highQuality;

		/// <summary>
		/// Creates an instance of the <see cref="ResampledAudioSource" /> class.
		/// </summary>
		/// <param name="source">The source <see cref="AudioStream" />.</param>
		/// <param name="format">The wave format.</param>
		/// <param name="highQuality">Whether to resample with high quality.</param>
		public ResampledAudioSource(AudioStream source, WaveFormat format, bool highQuality = true) : base(format) {
			_source = source ?? throw new ArgumentNullException(nameof(source));
			_highQuality = highQuality;

			if (!IsFormatSupported(Format, source.Format)) throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Resampling to a different channel layout is not supported. Source: {0}. Target: {1}.", source.Format, Format));

			_internal = new(source.Format, Format, _highQuality);
			_source.BufferSize = _internal._inBufferFrameLength;
		}
		internal static bool IsFormatSupported(WaveFormat format, WaveFormat sourceFormat) => format.Channels == sourceFormat.Channels && format.ChannelMask == sourceFormat.ChannelMask;

		/// <inheritdoc />
		public override long FrameLength => (long)(_source.FrameLength * _internal?._factor ?? 0);

		sealed class Internal {
			readonly WaveFormat _inFormat;
			public readonly double _factor;
			public int _inBufferFrameLength;
			readonly int _filterWidth;
			readonly int _channelCount;
			int _inFrameOffset;
			byte[] _inBuffer = [];
			readonly Resampler[] _resampler;
			readonly double[][] _inSampleBuffer;
			readonly double[][] _outSampleBuffer;
			readonly SampleReader _sampleReader;
			readonly SampleWriter _sampleWriter;

			public Internal(WaveFormat inFormat, WaveFormat outFormat, bool highQuality) {
				_inFormat = inFormat;
				_channelCount = outFormat.Channels;
				_factor = (double)outFormat.SampleRate / inFormat.SampleRate;

				_resampler = new Resampler[_channelCount];
				for (int i = 0; i < _channelCount; i++) _resampler[i] = new(highQuality, _factor, _factor);
				_filterWidth = _resampler[0].FilterWidth;

				_inSampleBuffer = new double[_channelCount][];
				_outSampleBuffer = new double[_channelCount][];

				for (int i = 0; i < _channelCount; i++) {
					_inSampleBuffer[i] = [];
					_outSampleBuffer[i] = [];
				}

				_sampleReader = SampleConvert.GetSampleReader(inFormat.SampleFormat);
				_sampleWriter = SampleConvert.GetSampleWriter(outFormat.SampleFormat);
			}
			public int EnsureBufferSize(int targetBufferSize, int currentBufferSize) {
				int newBufferSize = AudioStreamHelpers.GetNewBufferSize(targetBufferSize, currentBufferSize);
				if (newBufferSize <= currentBufferSize) return currentBufferSize;

				_inBufferFrameLength = (int)(newBufferSize / _factor) + _filterWidth;
				_inBuffer = new byte[_inBufferFrameLength * _inFormat.FrameSize];

				for (int i = 0; i < _channelCount; i++) {
					var oldInSampleBuffer = _inSampleBuffer[i];
					var newInSampleBuffer = new double[_inBufferFrameLength];
					Array.Copy(oldInSampleBuffer, newInSampleBuffer, oldInSampleBuffer.Length);
					_inSampleBuffer[i] = newInSampleBuffer;

					_outSampleBuffer[i] = new double[newBufferSize];
				}

				return newBufferSize;
			}
			public unsafe int ReadFramesInternal(AudioStream source, ref byte buffer, int frameCount) {
				int inFrameCount = _inBufferFrameLength - _inFrameOffset;
				_inFrameOffset += inFrameCount = source.ReadFrames(_inBuffer, _inFrameOffset * _inFormat.FrameSize, inFrameCount);
				fixed (byte* rptr = _inBuffer) {
					byte* ptr = rptr;
					for (int i = _inFrameOffset - inFrameCount; i < _inFrameOffset; i++) {
						for (int c = 0; c < _channelCount; c++) {
							_inSampleBuffer[c][i] = _sampleReader(ref ptr);
						}
					}
				}
				int usedInFrameCount = 0, outFrameCount = 0;
				for (int c = 0; c < _channelCount; c++) {
					fixed (double* iptr = _inSampleBuffer[c], optr = _outSampleBuffer[c]) {
						outFrameCount = _resampler[c].Process(_factor, iptr, _inFrameOffset, false, out usedInFrameCount, optr, frameCount);
					}
				}
				fixed (byte* rptr = &buffer) {
					byte* ptr = rptr;
					for (int i = 0; i < frameCount; i++) {
						for (int c = 0; c < _channelCount; c++) {
							_sampleWriter(ref ptr, _outSampleBuffer[c][i]);
						}
					}
				}
				_inFrameOffset -= usedInFrameCount;
				if (_inFrameOffset > 0) {
					for (int c = 0; c < _channelCount; c++) {
						double[] sampleBuffer = _inSampleBuffer[c];
						Array.Copy(sampleBuffer, usedInFrameCount, sampleBuffer, 0, _inFrameOffset);
					}
				}
				return outFrameCount;
			}
		}

		/// <inheritdoc />
		protected override int EnsureBufferSize(int targetBufferSize) {
			int newBufferSize = _internal.EnsureBufferSize(targetBufferSize, BufferSize);
			_source.BufferSize = _internal._inBufferFrameLength;
			return newBufferSize;
		}

		readonly Internal _internal;
		/// <inheritdoc />
		protected override unsafe int ReadFramesInternal(ref byte buffer, int frameCount) {
			return _internal?.ReadFramesInternal(_source, ref buffer, frameCount) ?? 0;
		}

		/// <inheritdoc />
		protected override long SeekFrameInternal(long frameOffset, SeekOrigin origin) {
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override bool CanRead => true;
		/// <inheritdoc />
		public override bool CanSeek => true;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}

	/// <summary>
	/// A builder that builds <see cref="ResampledAudioSource" />.
	/// </summary>
	/// <param name="source">The source <see cref="AudioStream" />.</param>
	public class ResampledAudioSourceBuilder(AudioStream source) : AudioStreamBuilder<ResampledAudioSource> {
		/// <inheritdoc />
		public override WaveFormat DefaultFormat => source.Format;
		/// <inheritdoc />
		public override bool IsFormatSupported(WaveFormat format) => ResampledAudioSource.IsFormatSupported(format, source.Format);

		/// <summary>
		/// Whether to resample with high quality.
		/// </summary>
		public bool HighQuality { get; set; } = true;

		/// <inheritdoc />
		public override ResampledAudioSource Build(WaveFormat format) => new(source, format, HighQuality);
	}
}
