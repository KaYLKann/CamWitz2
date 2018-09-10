using Android.Media;
using Java.Nio;
using System;
using Debug = System.Diagnostics.Debug;

namespace CamWiz2.Droid.CameraImplement
{
    public class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        public event EventHandler<byte[]> Photo;

        public void OnImageAvailable(ImageReader reader)
        {
            Image image = null;

            try
            {
                image = reader.AcquireLatestImage();
                ByteBuffer buffer    = image.GetPlanes()[0].Buffer;
                byte[]     imageData = new byte[buffer.Capacity()];
                buffer.Get(imageData);

                Photo?.Invoke(this, imageData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine( $"         [ImageAvailableListener] OnImageAvailable {ex.Message} {ex.StackTrace}");
            }
            finally
            {
                if (image != null)
                {
                    image.Close();
                }
            }
        }
    }
}