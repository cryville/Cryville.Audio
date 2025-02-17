using Cryville.Audio.OpenSLES.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Cryville.Audio.OpenSLES {
	/// <summary>
	/// An <see cref="IAudioDeviceManager" /> that interacts with OpenSL ES.
	/// </summary>
	public class Engine : IAudioDeviceManager {
		internal SLItfWrapper<SLObjectItf> ObjEngine;

		/// <summary>
		/// Creates an instance of the <see cref="Engine" /> class.
		/// </summary>
		public Engine() {
			try {
				Helpers.SLR(Exports.slCreateEngine(out var pObjEngine, 0, null, 0, IntPtr.Zero, IntPtr.Zero), "slCreateEngine");
				ObjEngine = new SLItfWrapper<SLObjectItf>(pObjEngine);
				Helpers.SLR(ObjEngine.Obj.Realize(ObjEngine, false), "ObjEngine.Realize");
			}
			catch (EntryPointNotFoundException ex) {
				throw new PlatformNotSupportedException("OpenSL ES entry point not found.", ex);
			}
		}

		int _disposed;
		/// <inheritdoc />
		~Engine() {
			Dispose(false);
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
		protected virtual void Dispose(bool disposing) {
			if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
			ObjEngine.Obj.Destroy(ObjEngine);
		}

		/// <inheritdoc />
		public IAudioDevice GetDefaultDevice(DataFlow dataFlow) => new OutputDevice(this);

		/// <inheritdoc />
		[SuppressMessage("Reliability", "IDE0079", Justification = "False report")]
		[SuppressMessage("Reliability", "CA2000")]
		public IEnumerable<IAudioDevice> GetDevices(DataFlow dataFlow) => dataFlow switch {
			DataFlow.Out => [new OutputDevice(this)],
			DataFlow.In => throw new NotImplementedException(),
			_ => throw new NotSupportedException(),
		};
	}
}
