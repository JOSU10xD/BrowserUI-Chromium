using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using Windows.Storage.Pickers;
using BrowserUIMultiCore;
using System.Diagnostics;

namespace BrowserUI.Pages
{
    public sealed partial class NewTab1 : Page, IBrowserTab
    {
        private string searchEngineUrl;

        public NewTab1()
        {
            this.InitializeComponent();
            LoadUserSearchEngine();

            // Initialize WebView2 Environment
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                await WebView.EnsureCoreWebView2Async();

                // Attach events after WebView2 environment is ready
                WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

                // Optionally: Open home page or search engine as default
                WebView.Source = new Uri(searchEngineUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize WebView2: {ex.Message}");
            }
        }

        private void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            // Prevent new windows from opening and navigate in the same WebView2
            args.Handled = true;
            WebView.Source = new Uri(args.Uri);
        }

        private void LoadUserSearchEngine()
        {
            try
            {
                if (AuthService.CurrentUser == null)
                {
                    searchEngineUrl = "https://www.bing.com";
                }
                else
                {
                    Settings userSettings = UserFolderManager.LoadUserSettings(AuthService.CurrentUser);
                    searchEngineUrl = userSettings?.SearchUrl ?? "https://www.bing.com";
                }
            }
            catch (Exception ex)
            {
                searchEngineUrl = "https://www.bing.com";
                Debug.WriteLine($"Failed to load user settings: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (WebView?.CoreWebView2 != null)
            {
                WebView.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
            }
            WebView?.Close();
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
                WebView.Source = uriResult;
            }
            else
            {
                string searchUrl = $"{searchEngineUrl}{Uri.EscapeDataString(input)}";
                WebView.Source = new Uri(searchUrl);
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

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                await WebView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
            }
        }
    }
}
