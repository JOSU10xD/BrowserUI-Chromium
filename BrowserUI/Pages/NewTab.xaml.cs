using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.UI.Dispatching;
using BrowserUIMultiCore;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.UI.WebUI;

namespace BrowserUI.Pages
{
    public sealed partial class NewTab : Page, IBrowserTab
    {
        private DispatcherTimer timer;
        private string searchEngineUrl;

        private List<string> navigationHistory = new();
        private int currentHistoryIndex = -1;

        private bool IsNavigatingThroughHistory = false;
        private string lastCommittedUrl = null;
        private string pendingUrl = null;

        public NewTab()
        {
            this.InitializeComponent();
            InitializeTime();
            LoadUserSearchEngine();
            InitializeWebViewSettings();

            PushToHistory("HOME");
            ShowHomeScreen();
        }

        private void InitializeTime()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) =>
            {
                var now = DateTime.Now;
                NtpTime.Text = now.ToString("HH:mm:ss");
                NtpDate.Text = now.ToString("dddd, MMM dd yyyy");
            };
            timer.Start();
        }

        private void LoadUserSearchEngine()
        {
            try
            {
                if (AuthService.CurrentUser == null)
                {
                    searchEngineUrl = "https://www.bing.com/search?q=";
                }
                else
                {
                    Settings userSettings = UserFolderManager.LoadUserSettings(AuthService.CurrentUser);
                    searchEngineUrl = userSettings?.SearchUrl ?? "https://www.bing.com/search?q=";
                }
            }
            catch (Exception ex)
            {
                searchEngineUrl = "https://www.bing.com/search?q=";
                Debug.WriteLine($"Failed to load user settings: {ex.Message}");
            }
        }

        public void GoButton_Click(object sender, RoutedEventArgs e, string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                BrowserView.Source = uriResult;
            }
            else
            {
                string searchUrl = $"{searchEngineUrl}{Uri.EscapeDataString(input)}";
                BrowserView.Source = new Uri(searchUrl);
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
                await BrowserView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
            }
        }

        private async void InitializeWebViewSettings()
        {
            await BrowserView.EnsureCoreWebView2Async();

            BrowserView.CoreWebView2.NewWindowRequested += (sender, args) =>
            {
                args.Handled = true;
                NavigateToBrowser(args.Uri.ToString());
            };

            BrowserView.NavigationStarting += (s, e) =>
            {
                if (!IsNavigatingThroughHistory)
                {
                    pendingUrl = e.Uri;
                }
            };

            BrowserView.NavigationCompleted += (s, e) =>
            {
                if (e.IsSuccess && pendingUrl != null)
                {
                    string currentUrl = BrowserView.Source.ToString();

                    // Avoid adding duplicate or reload entries into history
                    if (!IsNavigatingThroughHistory && lastCommittedUrl != currentUrl)
                    {
                        PushToHistory(currentUrl);
                    }

                    lastCommittedUrl = currentUrl;
                }
                pendingUrl = null;
                IsNavigatingThroughHistory = false;
            };
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string query = args.QueryText?.Trim();

            if (!string.IsNullOrWhiteSpace(query))
            {
                string url = GetFormattedUrl(query);
                NavigateToBrowser(url);
            }
        }

        private string GetFormattedUrl(string input)
        {
            if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
            {
                return input;
            }
            else if (input.Contains(".") && !input.Contains(" "))
            {
                return $"https://{input}";
            }
            else
            {
                return $"{searchEngineUrl}{Uri.EscapeDataString(input)}";
            }
        }

        private void NavigateToBrowser(string url)
        {
            try
            {
                BrowserView.Source = new Uri(url);
                ShowBrowserView();
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Navigation Error",
                    Content = $"Failed to navigate to {url}.\n\nError: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = errorDialog.ShowAsync();
            }
        }

        private void ShowBrowserView()
        {
            HomeScreenGrid.Visibility = Visibility.Collapsed;
            BrowserView.Visibility = Visibility.Visible;
        }

        private void ShowHomeScreen()
        {
            HomeScreenGrid.Visibility = Visibility.Visible;
            BrowserView.Visibility = Visibility.Collapsed;
        }

        private void PushToHistory(string url)
        {
            // Avoid duplicate back-to-back entries in history
            if (currentHistoryIndex >= 0 && navigationHistory[currentHistoryIndex] == url)
            {
                return;
            }

            if (currentHistoryIndex < navigationHistory.Count - 1)
            {
                navigationHistory.RemoveRange(currentHistoryIndex + 1, navigationHistory.Count - currentHistoryIndex - 1);
            }

            navigationHistory.Add(url);
            currentHistoryIndex = navigationHistory.Count - 1;

            Debug.WriteLine($"History: {string.Join(" -> ", navigationHistory)} | CurrentIndex: {currentHistoryIndex}");
        }

        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentHistoryIndex > 0)
            {
                currentHistoryIndex--;
                NavigateToHistoryEntry(currentHistoryIndex);
            }
        }

        public void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentHistoryIndex < navigationHistory.Count - 1)
            {
                currentHistoryIndex++;
                NavigateToHistoryEntry(currentHistoryIndex);
            }
        }

        private void NavigateToHistoryEntry(int index)
        {
            IsNavigatingThroughHistory = true;
            string entry = navigationHistory[index];

            if (entry == "HOME")
            {
                ShowHomeScreen();
            }
            else
            {
                BrowserView.Source = new Uri(entry);
                ShowBrowserView();
            }
        }

        public void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (navigationHistory[currentHistoryIndex] != "HOME")
            {
                PushToHistory("HOME");
                ShowHomeScreen();
            }
        }

        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserView.Visibility == Visibility.Visible)
            {
                BrowserView.Reload();
            }
        }

        public void Dispose()
        {
            BrowserView?.CoreWebView2?.Stop();
            BrowserView?.Close();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Settings",
                Content = new TextBlock { Text = "Settings dialog placeholder." },
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private void ToggleDateButton_Click(object sender, RoutedEventArgs e)
        {
            NtpTime.Visibility = NtpTime.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            NtpDate.Visibility = NtpDate.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
