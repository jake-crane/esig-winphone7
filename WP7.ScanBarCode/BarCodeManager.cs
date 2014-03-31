using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using com.google.zxing;
using System.Collections.Generic;

namespace WP7.ScanBarCode
{
    /// <summary>
    /// Scan a barcode for a live video stream
    /// </summary>
    public static class BarCodeManager
    {
        internal static Action<string> _onBarCodeFound;
        internal static Action<Exception> _onError;
        
        static BarCodeManager()
        {
            MaxTry = 15;   
        }

        /// <summary>
        /// Starts the scan : navigates to the scan page and starts reading video stream
        /// Note : Scan will auto-stop if navigation occurs
        /// </summary>
        /// <param name="onBarCodeFound">Delegate Action on a barcode found</param>
        /// <param name="onError">Delegate Action on error</param>
        /// <param name="zxingReader">(optional) A specific reader format, Default will be EAN13Reader </param>
        public static void StartScan(Action<string> onBarCodeFound, Action<Exception> onError, BarcodeFormat barcodeFormat = null)
        {
            
            var _mainFrame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (_mainFrame != null)
            {
                if (barcodeFormat == null)
                {
                    barcodeFormat = BarcodeFormat.EAN_13;
                }
                _onBarCodeFound = onBarCodeFound;
                _onError = onError;
                _ZXingReader = GetReader(barcodeFormat);

                _mainFrame.Navigate(new Uri("/WP7.ScanBarCode;component/BarCode.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// Try 20 times to focus and scan for 1,5 sec (default)
        /// </summary>
        public static int MaxTry
        {
            get;
            set;
        }

        private static Reader _ZXingReader;
        internal static Reader ZXingReader
        {
            get
            {
                if (_ZXingReader == null)
                    return new com.google.zxing.oned.EAN13Reader();
                return _ZXingReader;
            }

            set
            {
                _ZXingReader = value;
            }
        }

        /// <summary>
        /// Returns the zxing reader class for the current specified ScanMode.
        /// </summary>
        /// <returns></returns>
        internal static com.google.zxing.Reader GetReader(BarcodeFormat format)
        {
            Dictionary<object, object> zxingHints
                = new Dictionary<object, object>() { { DecodeHintType.TRY_HARDER, true } };
            com.google.zxing.Reader r;
            switch (format.Name)
            {
                case "CODE_128":
                    r = new com.google.zxing.oned.Code128Reader();
                    break;
                case "CODE_39":
                    r = new com.google.zxing.oned.Code39Reader();
                    break;
                case "EAN_13":
                    r = new com.google.zxing.oned.EAN13Reader();
                    break;
                case "EAN_8":
                    r = new com.google.zxing.oned.EAN8Reader();
                    break;
                case "ITF":
                    r = new com.google.zxing.oned.ITFReader();
                    break;
                case "UPC_A":
                    r = new com.google.zxing.oned.UPCAReader();
                    break;
                case "UPC_E":
                    r = new com.google.zxing.oned.UPCEReader();
                    break;
                case "QR_CODE":
                    r = new com.google.zxing.qrcode.QRCodeReader();
                    break;
                case "DATAMATRIX":
                    r = new com.google.zxing.datamatrix.DataMatrixReader();
                    break;

                case "ALL_1D":
                    r = new com.google.zxing.oned.MultiFormatOneDReader(zxingHints);
                    break;
                
                //Auto-Detect:
                case "UPC_EAN":
                default:
                    r = new com.google.zxing.oned.MultiFormatUPCEANReader(zxingHints);
                    break; 
            }
            return r;
        }
    }
}
