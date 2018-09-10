using System;
using Android.Hardware.Camera2;

namespace CamWiz2.Droid.CameraImplement
{
    public class CameraCaptureStateListener : CameraCaptureSession.StateCallback
    {
        public Action<CameraCaptureSession> OnConfigureFailedAction;

        public Action<CameraCaptureSession> OnConfiguredAction;

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            if (OnConfigureFailedAction != null)
            {
                OnConfigureFailedAction(session);
            }
        }

        public override void OnConfigured(CameraCaptureSession session)
        {
            if (OnConfiguredAction != null)
            {
                OnConfiguredAction(session);
            }
        }
    }
}