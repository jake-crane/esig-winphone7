using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Devices;
using com.google.zxing;
using com.google.zxing.oned;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace WP7.ScanBarCode
{
    public partial class BarCode : PhoneApplicationPage
    {
        PhotoCamera _cam;
        VideoBrush _videoBrush = new VideoBrush();        
        byte[] _buffer;
        Reader _reader;
        Stopwatch watch = new Stopwatch();

        public static event Action<string> CodeBarFoundEvent;

        public BarCode()
        {            
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _cam = new PhotoCamera();

            _cam.Initialized += new EventHandler<CameraOperationCompletedEventArgs>(cam_Initialized);
            _cam.AutoFocusCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_AutoFocusCompleted);

            video.Fill = _videoBrush;
            _videoBrush.SetSource(_cam);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            _cam.CancelFocus();
            _cam.Dispose();

            base.OnNavigatingFrom(e);
        }

        void cam_Initialized(object sender, CameraOperationCompletedEventArgs e)
        {
            Dictionary<object, object> zxingHints 
                = new Dictionary<object, object>() { { DecodeHintType.TRY_HARDER, true } };
            _cam.FlashMode = FlashMode.Auto;
            _reader = new MultiFormatUPCEANReader(zxingHints);

            _cam.Focus();                    
        }

        BinaryBitmap GetBitmapFromVideo(PhotoCamera cam)
        {
            BinaryBitmap binaryBitmap = null;

            try
            {
                // Update buffer size    
                var pixelWidth = (int)_cam.PreviewResolution.Width;
                var pixelHeight = (int)_cam.PreviewResolution.Height;

                if (_buffer == null || _buffer.Length != (pixelWidth * pixelHeight))
                {
                    _buffer = new byte[pixelWidth * pixelHeight];
                }

                _cam.GetPreviewBufferY(_buffer);

                var luminance = new RGBLuminanceSource(_buffer, pixelWidth, pixelHeight, true);
                var binarizer = new com.google.zxing.common.HybridBinarizer(luminance);

                binaryBitmap = new BinaryBitmap(binarizer);
            }
            catch
            {
            }

            return binaryBitmap;
        }

        void cam_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            Result result = null;

            try
            {
                watch.Reset();
                watch.Start();

                while ((watch.ElapsedMilliseconds < 1500) && (result == null))
                {
                    var binaryBitmap = GetBitmapFromVideo(_cam);
                    if (binaryBitmap != null)
                    {
                        try
                        {
                            result = _reader.decode(binaryBitmap);
                        }
                        catch
                        {
                            // Wasn't able to find a barcode
                        }

                        if (result != null)
                            Dispatcher.BeginInvoke(() => OnBarCodeFound(result.Text));
                    }
                }

                // Try to focus again
                if (result == null)
                    _cam.Focus();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                NavigationService.GoBack();
            }
            finally
            {
                watch.Stop();
            }
        }

        private void OnBarCodeFound(string barcode)
        {
            VibrateController.Default.Start(TimeSpan.FromMilliseconds(200));
            //App.ViewModel.EditWineViewModel.BarCode = barcode;
            //NavigationService.GoBack();
            CodeBarFoundEvent(barcode);
        }
    }
}