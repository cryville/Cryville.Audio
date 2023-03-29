using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Cryville.Audio.Source {
	/// <summary>
	/// An <see cref="AudioStream" /> that uses Libav to demux and decode audio files.
	/// </summary>
	public class LibavFileAudioSource : AudioStream {
		internal unsafe class Internal {
			readonly AVFormatContext* formatCtx;
			AVCodec* codec;
			AVCodecContext* codecCtx;
			AVPacket* packet;
			AVFrame* frame;
			readonly SwrContext* swrContext;
			byte* _buffer;

			public readonly int BestStreamIndex;
			public readonly ReadOnlyCollection<int> Streams;
			public int SelectedStream;
			long _pts;
			WaveFormat? OutFormat;
			int BufferSize;
			int bytesPerSamplePerChannel;
			public bool EOF;
			public Internal(string file) {
				formatCtx = ffmpeg.avformat_alloc_context();
				fixed (AVFormatContext** formatCtxPtr = &formatCtx) {
					HR(ffmpeg.avformat_open_input(formatCtxPtr, file, null, null));
				}
				HR(ffmpeg.avformat_find_stream_info(formatCtx, null));
				BestStreamIndex = HR(ffmpeg.av_find_best_stream(formatCtx, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, null, 0));
				List<int> streams = new List<int>();
				for (int i = 0; i < formatCtx->nb_streams; i++)
					if (formatCtx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
						streams.Add(i);
				Streams = streams.AsReadOnly();
			}

			public double GetDuration(int index) {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				if (index >= formatCtx->nb_streams || index < -1)
					throw new ArgumentOutOfRangeException(nameof(index));
				if (index == -1) return (double)formatCtx->duration / ffmpeg.AV_TIME_BASE;
				else {
					var stream = formatCtx->streams[index];
					return (double)stream->duration * stream->time_base.num / stream->time_base.den;
				}
			}

			public double GetPresentationTimestamp() {
				if (swrContext == null) throw new ObjectDisposedException(null);
				var timeBase = formatCtx->streams[SelectedStream]->time_base;
				return (double)_pts * timeBase.num / timeBase.den;
			}

			public void OpenStream(int index) {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				if (codecCtx != null)
					throw new InvalidOperationException("Stream already opened.");
				if (index >= formatCtx->nb_streams)
					throw new ArgumentOutOfRangeException(nameof(index));
				SelectedStream = index;

				var param = formatCtx->streams[index]->codecpar;
				codec = ffmpeg.avcodec_find_decoder(param->codec_id);
				if (codec == null) throw new LibavException("Codec not found.");
				codecCtx = ffmpeg.avcodec_alloc_context3(codec);
				ffmpeg.avcodec_parameters_to_context(codecCtx, param);
				HR(ffmpeg.avcodec_open2(codecCtx, codec, null));

				packet = ffmpeg.av_packet_alloc();
			}

			public void SetFormat(WaveFormat format, int bufferSize) {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				if (OutFormat != null) throw new InvalidOperationException("Format already set.");
				if (codecCtx == null) OpenStream(BestStreamIndex);
				OutFormat = format;
				BufferSize = bufferSize;
				var outFormat = OutFormat.Value;
				bytesPerSamplePerChannel = OutFormat.Value.BitsPerSample * OutFormat.Value.Channels / 8;

				AVChannelLayout outLayout;
				ffmpeg.av_channel_layout_default(&outLayout, outFormat.Channels);

				frame = ffmpeg.av_frame_alloc();
				_buffer = (byte*)Marshal.AllocHGlobal(BufferSize).ToPointer();

				fixed (SwrContext** pSwrContext = &swrContext)
					HR(ffmpeg.swr_alloc_set_opts2(
						pSwrContext,
						&outLayout, ToInternalFormat(outFormat), (int)outFormat.SampleRate,
						&codecCtx->ch_layout, codecCtx->sample_fmt, codecCtx->sample_rate,
						0, null
					));

				HR(ffmpeg.swr_init(swrContext));
			}

			public int FillBuffer(byte[] buffer, int offset, int count) {
				if (swrContext == null) throw new ObjectDisposedException(null);
				int samples = count / bytesPerSamplePerChannel;
				int decoded = 0;
				if (!EOF) {
					while (decoded < samples) {
						int frame_size;
						int out_samples = HR(ffmpeg.swr_get_out_samples(swrContext, 0));
						if (out_samples < samples) {
							// Samples in the buffer are not sufficient. Read and decode a new frame.
							int ret = ffmpeg.avcodec_receive_frame(codecCtx, frame);
							if (ret == -0xb) {
								while (true) {
									ret = ffmpeg.av_read_frame(formatCtx, packet);
									if (ret == -0x20464f45) {
										EOF = true;
										goto eof;
									}
									else if (ret < 0) HR(ret);
									if (packet->stream_index == SelectedStream) {
										_pts = packet->pts;
										break;
									}
									ffmpeg.av_packet_unref(packet);
								}
								HR(ffmpeg.avcodec_send_packet(codecCtx, packet));
								ffmpeg.av_packet_unref(packet);
								continue;
							}
							fixed (byte** ptr = &_buffer) {
								frame_size = HR(ffmpeg.swr_convert(swrContext, ptr, samples - decoded, frame->extended_data, frame->nb_samples));
							}
							ffmpeg.av_frame_unref(frame);
						}
						else {
							// Samples in the buffer are sufficient. Flush them.
							fixed (byte** ptr = &_buffer) {
								frame_size = HR(ffmpeg.swr_convert(swrContext, ptr, samples - decoded, null, 0));
							}
							if (frame_size == 0) goto eof; // Don't know why this happens but dumb fix anyway
						}
						Marshal.Copy(new IntPtr(_buffer), buffer, decoded * bytesPerSamplePerChannel + offset, frame_size * bytesPerSamplePerChannel);
						decoded += frame_size;
					}
				}
			eof:
				int len = decoded * bytesPerSamplePerChannel;
				SilentBuffer(OutFormat.Value, buffer, offset + len, count - len);
				return len;
			}

			public void Seek(double time) {
				if (formatCtx == null) throw new ObjectDisposedException(null);
				var timeBase = formatCtx->streams[SelectedStream]->time_base;
				var ts = (long)(time / timeBase.num * timeBase.den);
				HR(ffmpeg.avformat_seek_file(formatCtx, SelectedStream, 0, ts, ts, 0));
				HR(ffmpeg.swr_drop_output(swrContext, HR(ffmpeg.swr_get_out_samples(swrContext, 0))));
				if (!EOF) {
					while (true) {
						int ret = ffmpeg.avcodec_receive_frame(codecCtx, frame);
						if (ret == -0xb) {
							while (true) {
								ret = ffmpeg.av_read_frame(formatCtx, packet);
								if (ret == -0x20464f45) {
									EOF = true;
									return;
								}
								else if (ret < 0) HR(ret);
								if (packet->stream_index == SelectedStream) {
									_pts = packet->pts;
									break;
								}
								ffmpeg.av_packet_unref(packet);
							}
							HR(ffmpeg.avcodec_send_packet(codecCtx, packet));
							ffmpeg.av_packet_unref(packet);
							continue;
						}
						long epts = _pts + frame->pkt_duration;
						if (epts >= ts) {
							fixed (byte** ptr = &_buffer) {
								for (int rem = (int)(frame->nb_samples * ((double)(ts - _pts) / (epts - _pts)) * OutFormat.Value.SampleRate / codecCtx->sample_rate); rem > 0; rem -= BufferSize) {
									HR(ffmpeg.swr_convert( swrContext, ptr, Math.Min(rem, BufferSize), frame->extended_data, frame->nb_samples));
								}
							}
							ffmpeg.av_frame_unref(frame);
							break;
						}
						ffmpeg.av_frame_unref(frame);
					}
				}
			}

			public void Close() {
				if (_buffer != null) {
					Marshal.FreeHGlobal(new IntPtr(_buffer));
					_buffer = null;
				}
				if (swrContext != null) {
					fixed (SwrContext** swrContextPtr = &swrContext) {
						ffmpeg.swr_free(swrContextPtr);
					}
				}
				if (frame != null) {
					fixed (AVFrame** framePtr = &frame) {
						ffmpeg.av_frame_free(framePtr);
					}
				}
				if (packet != null) {
					fixed (AVPacket** packetPtr = &packet) {
						ffmpeg.av_packet_free(packetPtr);
					}
				}
				if (codecCtx != null) {
					fixed (AVCodecContext** codecCtxPtr = &codecCtx) {
						ffmpeg.avcodec_free_context(codecCtxPtr);
					}
				}
				if (formatCtx != null) {
					fixed (AVFormatContext** formatCtxPtr = &formatCtx) {
						ffmpeg.avformat_close_input(formatCtxPtr);
					}
					ffmpeg.avformat_free_context(formatCtx);
				}
			}

			static int HR(int value) {
				if (value < 0) throw new LibavException(string.Format(CultureInfo.InvariantCulture, "An external error occured: 0x{0:x8}", -value));
				return value;
			}

			static AVSampleFormat ToInternalFormat(WaveFormat value) {
				switch (value.SampleFormat) {
					case SampleFormat.U8: return AVSampleFormat.AV_SAMPLE_FMT_U8;
					case SampleFormat.S16: return AVSampleFormat.AV_SAMPLE_FMT_S16;
					case SampleFormat.S32: return AVSampleFormat.AV_SAMPLE_FMT_S32;
					case SampleFormat.F32: return AVSampleFormat.AV_SAMPLE_FMT_FLT;
					case SampleFormat.F64: return AVSampleFormat.AV_SAMPLE_FMT_DBL;
					default: throw new NotSupportedException();
				}
			}
		}
		readonly Internal _internal;

		/// <summary>
		/// Creates an instance of the <see cref="LibavFileAudioSource" /> class and loads the specified <paramref name="file" />.
		/// </summary>
		/// <param name="file">The audio file.</param>
		public LibavFileAudioSource(string file) {
			_internal = new Internal(file);
		}

		/// <summary>
		/// Whether this audio stream has been disposed.
		/// </summary>
		public bool Disposed { get; private set; }

		/// <inheritdoc />
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			_internal.Close();
			Disposed = true;
		}

		/// <inheritdoc />
		public override bool EndOfData => _internal.EOF;

		/// <summary>
		/// The index to the best audio stream.
		/// </summary>
		public int BestStreamIndex => _internal.BestStreamIndex;
		/// <summary>
		/// The collection of indices to all audio streams.
		/// </summary>
		public ReadOnlyCollection<int> Streams => _internal.Streams;

		/// <summary>
		/// Selects the best stream as the source.
		/// </summary>
		/// <exception cref="InvalidOperationException">The stream has been selected.</exception>
		/// <remarks>
		/// <para>This method can only be called before <see cref="AudioStream.SetFormat(WaveFormat, int)" /> is called, which is called while setting <see cref="AudioClient.Source" />.</para>
		/// </remarks>
		public void SelectStream() => SelectStream(BestStreamIndex);

		/// <summary>
		/// Selects a stream as the source.
		/// </summary>
		/// <param name="index">The index of the stream.</param>
		/// <exception cref="InvalidOperationException">The stream has been selected.</exception>
		/// <remarks>
		/// <para>This method can only be called before <see cref="AudioStream.SetFormat(WaveFormat, int)" /> is called, which is called while setting <see cref="AudioClient.Source" />.</para>
		/// </remarks>
		public void SelectStream(int index) => _internal.OpenStream(index);

		/// <summary>
		/// Gets the duration of a stream or the file.
		/// </summary>
		/// <param name="streamId">The stream index. The duration of the file is retrieved if <c>-1</c> is specified.</param>
		/// <returns>The duration in seconds.</returns>
		public double GetStreamDuration(int streamId = -1) => _internal.GetDuration(streamId);

		/// <inheritdoc />
		protected internal override bool IsFormatSupported(WaveFormat format)
			=> format.SampleFormat == SampleFormat.U8
			|| format.SampleFormat == SampleFormat.S16
			|| format.SampleFormat == SampleFormat.S32
			|| format.SampleFormat == SampleFormat.F32
			|| format.SampleFormat == SampleFormat.F64;

		/// <inheritdoc />
		protected override void OnSetFormat() => _internal.SetFormat(Format, BufferSize);

		int _pos;
		double _time;
		/// <inheritdoc />
		public override int Read(byte[] buffer, int offset, int count) {
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (buffer.Length - offset < count) throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
			if (Disposed) throw new ObjectDisposedException(null);
			var readCount = _internal.FillBuffer(buffer, offset, count);
			_time = _internal.GetPresentationTimestamp();
			_pos += readCount;
			return readCount;
		}

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin) {
			SeekTime((double)Format.Align(offset) / Format.BytesPerSecond, origin);
			return Position;
		}
		/// <inheritdoc />
		public override double SeekTime(double offset, SeekOrigin origin) {
			if (Disposed) throw new ObjectDisposedException(null);
			double newTime;
			switch (origin) {
				case SeekOrigin.Begin: newTime = offset; break;
				case SeekOrigin.Current: newTime = _time + offset; break;
				case SeekOrigin.End: newTime = Duration + offset; break;
				default: throw new ArgumentException("Invalid SeekOrigin.", nameof(origin));
			}
			if (newTime < 0) throw new ArgumentException("Seeking is attempted before the beginning of the stream.");
			_internal.Seek(newTime);
			_time = _internal.GetPresentationTimestamp();
			_pos = Format.Align(_time * Format.BytesPerSecond);
			return Time;
		}

		/// <inheritdoc />
		public override bool CanRead => !Disposed;
		/// <inheritdoc />
		public override bool CanSeek => !Disposed;
		/// <inheritdoc />
		public override bool CanWrite => false;
		/// <inheritdoc />
		/// <remarks>
		/// <para>This property may be inaccurate.</para>
		/// </remarks>
		public override long Length => Format.Align(Duration * Format.BytesPerSecond);
		/// <inheritdoc />
		/// <remarks>
		/// <para>This property may be inaccurate.</para>
		/// </remarks>
		public override double Duration => GetStreamDuration(_internal.SelectedStream);
		/// <inheritdoc />
		public override double Time => _time;
		/// <inheritdoc />
		/// <remarks>
		/// <para>This property may become inaccurate after <see cref="Seek(long, SeekOrigin)" /> is called.</para>
		/// </remarks>
		public override long Position { get => _pos; set => Seek(value, SeekOrigin.Begin); }
		/// <inheritdoc />
		public override void Flush() { }
		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();
		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}

	/// <summary>
	/// The exception that is thrown by Libav.
	/// </summary>
	[Serializable]
	public class LibavException : Exception {
		/// <inheritdoc />
		public LibavException() { }
		/// <inheritdoc />
		public LibavException(string message) : base(message) { }
		/// <inheritdoc />
		public LibavException(string message, Exception innerException) : base(message, innerException) { }
		/// <inheritdoc />
		protected LibavException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
