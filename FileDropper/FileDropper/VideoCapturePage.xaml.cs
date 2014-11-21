using System;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace UniversalJupiter
{
    public sealed partial class VideoCapturePage : Page
    {
        private CameraCapture cameraCapture;
        private StorageFile videoFile;

        public VideoCapturePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Init and show preview
            cameraCapture = new CameraCapture();
            PreviewElement.Source = await cameraCapture.Initialize(CaptureUse.Video);
            await cameraCapture.StartPreview();
            videoFile = null;

            // Init camera properties / parameters
            InitializeVideoParamterControl(cameraCapture.VideoDeviceController.Brightness, SliderBrightness);
            InitializeVideoParamterControl(cameraCapture.VideoDeviceController.Focus, SliderFocus);
            InitializeVideoParamterControl(cameraCapture.VideoDeviceController.Zoom, SliderZoom);
            InitializeVideoParamterControl(cameraCapture.VideoDeviceController.Hue, SliderHue);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Release resources
            if (cameraCapture != null)
            {
                await cameraCapture.StopPreview();
                PreviewElement.Source = null;
                cameraCapture.Dispose();
                cameraCapture = null;
            }
        }

        private async void BtnRecordVideo_Click(object sender, RoutedEventArgs e)
        {
            if (videoFile == null)
            {
                // Start video recording
                videoFile = await cameraCapture.StartVideoRecording();
                BtnRecordVideo.Content = "Stop Video Recording";
            }
            else
            {
                // Stop video recording
                await cameraCapture.StopVideoRecording();

                // Start playback
                PlaybackElement.SetSource(await videoFile.OpenReadAsync(), videoFile.ContentType);

                // Update UI
                PlaybackElement.Visibility = Visibility.Visible;
                ParamtersUi.Visibility = Visibility.Collapsed;
                PreviewElement.Visibility = Visibility.Collapsed;
                BtnRecordVideo.Visibility = Visibility.Collapsed;
                BtnRecordVideo.Content = "Start Video Recording";
                videoFile = null;
            }
        }

        private void PlaybackElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            ParamtersUi.Visibility = Visibility.Visible;
            PreviewElement.Visibility = Visibility.Visible;
            BtnRecordVideo.Visibility = Visibility.Visible;
            PlaybackElement.Visibility = Visibility.Collapsed;
        }

        private void InitializeVideoParamterControl(MediaDeviceControl videoDeviceControl, Slider slider)
        {
            if (videoDeviceControl != null && (videoDeviceControl.Capabilities).Supported)
            {
                slider.IsEnabled = true;
                slider.Maximum = videoDeviceControl.Capabilities.Max;
                slider.Minimum = videoDeviceControl.Capabilities.Min;
                slider.StepFrequency = videoDeviceControl.Capabilities.Step;
                double currentValue;
                if (videoDeviceControl.TryGetValue(out currentValue))
                {
                    slider.Value = currentValue;
                }
            }
            else
            {
                slider.IsEnabled = false;
            }
        }

        private void SliderBrightness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var wasSuccess = cameraCapture.VideoDeviceController.Brightness.TrySetValue(SliderBrightness.Value);
        }

        private void SliderFocus_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var wasSuccess = cameraCapture.VideoDeviceController.Focus.TrySetValue(SliderFocus.Value);
        }

        private void SliderZoom_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var wasSuccess = cameraCapture.VideoDeviceController.Zoom.TrySetValue(SliderZoom.Value);
        }

        private void SliderHue_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var wasSuccess = cameraCapture.VideoDeviceController.Hue.TrySetValue(SliderHue.Value);
        }
    }
}
