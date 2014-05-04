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

namespace esig {

    public partial class Sign : PhoneApplicationPage {

        // the signature Id from the QR code
        private string sigId;

        //The server address from the QR code
        private string site;

        //stores the user's current point on the canvas
        private Point currentPoint;
        //stores the user's previous point on the canvas
        private Point previousPoint;

        //stores a list of timed points the user has drawn
        //null values i nthe list indicate the user lifted their finger
        List<TimedPoint> timedPoints = new List<TimedPoint>();

        /*The outputBytes is a byte array to be sent to the sigstore servlet.
        It will contain the sigid, imagedata, signature start time,
        signature finish time, canvas width, canvas height and canvas rotation
        */
        private byte[] outputBytes;

        public Sign() {
            InitializeComponent();
        }

        /// <summary>
        /// This method is called when the canvas loads.
        /// It simply calls the dradrawBaseline method.
        /// </summary>
        private void canvas1_Loaded(object sender, RoutedEventArgs e) {
            DrawBaseline();
        }

        /// <summary>
        /// This method is called when the user begins touching the canvas.
        /// It sets both currentPoint and previousPoint to the point at which the user's finger touched the canvas.
        /// </summary>
        private void canvas1_MouseEnter(object sender, MouseEventArgs e) {
            currentPoint = e.GetPosition(canvas1);
            previousPoint = currentPoint;
            timedPoints.Add(new TimedPoint(currentPoint.X, currentPoint.Y));
        }

        /// <summary>
        /// This method is called when the user lifts their finger from the canvas 
        /// or when the user drags their finger off of the canvas.
        /// This method inserts a null object into the timedPoints list
        /// to indicate that the user lifted their finger.
        /// </summary>
        private void canvas1_MouseLeave(object sender, MouseEventArgs e) {
            timedPoints.Add(null);
        }

        /// <summary>
        ///  This method is called when the user moves their finger on the canvas.
        ///  It will draw a line from the previous point on the canvas to the current point on the canvas.
        ///  It also draws an elipse at the old and current point to fill in gaps between the lines.
        /// </summary>
        private void canvas1_MouseMove(object sender, MouseEventArgs e) {
		
            /*This creates an ellipse for previousPoint.
            This can not be done in canvas1_mouseEnter because for 
            some reason button1_Click calls canvas1_MouseEnter.*/
            Ellipse ellipse1 = new Ellipse() {
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.Black),
                Width = 5,
                Height = 5
            };
            Canvas.SetLeft(ellipse1, previousPoint.X - ellipse1.Width / 2);
            Canvas.SetTop(ellipse1, previousPoint.Y - ellipse1.Height / 2);
            canvas1.Children.Add(ellipse1);

            currentPoint = e.GetPosition(canvas1);
            timedPoints.Add(new TimedPoint(currentPoint.X, currentPoint.Y));

            Line line = new Line() { X1 = previousPoint.X, Y1 = previousPoint.Y, X2 = currentPoint.X, Y2 = currentPoint.Y };
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

            previousPoint = currentPoint;

        }

        /// <summary>
        ///  This method draws a baseline on the canvas.
        ///  The line will be drawn 75% of the way down the canvas.
        ///  The line will be drawn 5% from the left of the canvas to 95% from the left of the canvas.
        /// </summary>
        void DrawBaseline() {

            Line line = new Line() {
                X1 = canvas1.ActualWidth * 0.05,
                Y1 = canvas1.ActualHeight * 0.75, 
                X2 = canvas1.ActualWidth * 0.95, 
                Y2 = canvas1.ActualHeight * 0.75,
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 4
            };

            canvas1.Children.Add(line);
        }

        /// <summary>
        ///  This method runs when the user presses the Clear button.
        ///  It will clear the canvas and timedPoints list then redraw the baseline.
        ///  Pressing this button often unintentionally calls canvas1's canvas1_MouseEnter and canvas1_MouseLeave events.
        ///  This has the effect of adding unwanted points to the timedPoints list. 
        ///  This issue prevents me from drawing an elipse when the user taps the canvas. 
        ///  Drawing an elipse when the user tapped the canvas would allow the user to more easily dot their i's
        ///  I was unable to come up with a good solution for this issue.
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e) {
            canvas1.Children.Clear();
            timedPoints.Clear();
            DrawBaseline();
        }

        /// <summary>
        ///  This method runs when the user presses the Submit button.
        ///  If the user has not entered any signature, it will print a message and return.
        ///  If the user has entered a signature, it will create a JSON string to be sent to 
        ///  the server and begin a POST request.
        ///  Pressing this button often unintentionally calls canvas1's canvas1_MouseEnter and canvas1_MouseLeave events.
        ///  This has the effect of adding extra unwanted points to the timedPoints list.
        ///  This issue prevents me from drawing an elipse when the user taps the canvas.
        ///  Drawing an elipse when the user tapped the canvas would allow the user to more easily dot their i's
        ///  I was unable to come up with a good solution for this issue.
        /// </summary>
        private void button2_Click(object sender, RoutedEventArgs e) {

            if (timedPoints.Count == 0) {
                MessageBox.Show("Please sign before submitting your signature.");
                return;
            }

            button2.Content = "Submitting...";
            button2.IsEnabled = false;

            string stringOutput = "{\"sigId\":\"" + sigId + "\",";

            stringOutput += GetJsonPointsString(timedPoints) + ",";


            WriteableBitmap wb = new WriteableBitmap(canvas1, null);
            byte[] bytes = ConvertToBytes(wb);

            stringOutput += "\"imageData\":\"" + Convert.ToBase64String(bytes) + "\",";

            stringOutput += "\"startTime\":" + GetFirstNonNullPoint(timedPoints).Time
                + ",\"finishTime\":" + GetLastNonNullPoint(timedPoints).Time
                + ",\"width\":" + canvas1.ActualWidth
                + ",\"height\":" + canvas1.ActualHeight
                + ",\"rotation\":0}";

            //MessageBox.Show("stringOutput: " + stringOutput);

            outputBytes = System.Text.Encoding.UTF8.GetBytes(stringOutput);

            Post();

        }

        /// <param name="timedPoints">
        /// The list to search.
        /// </param>
        /// <returns>
        /// Returns the first non-null point in the passed TimedPoint list if one exists. 
        /// If the list does not contain a non-null point, null will be returned.
        /// </returns>
        private TimedPoint GetFirstNonNullPoint(List<TimedPoint> timedPoints) {
            for(int i = 0; i < timedPoints.Count; i++) {
                if (timedPoints[i] != null) {
                    return timedPoints[i];
                }
            }
            return null;
        }

        /// <param name="timedPoints">
        /// The list to search.
        /// </param>
        /// <returns>
        /// Returns the last non-null point in the passed TimedPoint list if one exists. 
        /// If the list does not contain a non-null point, null will be returned.
        /// </returns>
        private TimedPoint GetLastNonNullPoint(List<TimedPoint> timedPoints) {
            for (int i = timedPoints.Count - 1; i > -1; i--) {
                if (timedPoints[i] != null) {
                    return timedPoints[i];
                }
            }
            return null;
        }

        /// <param name="timedPoints">
        /// The list to search.
        /// </param>
        /// <returns>
        /// Returns a string containing the JSON representation of the timedPoints list.
        /// </returns>
        public string GetJsonPointsString(List<TimedPoint> timedPoints) {
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

        /// <param name="wBitmap">
        /// A WritableBitmap to be converted to a byte array.
        /// </param>
        /// <returns>
        /// Returns a byte array representation of the passed WritableBitmap.
        /// </returns>
        public static byte[] ConvertToBytes(WriteableBitmap wBitmap) {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream()) {
                wBitmap.SaveJpeg(stream, wBitmap.PixelWidth, wBitmap.PixelHeight, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);
                data = stream.GetBuffer();
            }

            return data;
        }

        /// <summary>
        /// Sends a post request to the sigstore servlet and sets RequestReady at a callback method.
        /// </summary>
        void Post() {
            Uri uri = new Uri(site + "sigstore", UriKind.Absolute);
            //MessageBox.Show("posting to: " + uri.ToString());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";
            request.UserAgent = "windowsphone7 v0.0.1";
            request.BeginGetRequestStream(new AsyncCallback(RequestReady), request);
        }

        /// <summary>
        /// Writes the outputBytes array to to the stream created in the post method.
        /// </summary>
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

        /// <summary>
        /// Processes the server response.
        /// </summary>
        void ResponseReady(IAsyncResult asyncResult) {
            HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
            HttpWebResponse response;
            try {
                response = (HttpWebResponse)request.EndGetResponse(asyncResult);
            } catch (Exception w) {
                this.Dispatcher.BeginInvoke(delegate() {
                    MessageBox.Show("Exception Message:\n" + w.Message);
                    button2.Content = "Submit";
                    button2.IsEnabled = true;
                });
                return;
            }

            //Two comments below not by Jake
            // Hack for solving multi-threading problem 
            // I think this is a bug 
            this.Dispatcher.BeginInvoke(delegate() {
                //MessageBox.Show("Status Code:\n" + response.StatusCode.ToString());
                if (response.StatusCode.Equals(HttpStatusCode.OK) || response.StatusCode.Equals(HttpStatusCode.NoContent)) {
                    MessageBox.Show("Your signature has been received.");
                } else {
                    MessageBox.Show("Error: " + response.StatusCode);
                }
                response.Close();
                button2.Content = "Submit";
                button2.IsEnabled = true;
                /*using (Stream responseStream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(responseStream)) {
                        string result = reader.ReadToEnd();
                        MessageBox.Show("Result:\n" + result);
                    }
                }*/
            });
        }

        /// <summary>
        /// The method is called when the user is navigated to the sign page.
        /// It removes back entries and process the QR code string.
        /// </summary>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            //Remove back entries so when the user presses back the app will close.
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