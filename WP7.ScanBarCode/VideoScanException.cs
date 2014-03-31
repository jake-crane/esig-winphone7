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

namespace WP7.ScanBarCode
{
    public class VideoScanException : Exception
    {
        public VideoScanException() 
        { 
        }

        public VideoScanException(string message) : base(message)
        {
        }

        public VideoScanException(string message, Exception e) : base(message, e)
        {
        }
    }
}
