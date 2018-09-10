using Android;
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using CamWiz2.CameraImplementation;
using Orientation = CamWiz2.CameraImplementation.Orientation;

namespace CamWiz2.Droid
{
    [Activity(Label = "CamWiz2", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new System.String[] { Manifest.Permission.Camera }, 1);
            }

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {

            if (requestCode != 1) {
               
            }

            for (int i = 0; i < permissions.Length; i++) {
                if (permissions[i] == Manifest.Permission.Camera) {
                    if (grantResults[i] == Permission.Granted) {
                        
                    }
                }
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //todo : следим за изменением положения телефона 
        public override void OnConfigurationChanged(Configuration newConfig) {
            base.OnConfigurationChanged(newConfig);

            System.Diagnostics.Debug.WriteLine("MainActivity: Orientation changed. " + newConfig.Orientation);

            switch (newConfig.Orientation)
            {
                case Android.Content.Res.Orientation.Portrait:
                    OrientationPage.NotifyOrientationChange(Orientation.Portrait);  //todo: довести до view model 
                    break;
                case Android.Content.Res.Orientation.Landscape:
                    OrientationPage.NotifyOrientationChange(Orientation.LandscapeLeft);
                    break;
            }
        }
    }
}