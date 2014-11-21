using FileDropper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace FileDropper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompassPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Geolocator geolocator = null;
        private Geoposition myLocation = null;
        private CoreDispatcher _cd;
        private MapIcon myLocationIcon = null;
        private MapIcon destination = null;
        private double current_angle = 0;

        // Compass Accelerometer
        private Compass _compass; // Our app's compass object
        private double current_north = 0;

        // Current FileData
        public static FileList current_file = new FileList();
        //public static FileData current_file = new FileData();
        public CompassPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            Debug.WriteLine("Did Load on Create");
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            _cd = Window.Current.CoreWindow.Dispatcher;

            _compass = Compass.GetDefault(); // Get the default compass object

            // Assign an event handler for the compass reading-changed event
            if (_compass != null)
            {
                // Establish the report interval for all scenarios
                uint minReportInterval = _compass.MinimumReportInterval;
                uint reportInterval = minReportInterval > 500 ? minReportInterval : 500;
                _compass.ReportInterval = reportInterval;
                _compass.ReadingChanged += new TypedEventHandler<Compass, CompassReadingChangedEventArgs>(ReadingChanged);
            }
           

        }

        private async void ReadingChanged(object sender, CompassReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CompassReading reading = e.Reading;
                string magnetic = String.Format("{0,5:0.00}", reading.HeadingMagneticNorth);
                string north = "";
                if (reading.HeadingTrueNorth.HasValue)
                    north = String.Format("{0,5:0.00}", reading.HeadingTrueNorth);
                else
                    north = "No reading.";

                //Debug.WriteLine("Magnetic: " + magnetic + " North: " + north);
                double angle = Convert.ToDouble(magnetic);
                Storyboard board = new Storyboard();
                if (current_north != angle) {
                    updateIndicator();
                }
                current_north = -angle;
                var timeline = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(timeline, rotateTransform_compass);
                Storyboard.SetTargetProperty(timeline, "Angle");
                var frame = new EasingDoubleKeyFrame() { KeyTime = TimeSpan.FromMilliseconds(400), Value = current_north, EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut } };
                timeline.KeyFrames.Add(frame);
                board.Children.Add(timeline);

                

                board.Begin();
            });
        }
        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            
            //await loadData("http://api.rottentomatoes.com/api/public/v1.0/lists/movies/in_theaters.json?page_limit=16&page=1&country=us&apikey=592aacdwnb4649hcpjfkxy96");
            //FileData recieveFile = new FileData("MyFile", "13.8724809", "100.5830644", "Gain", "test.png");

            Debug.WriteLine("Did Load");
            this.pickBtn.Click += async (o, eb) =>
            {
                double displacement = getdistancebtw(myLocationIcon.Location, destination.Location);
                if (displacement > 5)
                {
                    ShowToastNotification("Too far away!");
                }
                else
                {
                    
                    ShowToastNotification("You have obtain " + current_file.NearestFile.FileName + " by " + current_file.NearestFile.DropBy);
                    await loadFile("http://gain.osk130.com/adprog/files/" + current_file.NearestFile.FileName, current_file.NearestFile.FileType);
                }
            
            };
            geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            geolocator.MovementThreshold = 1; // The units are meters.

            
            //Geoposition geoposition = null;
            Debug.WriteLine("Before Try");
            try
            {
                Debug.WriteLine("After Try");
                myLocation = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10));
                string mylat = myLocation.Coordinate.Point.Position.Latitude+"";
                string mylng = myLocation.Coordinate.Point.Position.Longitude+"";
                string radius = "100000000000";
                string link = "http://gain.osk130.com/adprog/getdata.php?lat=" + mylat + "&lon=" + mylng + "&distance=" + radius;
                Debug.WriteLine(link);
                await loadData(link,myLocation.Coordinate.Point);
                
            }
            catch (UnauthorizedAccessException ex)
            {
                // location services disabled or other error
                // user should setup his location
                Debug.WriteLine("Position not Found UnAuthorize: " + ex.StackTrace);
            }
            
            Debug.WriteLine("Done Try");
            string type = current_file.NearestFile.FileType;
            Debug.WriteLine("Type: "+type);
            this.fileImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/" + type + ".png"));
            myMapControl.MapServiceToken = "HJ1Q_ons4LFZJNL4KONPmg";
            if (DeviceInfo.IsRunningOnEmulator)
            {
                myMapControl.Center = current_file.NearestFile.Position;
            }
            else
            {
                myMapControl.Center = myLocation.Coordinate.Point;
            }
            myMapControl.ZoomLevel = 15d;



            Debug.WriteLine("Head : " + myLocation.Coordinate.Heading);

            myLocationIcon = new MapIcon();
            //Image icon = 
            RandomAccessStreamReference refer = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/mylocation.png"));
            myLocationIcon.Image = refer;
            myLocationIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);

            //Testing on Emulator
            if (DeviceInfo.IsRunningOnEmulator)
            {

                myLocationIcon.Location = new Geopoint(new BasicGeoposition
                {
                    Latitude = current_file.NearestFile.Position.Position.Latitude,
                    Longitude = current_file.NearestFile.Position.Position.Longitude+0.01

                });
            }
            else
            {
                myLocationIcon.Location = myLocation.Coordinate.Point;
            }
            myLocationIcon.Title = "You are here";
            myMapControl.MapElements.Add(myLocationIcon);

            Debug.WriteLine("Position " + "Lat: " + myLocation.Coordinate.Point.Position.Latitude + " Lng: " + myLocation.Coordinate.Point.Position.Longitude);


            destination = new MapIcon();
            destination.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/position02.png"));
            destination.NormalizedAnchorPoint = new Point(0.5, 1.0);
            destination.Location = current_file.NearestFile.Position;
            destination.Title = current_file.NearestFile.Name;
            myMapControl.MapElements.Add(destination);
            geolocator.StatusChanged += geolocator_StatusChanged;
            geolocator.PositionChanged += geolocator_PositionChanged;
        }

        private void simpleToast_Click(object sender, RoutedEventArgs e)  
        {  
            ToastTemplateType toastType = ToastTemplateType.ToastText02;  
  
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastType);  
  
            XmlNodeList toastTextElement = toastXml.GetElementsByTagName("text");  
            toastTextElement[0].AppendChild(toastXml.CreateTextNode("Hello C# Corner"));  
            toastTextElement[1].AppendChild(toastXml.CreateTextNode("I am poping you from a Winmdows Phone App"));  
  
            IXmlNode toastNode= toastXml.SelectSingleNode("/toast");  
            ((XmlElement)toastNode).SetAttribute("duration","long");  
  
            ToastNotification toast = new ToastNotification(toastXml);  
            ToastNotificationManager.CreateToastNotifier().Show(toast);  
  
  
        }  
        private void ShowToastNotification(String message)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            // Set Text
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(message));

            // Set image
            // Images must be less than 200 KB in size and smaller than 1024 x 1024 pixels.
            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/Compass/app_icon.png");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("alt", "logo");

            // toast duration
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");

            // toast navigation
            var toastNavigationUriString = "#/MainPage.xaml?param1=12345";
            var toastElement = ((XmlElement)toastXml.SelectSingleNode("/toast"));
            toastElement.SetAttribute("launch", toastNavigationUriString);

            // Create the toast notification based on the XML content you've specified.
            ToastNotification toast = new ToastNotification(toastXml);

            // Send your toast notification.
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        public static double getdistancebtw(Geopoint point1, Geopoint point2)
        {
            double lat1 = point1.Position.Latitude;
            double lat2 = point2.Position.Latitude;
            double long1 = point1.Position.Longitude;
            double long2 = point2.Position.Longitude;

            //return in meter
            return DistanceAlgorithm.DistanceBetweenPlaces(long1, lat1, long2, lat2)*1000;
        }
        private double angleFromCoordinate(Geopoint point1,Geopoint point2)
        {
            double lat1 = point1.Position.Latitude;
            double lat2 = point2.Position.Latitude;
            double long1 = point1.Position.Longitude;
            double long2 = point2.Position.Longitude;
            double dLon = (long2 - long1);

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1)
                    * Math.Cos(lat2) * Math.Cos(dLon);

            double brng = Math.Atan2(y, x);

            brng = RadianToDegree(brng);
            brng = (brng + 360) % 360;
            //brng = 360 - brng;
            Debug.WriteLine("Angle: " + brng); ;
            return brng;
        }
    
        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
   
        async void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            string status = "";

            switch (args.Status)
            {
                case PositionStatus.Disabled:
                    // the application does not have the right capability or the location master switch is off
                    status = "location is disabled in phone settings";
                    break;
                case PositionStatus.Initializing:
                    // the geolocator started the tracking operation
                    status = "initializing";
                    break;
                case PositionStatus.NoData:
                    // the location service was not able to acquire the location
                    status = "no data";
                    break;
                case PositionStatus.Ready:
                    // the location service is generating geopositions as specified by the tracking parameters
                    status = "ready";
                    break;
                case PositionStatus.NotAvailable:
                    status = "not available";
                    // not used in WindowsPhone, Windows desktop uses this value to signal that there is no hardware capable to acquire location information
                    break;
                case PositionStatus.NotInitialized:
                    // the initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state

                    break;
            }

            await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            
            {
                Debug.WriteLine( status);
            });
        }

        async void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //args.Position.Coordinate.Heading;

                Geocoordinate change = args.Position.Coordinate;
                Debug.WriteLine("Lat: " + change.Latitude + " Lan: " + change.Longitude + " Bearing " + change.Heading);
                myMapControl.MapElements.Remove(myLocationIcon);
                if (myLocationIcon == null)
                {
                    myLocationIcon = new MapIcon();
                    //Image icon = 
                    RandomAccessStreamReference refer = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/mylocation.png"));
                    myLocationIcon.Image = refer;
                    myLocationIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                    myLocationIcon.Title = "You are here";
                   
                }
                myLocationIcon.Location = change.Point;

                myMapControl.MapElements.Add(myLocationIcon);
                double angle = angleFromCoordinate(myLocationIcon.Location, destination.Location);
                current_angle = angle;

                updateIndicator();
               
                updateDistance();
                updateUserFile();
                //await Task.Delay(500);

                
                
                
            });
        }
        private void updateUserFile()
        {
            this.filename.Text = current_file.NearestFile.Name;
            this.type.Text = current_file.NearestFile.FileName;
            this.user.Text = current_file.NearestFile.DropBy;
        }
        private void updateFilePosition()
        {
            Storyboard board = new Storyboard();
            var timeline = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(timeline, rotateTransform);
            Storyboard.SetTargetProperty(timeline, "Angle");
            //Debug.WriteLine("Current North: " + current_north + " Current Angle: " + current_angle);
            double indicator_angle = (360 + current_north + current_angle) % 360;
            //Debug.WriteLine("File Angle: " + indicator_angle);
            var frame = new EasingDoubleKeyFrame() { KeyTime = TimeSpan.FromSeconds(1), Value = indicator_angle, EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut } };
            timeline.KeyFrames.Add(frame);
            board.Children.Add(timeline);

            board.Begin();
        }
        private void updateDistance()
        {
            double displacement = getdistancebtw(myLocationIcon.Location, destination.Location);


            if ((int)displacement >= 1000)
            {
                displacement = displacement / 1000;
                distance.Text = displacement.ToString("#,##0.#");
                this.unit.Text = "km.";
            }
            else
            {
                this.unit.Text = "m.";
                distance.Text = displacement.ToString("#,##0");
            }
        }

        async void updateIndicator()
        {
            await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Storyboard board = new Storyboard();
                var timeline = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(timeline, rotateTransform);
                Storyboard.SetTargetProperty(timeline, "Angle");
                //Debug.WriteLine("Current North: " + current_north + " Current Angle: " + current_angle);
                double indicator_angle = (360 + current_north + current_angle)%360;
                //Debug.WriteLine("Indicator Angle: " + indicator_angle);
                var frame = new EasingDoubleKeyFrame() { KeyTime = TimeSpan.FromSeconds(1), Value = indicator_angle, EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut } };
                timeline.KeyFrames.Add(frame);
                board.Children.Add(timeline);

                board.Begin();
            });
        }
        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        public async Task loadFile(string url, String type)
        {
            //MessageDialog msgbox = new MessageDialog("Message Box is displayed");
            //Calling the Show method of MessageDialog class  
            //which will show the MessageBox  
            //await msgbox.ShowAsync();  
            Uri dataUri = new Uri(url);

            Debug.WriteLine(dataUri);
            HttpClient client = new HttpClient();
            Stream stream_file = await client.GetStreamAsync(dataUri);

            //string url2 = await client.GetStringAsync(dataUri);
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            await SaveStreamToFile(stream_file, current_file.NearestFile.FileName, token);
           
        }
        public async Task SaveStreamToFile(Stream streamToSave, string fileName, CancellationToken cancelToken)
        {
            StorageFile file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (Stream fileStram = await file.OpenStreamForWriteAsync())
            {
                const int BUFFER_SIZE = 1024;
                byte[] buf = new byte[BUFFER_SIZE];

                int bytesread = 0;
                while ((bytesread = await streamToSave.ReadAsync(buf, 0, BUFFER_SIZE)) > 0)
                {
                    await fileStram.WriteAsync(buf, 0, bytesread);
                    cancelToken.ThrowIfCancellationRequested();
                }
            }
        }

        public async Task loadData(string url,Geopoint point)
        {
            Debug.WriteLine("Start Loading");
            Uri dataUri = new Uri(url);

            Debug.WriteLine(dataUri);
            HttpClient client = new HttpClient();
            string url2 = await client.GetStringAsync(dataUri);
            Debug.WriteLine(url2);
            string jsonText = url2;
            //handle none json
            if (jsonText.Contains("<!DOCTYPE html>"))
            {
                jsonText = jsonText.Substring(jsonText.IndexOf("<body>")+6, jsonText.LastIndexOf("</body>")-((jsonText.IndexOf("<body>")+6)));
            }
            Debug.WriteLine(jsonText);
            JsonArray jsonArray = JsonArray.Parse(jsonText);
            current_file = new FileList(jsonArray, point);
           


        }

    }
}
