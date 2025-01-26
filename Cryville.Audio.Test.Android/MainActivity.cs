using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Cryville.Audio.AAudio;
using Cryville.Audio.OpenSLES;
using Cryville.Interop.Java;
using Cryville.Interop.Java.Xamarin;
using System;
using System.Diagnostics.CodeAnalysis;
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
		[SuppressMessage("CodeQuality", "IDE0079", Justification = "False report")]
		[SuppressMessage("Design", "CA1031", Justification = "Test method")]
		void OnClick(object? sender, EventArgs e) {
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
				/*log.Text += $"= PlayWithLibAV =\n";
				test.PlayWithLibAV(file.FullPath);
				log.Text += $"= PlayWithSimpleQueue =\n";
				test.PlayWithSimpleQueue(file.FullPath, file.FullPath);
				log.Text += $"= PlayCachedWithSimpleQueue =\n";
				test.PlayCachedWithSimpleQueue(file.FullPath);
				log.Text += $"= PlayTwoSessions =\n";
				test.PlayTwoSessions(file.FullPath, file.FullPath);*/
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
			protected override AudioUsage Usage => AudioUsage.Notification;
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