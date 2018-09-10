using Android.Hardware.Camera2;

namespace CamWiz2.Droid.CameraImplement
{
    public class CameraStateListener : CameraDevice.StateCallback
    {
        public CameraDroid Camera;

        public override void OnOpened(CameraDevice camera)
        {
            if (Camera != null)
            {
                Camera.cameraDevice = camera;
                Camera.StartPreview();
                Camera.OpeningCamera = false;

                Camera?.NotifyAvailable(true);
            }
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            if (Camera != null)
            {
                camera.Close();
                Camera.cameraDevice  = null;
                Camera.OpeningCamera = false;

                Camera?.NotifyAvailable(false);
            }
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            camera.Close();

            if (Camera != null)
            {
                Camera.cameraDevice  = null;
                Camera.OpeningCamera = false;

                Camera?.NotifyAvailable(false);
            }
        }
    }
}