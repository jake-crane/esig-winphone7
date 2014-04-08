using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Browser;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Phone.Controls;
using com.google.zxing;

namespace PhoneApp9 {

    public partial class MainPage : PhoneApplicationPage {

        public class TimedPoint {

            public double X {get; set;}

            public double Y {get; set;}

            public long Time {get; set;}

            public TimedPoint(double x, double y) {
                X = x;
                Y = y;
                Time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            }

            public override string ToString() {
                return "{\"x\":" + X + ",\"y\":" + Y + ",\"time\":" + Time + "}";
            }

        }

        string sigId;
        string site;

        private Point currentPoint;
        private Point oldPoint;

        List<TimedPoint> timedPoints = new List<TimedPoint>();

        byte[] outputBytes;

        public MainPage() {
            InitializeComponent();
            drawBaseline();
        }

        private void canvas1_Loaded(object sender, RoutedEventArgs e) {
            drawBaseline();
        }

        private void canvas1_MouseEnter(object sender, MouseEventArgs e) {
            currentPoint = e.GetPosition(canvas1);
            oldPoint = currentPoint;
            timedPoints.Add(new TimedPoint(currentPoint.X, currentPoint.Y));
        }

        private void canvas1_MouseLeave(object sender, MouseEventArgs e) {
            timedPoints.Add(null);
        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e) {
		
            /*This create an ellipse for oldPoint. 
            This can not be done in canvas1_mouseEnter because for 
            some reason button1_Click calls canvas1_MouseEnter.*/
            Ellipse ellipse1 = new Ellipse() {
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.Black),
                Width = 5,
                Height = 5
            };
            Canvas.SetLeft(ellipse1, oldPoint.X - ellipse1.Width / 2);
            Canvas.SetTop(ellipse1, oldPoint.Y - ellipse1.Height / 2);
            canvas1.Children.Add(ellipse1);

            currentPoint = e.GetPosition(canvas1);
            timedPoints.Add(new TimedPoint(currentPoint.X, currentPoint.Y));

            Line line = new Line() { X1 = oldPoint.X, Y1 = oldPoint.Y, X2 = currentPoint.X, Y2 = currentPoint.Y };
            line.Stroke = new SolidColorBrush(Colors.Black);
            line.StrokeThickness = 5;
            canvas1.Children.Add(line);

            //Create an ellipse for currentPoint
            Ellipse ellipse2 = new Ellipse() {
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.Black),
                Width = 5,
                Height = 5
            };
            Canvas.SetLeft(ellipse2, currentPoint.X - ellipse2.Width / 2);
            Canvas.SetTop(ellipse2, currentPoint.Y - ellipse2.Height / 2);
            canvas1.Children.Add(ellipse2);

            oldPoint = currentPoint;

        }

        void drawBaseline() {

            Line line = new Line() {
                X1 = canvas1.ActualWidth * 0.05,
                Y1 = canvas1.ActualHeight * 0.75, 
                X2 = canvas1.ActualWidth * 0.95, 
                Y2 = canvas1.ActualHeight * 0.75 };

            line.Stroke = new SolidColorBrush(Colors.Gray);

            line.StrokeThickness = 4;

            canvas1.Children.Add(line);
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            canvas1.Children.Clear();
            timedPoints.Clear();
            drawBaseline();
        }

        private void button2_Click(object sender, RoutedEventArgs e) {

            string stringOutput = "{\"sigId\":\"" + sigId + "\",";

            stringOutput += GetJsonPointsString() + ",";


            WriteableBitmap wb = new WriteableBitmap(canvas1, null);
            byte[] bytes = ConvertToBytes(wb);

            stringOutput += "\"dataURL\":\"data:image/png;base64," + Convert.ToBase64String(bytes) + "\",";

            stringOutput += "\"rotation\":0}";

            //MessageBox.Show("stringOutput: " + stringOutput);

            outputBytes = System.Text.Encoding.UTF8.GetBytes(stringOutput);

            post();

        }

        public string GetJsonPointsString() {
            string s = "\"points\":[";
            int i = 0;
            for (; i < timedPoints.Count - 2; i++) { //last point is null
                if (timedPoints[i] == null) {
                    s += "null,";
                } else {
                    s += timedPoints[i].ToString() + ",";
                }
            }
            s += timedPoints[i].ToString() + "]";

            return s;
        }

        public static byte[] ConvertToBytes(WriteableBitmap wBitmap) {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream()) {
                wBitmap.SaveJpeg(stream, wBitmap.PixelWidth, wBitmap.PixelHeight, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);
                data = stream.GetBuffer();
            }

            return data;
        }

        void post() {
            Uri uri = new Uri(site + "sigstore", UriKind.Absolute);
            //MessageBox.Show("posting to: " + uri.ToString());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "text/plain; charset=UTF-8";
            request.BeginGetRequestStream(new AsyncCallback(RequestReady), request);
        }

        void RequestReady(IAsyncResult asyncResult) {

            HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;

            //Two comments below not by Jake
            // Hack for solving multi-threading problem 
            // I think this is a bug 
            this.Dispatcher.BeginInvoke(delegate() {
                //MessageBox.Show("output length: " + outputBytes.Length.ToString());
                using (Stream stream = request.EndGetRequestStream(asyncResult)) {
                    //MessageBox.Show("stream length: " + stream.Length);
                    stream.Write(outputBytes, 0, outputBytes.Length);
                    //MessageBox.Show("stream length: " + stream.Length);
                }
                request.BeginGetResponse(new AsyncCallback(ResponseReady), request);
            });
        }

        void ResponseReady(IAsyncResult asyncResult) {
            HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
            HttpWebResponse response;
            try {
                response = (HttpWebResponse)request.EndGetResponse(asyncResult);
            } catch (Exception w) {
                this.Dispatcher.BeginInvoke(delegate() {
                    MessageBox.Show("Exception Message:\n" + w.Message);
                });
                return;
            }

            //Two comments below not by Jake
            // Hack for solving multi-threading problem 
            // I think this is a bug 
            this.Dispatcher.BeginInvoke(delegate() {
                //MessageBox.Show("Status Code:\n" + response.StatusCode.ToString());
                if (response.StatusCode.Equals(HttpStatusCode.OK)) {
                    MessageBox.Show("Your signature has been received.");
                } else {
                    MessageBox.Show("Error: " + response.StatusCode);
                }
                /*using (Stream responseStream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(responseStream)) {
                        string result = reader.ReadToEnd();
                        MessageBox.Show("Result:\n" + result);
                    }
                }*/
            });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            NavigationService.RemoveBackEntry();
            NavigationService.RemoveBackEntry();

            string link;

            if (NavigationContext.QueryString.TryGetValue("link", out link)) {
                //MessageBox.Show("link from QR code: " + link);
                sigId = link.Split('=')[1];
                //MessageBox.Show("sigId: " + signId);
                site = link.Split(new string[] { "sign" }, StringSplitOptions.None)[0];
                //MessageBox.Show("site: " + site);
            }

        }

    }

}