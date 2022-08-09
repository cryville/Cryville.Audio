using Microsoft.Windows;
using Microsoft.Windows.AudioClient;
using Microsoft.Windows.MMDevice;
using Microsoft.Windows.PropSys;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Cryville.Audio.Test {
	[TestFixture]
	public class WasapiNativeTest {
		void Log(string format, params object[] args) {
			TestContext.WriteLine(string.Format(format, args));
		}

		IMMDeviceEnumerator enumerator;
		IMMDevice device;

		[OneTimeSetUp]
		public void OneTimeSetUp() {
			enumerator = new MMDeviceEnumerator() as IMMDeviceEnumerator;
			enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device);
		}

		[Test]
		public void EnumerateDevices() {
			enumerator.EnumAudioEndpoints(EDataFlow.eRender, (int)DEVICE_STATE_XXX.DEVICE_STATEMASK_ALL, out IMMDeviceCollection devices);
			devices.GetCount(out uint count);
			Log("Device count: {0}", count);
		}

		[Test]
		public void GetDeviceBasicInfo() {
			device.GetId(out string id);
			Log("ID: {0}", id);
			device.GetState(out uint pstate);
			var state = (DEVICE_STATE_XXX)pstate;
			Log("State: {0}", state);
			Assert.Less(state, DEVICE_STATE_XXX.DEVICE_STATEMASK_ALL);
		}

		[Test]
		public void GetDeviceProperties() {
			Guid desciid = typeof(IPropertyDescription).GUID;
			device.OpenPropertyStore((uint)STGM.READ, out IPropertyStore propertyStore);
			propertyStore.GetCount(out uint count);
			Log("Property count: {0}", count);
			for (uint i = 0; i < count; i++) {
				propertyStore.GetAt(i, out PROPERTYKEY key);
				if (!PKeys.Keys.TryGetValue(key, out string keyname)) {
					try {
						NativeMethods.PSGetPropertyDescription(ref key, ref desciid, out object pdesc);
						var desc = pdesc as IPropertyDescription;
						desc.GetCanonicalName(out keyname);
						Marshal.ReleaseComObject(desc);
					}
					catch (COMException) {
						keyname = key.ToString();
					}
				}
				propertyStore.GetValue(ref key, out PROPVARIANT pvalue);
				var value = pvalue.ToObject(null);
				Log("{0}: {1}", keyname, value);
			}
			Marshal.ReleaseComObject(propertyStore);
		}

		[Test]
		public void EnumerateProperties() {
			Guid desclistiid = typeof(IPropertyDescriptionList).GUID;
			Guid desciid = typeof(IPropertyDescription).GUID;
			NativeMethods.PSEnumeratePropertyDescriptions(PROPDESC_ENUMFILTER.ALL, ref desclistiid, out object pdesclist);
			var desclist = pdesclist as IPropertyDescriptionList;
			desclist.GetCount(out uint count);
			Log("Count: {0}", count);
			for (uint i = 0; i < count; i++) {
				desclist.GetAt(i, ref desciid, out object pdesc);
				var desc = pdesc as IPropertyDescription;
				desc.GetPropertyKey(out var key);
				if (!PKeys.Keys.TryGetValue(key, out string keyname)) {
					try {
						desc.GetCanonicalName(out keyname);
					}
					catch (COMException) {
						keyname = key.ToString();
					}
					finally {
						Marshal.ReleaseComObject(desc);
					}
				}
				Log("{0}: {1}", key, keyname);
			}
			Marshal.ReleaseComObject(desclist);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown() {
			Marshal.ReleaseComObject(device);
			Marshal.ReleaseComObject(enumerator);
		}
	}
}