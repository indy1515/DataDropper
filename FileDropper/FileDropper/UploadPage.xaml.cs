using FileDropper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace FileDropper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private LocationFile locafile;
        //private Geoposition mylocation;
        StorageFile uploaded_file;
        CoreApplicationView view;
        public UploadPage()
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
            locafile = (LocationFile)(e.NavigationParameter);
            if (locafile.File != null)
            {
                uploaded_file = locafile.File;
                upload_filename.Text = uploaded_file.Name;
                file_name.Text = uploaded_file.DisplayName;
            }
            
            myMapControl.Center = locafile.Position.Coordinate.Point;
            myMapControl.ZoomLevel = 16d;

            MapIcon myLocationIcon = new MapIcon();
            //Image icon = 
            RandomAccessStreamReference refer = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/pinlocation.png"));
            myLocationIcon.Image = refer;
            myLocationIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            myLocationIcon.Location = new Geopoint(new BasicGeoposition
                {
                    Latitude = locafile.Position.Coordinate.Point.Position.Latitude,
                    Longitude = locafile.Position.Coordinate.Point.Position.Longitude

                });
            myLocationIcon.Title = "Drop Location";
            myMapControl.MapElements.Add(myLocationIcon);
            
        }
        private void FileTextListener(object sender, RoutedEventArgs e)
        {
            string extension = "";
            if (uploaded_file != null)
            {
                extension = uploaded_file.FileType;
            }
             
            upload_filename.Text = file_name.Text+extension;
        }

        private void ClickBrowseButton(object obj, RoutedEventArgs e)
        {
            view = CoreApplication.GetCurrentView();

            string ImagePath = string.Empty;
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            string[] types = { ".bmp", ".png", ".jpeg", ".jpg", ".txt", ".mp3", ".mp4",".avi", ".wmv", ".wav", ".ogg", ".raw" };
            // Filter to include a sample subset of file types
            //filePicker.FileTypeFilter.
            filePicker.FileTypeFilter.Clear();
            foreach (string type in types)
            {
                filePicker.FileTypeFilter.Add(type); 
            }

            filePicker.PickSingleFileAndContinue();
            view.Activated += viewActivated; 

        }

        private void viewActivated(CoreApplicationView sender, IActivatedEventArgs args1)
        {
            FileOpenPickerContinuationEventArgs args = args1 as FileOpenPickerContinuationEventArgs;

            if (args != null)
            {
                if (args.Files.Count == 0) return;

                view.Activated -= viewActivated;
                uploaded_file = args.Files[0];
                upload_filename.Text = uploaded_file.Name;
                file_name.Text = uploaded_file.DisplayName;
            }
        }

        private async void ClickSubmitButton(object obj, RoutedEventArgs e)
        {
            if (display_name.Text == "")
            {
                ShowToastNotification("Please Type File's Display Name!");
            }
            else if (your_name.Text == "")
            {
                ShowToastNotification("Please Type Your Name!");
            }
            else if (file_name.Text == "")
            {
                ShowToastNotification("File must have a name!");
            }
            else
            {
                string response = await PostData("http://gain.osk130.com/adprog/post.php", new FileData(display_name.Text,
                    locafile.Position.Coordinate.Point.Position.Latitude + "", locafile.Position.Coordinate.Point.Position.Longitude + "",
                    your_name.Text, file_name.Text), uploaded_file);
                ShowToastNotification("Done Uploading " + upload_filename.Text);
                Frame.Navigate(typeof(CompassPage), locafile);
                Debug.WriteLine("Response: " + response);
            }
        }


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

        public async Task<string> PostData(String url, FileData data, StorageFile file)
        {
            progressRing.IsActive = true;
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(data.Name), "name");
            form.Add(new StringContent(data.Position.Position.Latitude + ""), "x");
            form.Add(new StringContent(data.Position.Position.Longitude + ""), "y");
            form.Add(new StringContent(data.DropBy), "dropby");
            form.Add(new StringContent("ice"), "password");
            //  form.Add(new FormUrlEncodedContent(data), "profile_pic");
            byte[] imagebytearraystring = await GetBytesAsync(file);
            form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "file", upload_filename.Text);
            HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
          
            string sd = response.Content.ReadAsStringAsync().Result;
            progressRing.IsActive = false;
            return sd;
        }

        private async Task<byte[]> GetBytesAsync(StorageFile file)
        {
            byte[] fileBytes = null;
            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
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
    }
}
