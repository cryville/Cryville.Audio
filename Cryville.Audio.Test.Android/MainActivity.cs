using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Cryville.Interop.Java;
using Cryville.Interop.Java.Xamarin;
using System;
using System.Diagnostics.CodeAnalysis;
using Xamarin.Essentials;
using Exception = System.Exception;

namespace Cryville.Audio.Test.Android {
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity {
		[SuppressMessage("Usage", "CA2213", Justification = "Marshaled to Android GC")]
		TextView? log;
		protected override void OnCreate(Bundle? savedInstanceState) {
			base.OnCreate(savedInstanceState);
			Platform.Init(this, savedInstanceState);
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			JavaVMManager.Register(JniInvoke.Instance);
			EngineBuilder.Engines.Add(typeof(AAudio.AAudioManager));
			EngineBuilder.Engines.Add(typeof(OpenSLES.Engine));

			var button = FindViewById<Button>(Resource.Id.button1) ?? throw new InvalidOperationException("Button not found.");
			button.Click += OnClick;
			log = FindViewById<TextView>(Resource.Id.textView1);
		}
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
			Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
		[SuppressMessage("Design", "CA1031", Justification = "Test method")]
		async void OnClick(object? sender, EventArgs e) {
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
				var file = await FilePicker.PickAsync().ConfigureAwait(true);
				log.Text += $"= PlayWithLibAV =\n";
				test.PlayWithLibAV(file.FullPath);
				log.Text += $"= PlayWithSimpleQueue =\n";
				test.PlayWithSimpleQueue(file.FullPath, file.FullPath);
				log.Text += $"= PlayCachedWithSimpleQueue =\n";
				test.PlayCachedWithSimpleQueue(file.FullPath);
				log.Text += $"= PlayTwoSessions =\n";
				test.PlayTwoSessions(file.FullPath, file.FullPath);
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
			protected override void Log(string msg) => log.Text += msg + "\n";
		}
	}
}