using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Cryville.Audio.AAudio;
using Cryville.Audio.OpenSLES;
using Cryville.Interop.Java;
using Cryville.Interop.Java.Xamarin;
using Java.IO;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Exception = System.Exception;

namespace Cryville.Audio.Test.Android {
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity {
		TextView? log;
		protected override void OnCreate(Bundle? savedInstanceState) {
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			JavaVMManager.Register(JniInvoke.Instance);

			var button = FindViewById<Button>(Resource.Id.button1) ?? throw new InvalidOperationException("Button not found.");
			button.Click += OnClick;
			log = FindViewById<TextView>(Resource.Id.textView1);
		}
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			if (disposing) {
				log?.Dispose();
			}
		}
		void OnClick(object? sender, EventArgs e) {
			using var intent = new Intent(Intent.ActionOpenDocument);
			intent.AddCategory(Intent.CategoryOpenable);
			intent.SetType("audio/*");

			StartActivityForResult(intent, 0);
		}
		[SuppressMessage("CodeQuality", "IDE0079", Justification = "False report")]
		[SuppressMessage("Design", "CA1031", Justification = "Test method")]
		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent? data) {
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode != 0) return;
			if (ContentResolver == null) return;
			if (data?.Data is not global::Android.Net.Uri uri) return;
			using var pfd = ContentResolver.OpenFileDescriptor(uri, "r");
			if (pfd == null) return;
			using var stream = new FileInputStream(pfd.FileDescriptor);
			if (ApplicationContext?.CacheDir?.Path is not string cacheDir) return;
			var cacheFile = Path.Combine(cacheDir, "temp");
			using var outStream = new FileStream(cacheFile, FileMode.Create, FileAccess.Write);
			var buffer = new byte[0x10000];
			int len = 0;
			while ((len = stream.Read(buffer, 0, buffer.Length)) > 0) {
				outStream.Write(buffer, 0, len);
			}

			if (log == null) return;
			log.Text += "\nTest started\n";
			var test = new AndroidManagedTest(log);
			try {
				FFmpeg.AutoGen.ffmpeg.RootPath = "";
				log.Text += $"= SetUp =\n";
				test.OneTimeSetUp();
				log.Text += $"= EnumerateDevices =\n";
				test.EnumerateDevices();
				log.Text += $"= GetDeviceInformation =\n";
				test.GetDeviceInformation();
				log.Text += $"= PlaySingleTone =\n";
				test.PlaySingleTone();

				ManagedTestCaseResources.File = cacheFile;
				log.Text += $"= PlayWithLibAV =\n";
				test.PlayWithLibAV();
				log.Text += $"= PlayWithSimpleQueue =\n";
				test.PlayWithSimpleQueue();
				log.Text += $"= PlayCachedWithSimpleQueue =\n";
				test.PlayCachedWithSimpleQueue();
				log.Text += $"= PlayTwoSessions =\n";
				test.PlayTwoSessions();
				log.Text += $"= PlaySoughtWithLibAV =\n";
				test.PlaySoughtWithLibAV();
				log.Text += $"= PlayResampledWithLibAV =\n";
				test.PlayResampledWithLibAV();
			}
			catch (Exception ex) {
				log.Text += $"= Error =\n{ex}\n";
			}
			finally {
				log.Text += $"= TearDown =\n";
				test.OneTimeTearDown();
			}
		}

		sealed class AndroidManagedTest(TextView log) : DefaultManagedTest {
			protected override IAudioDeviceManager? CreateEngine() {
				var builder = new EngineBuilder();
				builder.Engines.Add(typeof(AAudioManager));
				builder.Engines.Add(typeof(Engine));
				return builder.Create();
			}
			protected override void Log(string msg) => log.Text += msg + "\n";
		}
	}
}