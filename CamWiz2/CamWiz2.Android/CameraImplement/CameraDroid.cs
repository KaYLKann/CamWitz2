using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Net;

namespace CamWiz2.Droid.CameraImplement
{
    public class CameraDroid : FrameLayout, TextureView.ISurfaceTextureListener
    {
        #region Static Properties 

        private static readonly SparseIntArray ORIENTATIONS = new SparseIntArray();

        #endregion

        #region Public Events 

        public event EventHandler<bool> Busy;

        public event EventHandler<bool> Available;

        public event EventHandler<byte[]> Photo;

        #endregion

        #region Private Properties 

        private readonly string _tag;

        //private readonly ILogger _log;

        private CameraStateListener _mStateListener;

        private CaptureRequest.Builder _previewBuilder;

        private CameraCaptureSession _previewSession;

        private SurfaceTexture _viewSurface;

        private AutoFitTextureView _cameraTexture;

        private MediaActionSound _mediaSound;

        private Android.Util.Size _previewSize;

        private Context _context;

        private CameraManager _manager;

        private bool _mediaSoundLoaded;

        private bool _openingCamera;

        #endregion

        #region Public Properties 

        public bool OpeningCamera
        {
            get
            {
                return _openingCamera;
            }
            set
            {
                if (_openingCamera != value)
                {
                    _openingCamera = value;
                    Busy?.Invoke(this, value);
                }
            }
        }

        public CameraDevice cameraDevice;

        #endregion

        #region Constructors 

        public CameraDroid(Context context) : base(context)
        {
            _context          = context;
            _mediaSoundLoaded = LoadShutterSound();

            //_log = IoC.Resolve<ILogger>();
            _tag = $"{GetType()} ";

            var inflater = LayoutInflater.FromContext(context);

            if (inflater != null)
            {
                var view = inflater.Inflate(Resource.Layout.CameraLayout, this);

                _cameraTexture                        = view.FindViewById<AutoFitTextureView>(Resource.Id.CameraTexture);
                _cameraTexture.SurfaceTextureListener = this;

                _mStateListener = new CameraStateListener() { Camera = this };

                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
            }
        }

        #endregion

        #region Private Methods 

        private void UpdatePreview()
        {
            if (cameraDevice != null && _previewSession != null)
            {
                try
                {
                    // The camera preview can be run in a background thread. This is a Handler for the camere preview 
                    _previewBuilder.Set(CaptureRequest.ControlMode, new Java.Lang.Integer((int)ControlMode.Auto));
                    HandlerThread thread = new HandlerThread("CameraPreview");
                    thread.Start();
                    Handler backgroundHandler = new Handler(thread.Looper);

                    // Finally, we start displaying the camera preview 
                    _previewSession.SetRepeatingRequest(_previewBuilder.Build(), null, backgroundHandler);
                }
                catch (CameraAccessException error)
                {
                    System.Diagnostics.Debug.WriteLine($"{error.Message} {error.StackTrace}");
                    //_log.WriteLineTime(_tag + "\n" +
                    //                   "UpdatePreview() Camera access exception.  \n " +
                    //                   "ErrorMessage: \n" +
                    //                   error.Message + "\n" +
                    //                   "Stacktrace: \n " +
                    //                   error.StackTrace);
                }
                catch (IllegalStateException error)
                {
                    System.Diagnostics.Debug.WriteLine($"{error.Message} {error.StackTrace}");
                    //_log.WriteLineTime(_tag + "\n" +
                    //                   "UpdatePreview() Illegal exception.  \n " +
                    //                   "ErrorMessage: \n" +
                    //                   error.Message + "\n" +
                    //                   "Stacktrace: \n " +
                    //                   error.StackTrace);
                }
            }
        }

        private bool LoadShutterSound()
        {
            try
            {
                _mediaSound = new MediaActionSound();
                _mediaSound.LoadAsync(MediaActionSoundType.ShutterClick);

                return true;
            }
            catch (Java.Lang.Exception error)
            {
                System.Diagnostics.Debug.WriteLine($"{error.Message} {error.StackTrace}");
                //                _log.WriteLineTime(_tag + "\n" +
                //                                   "LoadShutterSound() Error loading shutter sound  \n " +
                //                                   "ErrorMessage: \n" +
                //                                   error.Message + "\n" +
                //                                   "Stacktrace: \n " +
                //                                   error.StackTrace);
            }

            return false;
        }

        private int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        #endregion


        #region Public Methods 

        public void OpenCamera()
        {
            if (_context == null || OpeningCamera)
            {
                return;
            }

            OpeningCamera = true;

            _manager = (CameraManager)_context.GetSystemService(Context.CameraService);

            try
            {
                string cameraId = _manager.GetCameraIdList()[0];

                // To get a list of available sizes of camera preview, we retrieve an instance of 
                // StreamConfigurationMap from CameraCharacteristics 
                CameraCharacteristics characteristics = _manager.GetCameraCharacteristics(cameraId);
                StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

                _previewSize = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)))[0];

                Android.Content.Res.Orientation orientation = Resources.Configuration.Orientation;


                if (orientation == Android.Content.Res.Orientation.Landscape)
                {
                    _cameraTexture.SetAspectRatio(_previewSize.Width, _previewSize.Height);
                }
                else
                {
                    _cameraTexture.SetAspectRatio(_previewSize.Height, _previewSize.Width);
                }

                
                // We are opening the camera with a listener. When it is ready, OnOpened of mStateListener is called. 
                _manager.OpenCamera(cameraId, _mStateListener, null);

                System.Diagnostics.Debug.WriteLine("            [camera debug] camera is opened");
            }
            catch (Java.Lang.Exception error)
            {
                System.Diagnostics.Debug.WriteLine($"{error.Message} {error.StackTrace}");
                //                _log.WriteLineTime(_tag + "\n" +
                //                    "OpenCamera() Failed to open camera  \n " +
                //                    "ErrorMessage: \n" +
                //                    error.Message + "\n" +
                //                    "Stacktrace: \n " +
                //                    error.StackTrace);

                Available?.Invoke(this, false);
            }
        }

        public void SetVideo() {
            var recorder = new Android.Media.MediaRecorder();
            recorder.SetAudioSource(AudioSource.Mic);
            recorder.SetVideoSource(VideoSource.Surface);
            recorder.SetOutputFormat(OutputFormat.Mpeg4);
            recorder.SetVideoEncodingBitRate(10000000);
            recorder.SetVideoFrameRate(30);
            recorder.SetVideoEncoder(VideoEncoder.H264);
            recorder.SetAudioEncoder(AudioEncoder.Aac);
            recorder.SetVideoSize(_previewSize.Width, _previewSize.Height);
            SurfaceOrientation rotation = windowManager.DefaultDisplay.Rotation;

            recorder.SetOrientationHint();

            var pfd = new ParcelFileDescriptor(ParcelFileDescriptor.FromSocket(new Socket()));
            recorder.SetOutputFile(pfd.FileDescriptor);

            #region sample
            /*mediaRecorder.SetAudioSource(AudioSource.Mic);
            mediaRecorder.SetVideoSource(VideoSource.Surface);
            mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            mediaRecorder.SetOutputFile(GetVideoFile(Activity).AbsolutePath);
            mediaRecorder.SetVideoEncodingBitRate(10000000);
            mediaRecorder.SetVideoFrameRate(30);
            mediaRecorder.SetVideoSize(videoSize.Width, videoSize.Height);
            mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
            mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
            int rotation    = (int)Activity.WindowManager.DefaultDisplay.Rotation;
            int orientation = ORIENTATIONS.Get(rotation);
            mediaRecorder.SetOrientationHint(orientation);          //
            mediaRecorder.Prepare();*/
            #endregion


        }

        public void TakePhoto()
        {
            if (_context != null && cameraDevice != null)
            {
                try
                {
                    Busy?.Invoke(this, true);

                    if (_mediaSoundLoaded)
                    {
                        _mediaSound.Play(MediaActionSoundType.ShutterClick);
                    }

                    // Pick the best JPEG size that can be captures with this CameraDevice 
                    var characteristics = _manager.GetCameraCharacteristics(cameraDevice.Id);
                    Android.Util.Size[] jpegSizes = null;
                    if (characteristics != null)
                    {
                        jpegSizes = ((StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap)).GetOutputSizes((int)ImageFormatType.Jpeg);
                    }
                    int width = 640;
                    int height = 480;

                    if (jpegSizes != null && jpegSizes.Length > 0)
                    {
                        width = jpegSizes[0].Width;
                        height = jpegSizes[0].Height;
                    }

                    // We use an ImageReader to get a JPEG from CameraDevice 
                    // Here, we create a new ImageReader and prepare its Surface as an output from the camera 
                    var reader = ImageReader.NewInstance(width, height, ImageFormatType.Jpeg, 1);
                    var outputSurfaces = new List<Surface>(2);
                    outputSurfaces.Add(reader.Surface);
                    outputSurfaces.Add(new Surface(_viewSurface));

                    CaptureRequest.Builder captureBuilder = cameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);
                    captureBuilder.AddTarget(reader.Surface);
                    captureBuilder.Set(CaptureRequest.ControlMode, new Integer((int)ControlMode.Auto));

                    // Orientation 
                    var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                    SurfaceOrientation rotation = windowManager.DefaultDisplay.Rotation;

                    captureBuilder.Set(CaptureRequest.JpegOrientation, new Integer(ORIENTATIONS.Get((int)rotation)));

                    // This listener is called when an image is ready in ImageReader  
                    ImageAvailableListener readerListener = new ImageAvailableListener();

                    readerListener.Photo += (sender, e) =>
                    {
                        Photo?.Invoke(this, e);
                    };

                    // We create a Handler since we want to handle the resulting JPEG in a background thread 
                    HandlerThread thread = new HandlerThread("CameraPicture");
                    thread.Start();
                    Handler backgroundHandler = new Handler(thread.Looper);
                    reader.SetOnImageAvailableListener(readerListener, backgroundHandler);

                    var captureListener = new CameraCaptureListener();

                    captureListener.PhotoComplete += (sender, e) =>
                    {
                        Busy?.Invoke(this, false);
                        StartPreview();
                    };

                    cameraDevice.CreateCaptureSession(outputSurfaces, new CameraCaptureStateListener()
                    {
                        OnConfiguredAction = (CameraCaptureSession session) =>
                        {
                            try
                            {
                                _previewSession = session;
                                session.Capture(captureBuilder.Build(), captureListener, backgroundHandler);
                            }
                            catch (CameraAccessException ex)
                            {
                                Log.WriteLine(LogPriority.Info, "Capture Session error: ", ex.ToString());
                            }
                        }
                    }, backgroundHandler);
                }
                catch (CameraAccessException error)
                {
                    System.Diagnostics.Debug.WriteLine($"{error.Message} {error.StackTrace}");
                    //                    _log.WriteLineTime(_tag + "\n" +
                    //                        "TakePhoto() Failed to take photo  \n " +
                    //                        "ErrorMessage: \n" +
                    //                        error.Message + "\n" +
                    //                        "Stacktrace: \n " +
                    //                        error.StackTrace);
                }
                catch (Java.Lang.Exception error)
                {
                    System.Diagnostics.Debug.WriteLine($"{error.Message} {error.StackTrace}");
//                    _log.WriteLineTime(_tag + "\n" +
//                        "TakePhoto() Failed to take photo  \n " +
//                        "ErrorMessage: \n" +
//                        error.Message + "\n" +
//                        "Stacktrace: \n " +
//                        error.StackTrace);
                }
            }
        }

        public void ChangeFocusPoint(Xamarin.Forms.Point e)
        {
            string cameraId = _manager.GetCameraIdList()[0];

            // To get a list of available sizes of camera preview, we retrieve an instance of 
            // StreamConfigurationMap from CameraCharacteristics 
            CameraCharacteristics characteristics = _manager.GetCameraCharacteristics(cameraId);

            var rect = characteristics.Get(CameraCharacteristics.SensorInfoActiveArraySize) as Rect;
            var size = characteristics.Get(CameraCharacteristics.SensorInfoPixelArraySize) as Size;

            int areaSize   = 200;
            int right      = rect.Right;
            int bottom     = rect.Bottom;
            int viewWidth  = _cameraTexture.Width;
            int viewHeight = _cameraTexture.Height;
            int ll, rr;

            Rect newRect;
            int  centerX = (int)e.X;
            int  centerY = (int)e.Y;

            ll = ((centerX * right) - areaSize) / viewWidth;
            rr = ((centerY * bottom) - areaSize) / viewHeight;

            int focusLeft   = Clamp(ll, 0, right);
            int focusBottom = Clamp(rr, 0, bottom);

            newRect = new Rect(focusLeft, focusBottom, focusLeft + areaSize, focusBottom + areaSize);
            MeteringRectangle   meteringRectangle    = new MeteringRectangle(newRect, 500);
            MeteringRectangle[] meteringRectangleArr = { meteringRectangle };
            _previewBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Cancel);
            _previewBuilder.Set(CaptureRequest.ControlAeRegions, meteringRectangleArr);
            _previewBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Start);

            UpdatePreview();
        }

        //function, this will be responsible for starting the camera stream through the TextureView
        public void StartPreview()
        {
            if (cameraDevice != null && _cameraTexture.IsAvailable && _previewSize != null)
            {
                try
                {
                    var texture = _cameraTexture.SurfaceTexture;

                    texture.SetDefaultBufferSize(_previewSize.Width, _previewSize.Height);
                    Surface surface = new Surface(texture);

                    _previewBuilder = cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
                    _previewBuilder.AddTarget(surface);

                    // Here, we create a CameraCaptureSession for camera preview. 
                    cameraDevice.CreateCaptureSession(new List<Surface>() { surface },
                                                      new CameraCaptureStateListener()
                                                      {
                                                          OnConfigureFailedAction = (CameraCaptureSession session) =>
                                                                                    {
                                                                                    },
                                                          OnConfiguredAction = (CameraCaptureSession session) =>
                                                                               {
                                                                                   _previewSession = session;
                                                                                   UpdatePreview();
                                                                               }
                                                      },
                                                      null);

                }
                catch (Java.Lang.Exception error)
                {
                    System.Diagnostics.Debug.WriteLine($"{error.Message} {error.StackTrace}");
                    //                    _log.WriteLineTime(_tag + "\n" +
                    //                                       "TakePhoto() Failed to start preview \n " +
                    //                                       "ErrorMessage: \n" +
                    //                                       error.Message + "\n" +
                    //                                       "Stacktrace: \n " +
                    //                                       error.StackTrace);
                }
            }
        }

        public void SwitchFlash(bool flashOn)
        {
            try
            {
                _previewBuilder.Set(CaptureRequest.FlashMode, new Integer(flashOn ? (int)FlashMode.Torch : (int)FlashMode.Off));
                UpdatePreview();
            }
            catch (System.Exception error)
            {
                System.Diagnostics.Debug.WriteLine($"{error.Message} {error.StackTrace}");
                //                _log.WriteLineTime(_tag + "\n" +
                //                                   "TakePhoto() Failed to switch flash on/off \n " +
                //                                   "ErrorMessage: \n" +
                //                                   error.Message + "\n" +
                //                                   "Stacktrace: \n " +
                //                                   error.StackTrace);

            }
        }

        public void NotifyAvailable(bool isAvailable)
        {
            Available?.Invoke(this, isAvailable);
        }

        public void OnLayout(int l, int t, int r, int b)
        {
            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            _cameraTexture.Measure(msw, msh);
            _cameraTexture.Layout(0, 0, r - l, b - t);
        }

        public void ConfigureTransform(int viewWidth, int viewHeight)
        {
            if (_viewSurface != null && _previewSize != null && _context != null)
            {
                var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

                var rotation   = windowManager.DefaultDisplay.Rotation;
                var matrix     = new Matrix();
                var viewRect   = new RectF(0, 0, viewWidth, viewHeight);
                var bufferRect = new RectF(0, 0, _previewSize.Width, _previewSize.Height);

                var centerX = viewRect.CenterX();
                var centerY = viewRect.CenterY();

                if (rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270)
                {
                    bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
                    matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);

                    matrix.PostRotate(90 * ((int)rotation - 2), centerX, centerY);
                }

                _cameraTexture.SetTransform(matrix);
            }
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int w, int h)
        {
            _viewSurface = surface;

            ConfigureTransform(w, h);
            StartPreview();
        }
        
        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            _previewSession.StopRepeating();

            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            ConfigureTransform(width, height);
            StartPreview();
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }
        #endregion
    }

}