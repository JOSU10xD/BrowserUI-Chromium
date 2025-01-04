using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

namespace BrowserUI
{
    public sealed partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            this.InitializeComponent();
        }

        private void ChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker colorPicker = new();
            colorPicker.ColorChanged += (s, args) =>
            {
                App.HomepageBackgroundColor = args.NewColor;
                App.HomepageBackgroundImage = null;
            };

            Flyout flyout = new Flyout
            {
                Content = colorPicker,
                Placement = FlyoutPlacementMode.Full
            };
            flyout.ShowAt((FrameworkElement)sender);
        }

        private async void AddBackgroundImage_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                App.HomepageBackgroundImage = new Uri(file.Path);
                App.HomepageBackgroundColor = null;
            }
        }

        private void ResetHomepageSettings_Click(object sender, RoutedEventArgs e)
        {
            App.HomepageBackgroundImage = null;
            App.HomepageBackgroundColor = Colors.LightCyan;  // Default color
        }

        private void SearchEngineSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            App.DefaultSearchEngine = selectedItem.Content.ToString();
        }
    }
}