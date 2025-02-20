using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserUI.Pages
{
    public interface IBrowserTab
    {
        void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e);
        void ForwardButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e);
        void RefreshButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e);
        void GoButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e, string input);
        void CaptureScreenshot(object sender, Microsoft.UI.Xaml.RoutedEventArgs e);
    }
}