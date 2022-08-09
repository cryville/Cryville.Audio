using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Cryville.Audio.OpenSL;
using Cryville.Audio.Source;
using System;
using System.Threading;
using Xamarin.Essentials;
using Exception = System.Exception;

namespace Cryville.Audio.Test.Android {
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity {
		TextView log;
		AudioClient client;
		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			FindViewById<Button>(Resource.Id.button1).Click += OnClick;
			log = FindViewById<TextView>(Resource.Id.textView1);
		}
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
		async void OnClick(object sender, EventArgs e) {
			log.Text += "\nTest started\n";
			try {
				FFmpeg.AutoGen.ffmpeg.RootPath = "";
				using var manager = new Engine();
				log.Text += $"Engine created\n";
				using var device = manager.GetDefaultDevice(DataFlow.Out);
				log.Text += $"Device name: {device.Name}\n";
				client = device.Connect();
				client.Init(client.DefaultFormat);

				var file = await FilePicker.PickAsync();
				var source = new LibavFileAudioSource(file.FullPath);
				log.Text += string.Format("Duration: {0}s\n", source.GetDuration());
				log.Text += string.Format("Best stream index: {0}\n", source.BestStreamIndex);
				log.Text += string.Format("Best stream duration: {0}s\n", source.GetDuration(source.BestStreamIndex));
				source.SelectStream();
				client.Source = source;
				client.Start();
				for (int i = 0; i < 10; i++) {
					LogPosition("");
					Thread.Sleep(1000);
				}
				client.Source = null;
				client.Pause();
				source.Dispose();
			}
			catch (Exception ex) {
				log.Text += $"Error: {ex}\n";
			}
		}

		void LogPosition(string desc) {
			log.Text += string.Format(
				"Clock: {0:F6}s | Buffer: {1:F6}s | Latency: {2:F3}ms | {3}\n",
				client.Position, client.BufferPosition,
				(client.BufferPosition - client.Position) * 1e3, desc
			);
		}
	}
}