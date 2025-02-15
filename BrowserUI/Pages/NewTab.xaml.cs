using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Windows.Storage.Pickers;


namespace BrowserUI.Pages
{
    public partial class NewTab : Page
    {
        public NewTab()
        {
            this.InitializeComponent();
            WebView.Source = new Uri("https://www.google.com");
        }
        public void Dispose()
        {
            WebView?.Close();
            WebView?.CoreWebView2?.Stop();
            WebView = null;
        }

        public void GoHome()
        {
            WebView.Source = new Uri("https://www.google.com");
        }
        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebView.CanGoBack)
            {
                WebView.GoBack();
            }
        }
        public void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebView.CanGoForward)
            {
                WebView.GoForward();
            }
        }
        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WebView.Reload();
        }
        public void GoButton_Click(object sender, RoutedEventArgs e, string input)
        {

            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                // Navigate to the valid URL
                WebView.Source = uriResult;
            }
            else
            {
                // Treat the input as a search query
                string googleSearchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(input)}";
                WebView.Source = new Uri(googleSearchUrl);
            }
        }
        public async void CaptureScreenshot(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "Screenshot"
            };
            picker.FileTypeChoices.Add("PNG Image", new[] { ".png" });

            // Get the window handle for the MainWindow
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                await WebView.CoreWebView2.CapturePreviewAsync(
                    CoreWebView2CapturePreviewImageFormat.Png,
                    stream
                );
            }
        }

    }
}
