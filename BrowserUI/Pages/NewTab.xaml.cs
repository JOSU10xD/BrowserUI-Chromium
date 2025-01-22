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


namespace BrowserUI.Pages
{
    public partial class NewTab : Page
    {
        public NewTab()
        {
            this.InitializeComponent();
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
    }
}
