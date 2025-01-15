using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.WebUI;
using Microsoft.Web.WebView2.Core;
using Windows.Graphics.Imaging;
using System.Net.Http;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.System;


namespace BrowserUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            WebView.Source = new Uri("https://www.google.com");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebView.CanGoBack)
            {
                WebView.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebView.CanGoForward)
            {
                WebView.GoForward();
            }
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WebView.Reload();
        }
        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            string input = UrlBox.Text;

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
        private void UrlBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (UrlBox.FindName("PART_EditableTextBox") is TextBox textBox)
            {
                textBox.TextChanged += UrlBox_TextChanged;
            }
        }
        private async void UrlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = UrlBox.Text;

            if (!string.IsNullOrWhiteSpace(query))
            {
                try
                {
                    using HttpClient client = new();
                    string url = $"https://suggestqueries.google.com/complete/search?client=firefox&q={Uri.EscapeDataString(query)}";
                    var response = await client.GetStringAsync(url);

                    // Parse JSON response
                    var suggestions = JsonSerializer.Deserialize<List<object>>(response);
                    if (suggestions != null && suggestions.Count > 1 && suggestions[1] is JsonElement suggestionArray)
                    {
                        var searchSuggestions = suggestionArray.EnumerateArray().Select(x => x.GetString()).Where(x => x != null).ToList();
                        UrlBox.ItemsSource = new ObservableCollection<string>(searchSuggestions);
                        UrlBox.IsDropDownOpen = true;
                    }
                }
                catch (Exception ex)
                {
                    // Handle errors (e.g., network issues)
                    Console.WriteLine($"Error fetching suggestions: {ex.Message}");
                }
            }
        }
        private async void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            var coreWebView2 = WebView.CoreWebView2;
            if (coreWebView2 == null)
            {
                await WebView.EnsureCoreWebView2Async();
                coreWebView2 = WebView.CoreWebView2;
            }
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.SuggestedFileName = "Screenshot";
            picker.FileTypeChoices.Add("PNG Image", new[] { ".png" });
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    await coreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Activate();
        }
        private void UrlBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                GoButton_Click(sender, e);
            }
        }
    }
}
public class MainViewModel : INotifyPropertyChanged
{
    private string _searchQuery;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
