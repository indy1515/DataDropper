﻿using FileDropper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace FileDropper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TakePhoto : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private MediaCapture captureManager = null;
        int cameradevice = 1;
        int phototaken = 0;
        private StorageFile videoFile;
        private StorageFile file;
        MediaEncodingProfile videoEncodingProperties;
        public TakePhoto()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
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
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //this.navigationHelper.OnNavigatedTo(e);
            if (captureManager == null)
            {
                captureManager = new MediaCapture();
                //await captureManager.InitializeAsync();
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                await captureManager.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = devices[1].Id
                });
                CapturePreview.Source = captureManager;
                captureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
                
                await captureManager.StartPreviewAsync();
            }
            //InitCameraButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            TakePhotoButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CapturedImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            CapturePreview.Visibility = Windows.UI.Xaml.Visibility.Visible;

            videoFile = null;
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //this.navigationHelper.OnNavigatedFrom(e);
            if (captureManager != null)
            {
                await captureManager.StopPreviewAsync();
                CapturePreview.Source = null;
                captureManager.Dispose();
                captureManager = null;
            }
        }

        #endregion

        /*private async void InitCamera_Click(object sender, RoutedEventArgs e)
        {
            if (captureManager == null)
            {
                captureManager = new MediaCapture();
                await captureManager.InitializeAsync();
                CapturePreview.Source = captureManager;
                await captureManager.StartPreviewAsync();
            }

            //InitCameraButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            TakePhotoButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CapturedImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            CapturePreview.Visibility = Windows.UI.Xaml.Visibility.Visible;
            switchcamerabtn.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
        }*/
        private async void ClickComingSoon(object sender, RoutedEventArgs e)
        {
            ShowToastNotification("Coming Soon!");
            await Task.Delay(2000);
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
        private async void TakePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (phototaken == 0)
            {
                phototaken = 1;
                ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

                // create storage file in local app storage
                //StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                //  "TempPhoto.jpg",
                //CreationCollisionOption.GenerateUniqueName);

                StorageFolder folder = KnownFolders.CameraRoll;
                file = await folder.CreateFileAsync("Unnamed.jpg", CreationCollisionOption.GenerateUniqueName);
                // take photo
                await captureManager.CapturePhotoToStorageFileAsync(imgFormat, file);


                // Get photo as a BitmapImage
                //NOT USED: BitmapImage bmpImage = new BitmapImage(new Uri(file.Path));

                Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                Image newImage = new Image();
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.SetSource(stream);
                CapturedImage.Source = bmpImage;
                if (cameradevice == 1)
                {
                    CapturedImage.RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 90 };
                }
                else
                {
                    CapturedImage.RenderTransform = new RotateTransform() { CenterX = 0.5, CenterY = 0.5, Angle = 270 };
                }
                //InitCameraButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                TakePhotoButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                toListFileButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                toCompassButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                CapturePreview.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                CapturedImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                switchcamerabtn.Visibility = Visibility.Collapsed;
                videocapturebtn.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
            }
        }

        private void toListFile_Click(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(ListFilePage)); 
        }

        private void toCompass_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CompassPage)); 
        }

        private async void switchCamera(object sender, RoutedEventArgs e)
        {
            await captureManager.StopPreviewAsync();
            captureManager = new MediaCapture();
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (cameradevice == 0)
            {
                await captureManager.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = devices[1].Id
                });
                cameradevice = 1;
                captureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
            }
            else
            {
                await captureManager.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = devices[0].Id
                });
                cameradevice = 0;
                captureManager.SetPreviewRotation(VideoRotation.Clockwise270Degrees);
            }
            CapturePreview.Source = captureManager;
            await captureManager.StartPreviewAsync();
        }

        private void toVideoCapture(object sender, RoutedEventArgs e)
        {
            
            if (TakePhotoButton.Visibility == Visibility.Visible)
            {
                changevideo.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/Camera/camera.png", UriKind.RelativeOrAbsolute));
                //change video image
                TakePhotoButton.Visibility = Visibility.Collapsed;
                TakeVideoButton.Visibility = Visibility.Visible;
            }
            else
            {
                changevideo.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/Camera/Video.png", UriKind.RelativeOrAbsolute));
                //change video image
                TakePhotoButton.Visibility = Visibility.Visible;
                TakeVideoButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CameraPress(object sender, PointerRoutedEventArgs e)
        {
            BitmapImage bm = new BitmapImage(new Uri(@"ms-appx:///Assets/Camera/Capture_Pressed.png", UriKind.RelativeOrAbsolute));
            CameraIcon.Source = bm;
        }

        private void CameraRelease(object sender, PointerRoutedEventArgs e)
        {
            BitmapImage bm = new BitmapImage(new Uri(@"ms-appx:///Assets/Camera/Capture.png", UriKind.RelativeOrAbsolute));
            CameraIcon.Source = bm;
        }

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (captureManager == null)
            {
                captureManager = new MediaCapture();
                await captureManager.InitializeAsync();
                CapturePreview.Source = captureManager;
                await captureManager.StartPreviewAsync();
            }

           
            if (videoFile == null)
            {
                //InitCameraButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                TakePhotoButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                toListFileButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                toCompassButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CapturedImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                CapturePreview.Visibility = Windows.UI.Xaml.Visibility.Visible;
                switchcamerabtn.Visibility = Visibility.Visible;
                videocapturebtn.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;
                phototaken = 0;

            }

            else
            {
                TakeVideoButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                toListFileButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                toCompassButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CapturedImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                CapturePreview.Visibility = Windows.UI.Xaml.Visibility.Visible;
                switchcamerabtn.Visibility = Visibility.Visible;
                videocapturebtn.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;
                phototaken = 0;

                await captureManager.StopPreviewAsync();
                VideoPreview.Source = null;
                captureManager.Dispose();
                captureManager = null;


                captureManager = new MediaCapture();
                //await captureManager.InitializeAsync();
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                await captureManager.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = devices[1].Id
                });
                CapturePreview.Source = captureManager;
                captureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
                await captureManager.StartPreviewAsync();

                videoFile = null;
            }
        }



        private async void TakeVideo_Click(object sender, RoutedEventArgs e)
        {

            if (videoFile == null)
            {
                if (cameradevice == 1)
                {
                    captureManager.SetRecordRotation(VideoRotation.Clockwise90Degrees);
                }

                else { captureManager.SetRecordRotation(VideoRotation.Clockwise270Degrees); }
                var videoStorageFile = await KnownFolders.VideosLibrary.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
                videoEncodingProperties = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga);
                await captureManager.StartRecordToStorageFileAsync(videoEncodingProperties, videoStorageFile);
                videoFile = videoStorageFile;
            }

            else
            {
                await captureManager.StopRecordAsync();

                VideoPreview.SetSource(await videoFile.OpenReadAsync(), videoFile.ContentType);
                Debug.WriteLine(videoFile.Path);



                TakeVideoButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                toListFileButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                toCompassButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                videocapturebtn.Visibility = Visibility.Collapsed;
                //CapturePreview.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //CapturedVideo.Visibility = Windows.UI.Xaml.Visibility.Visible;
                switchcamerabtn.Visibility = Visibility.Collapsed;
                TakeVideoButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;

            }
        }
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            geolocator.MovementThreshold = 1; // The units are meters.

            Geoposition myLocation = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10));

            Debug.WriteLine("Video is null: " + (videoFile == null) + " Image is null: " + (file == null));

            if (videoFile == null){
                phototaken = 0;
                LocationFile locafile = new LocationFile(file, myLocation);
                Frame.Navigate(typeof(UploadPage), locafile);
                //StorageFile file = await StorageFile.CreateStreamedFileAsync()
                //CapturedImage.Source
                //TODO Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            }

            else{
                LocationFile locafile = new LocationFile(videoFile, myLocation);
                // to do : save videoFile
                Frame.Navigate(typeof(UploadPage), locafile);
            }
        }

    }
}