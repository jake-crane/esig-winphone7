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
using com.google.zxing;

namespace esig {
    public partial class MainPage : PhoneApplicationPage {
        public MainPage() {
            InitializeComponent();
        }

        /// <summary>
        /// This method is called when the user presses the Start Scan button.
        /// It asynchronously starts scanning for a QR code.
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e) {
            WP7.ScanBarCode.BarCodeManager.StartScan(
                // on success
                (b) => Dispatcher.BeginInvoke(() => {
                    //NavigationService.GoBack();
                    NavigationService.Navigate(new Uri("/Sign.xaml?link=" + b, UriKind.Relative));
                }),
                // on error
                (ex) => Dispatcher.BeginInvoke(() => {
                    MessageBox.Show(ex.Message);
                    NavigationService.GoBack();
                }),
                // Please, decode a QR Code
                BarcodeFormat.QR_CODE);
        }

    }
}