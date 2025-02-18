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
using BrowserUICore.ViewModel;
using Windows.ApplicationModel.UserDataAccounts.Provider;
using Microsoft.UI.Xaml.Markup;
using BrowserUICore.Models;
using BrowserUIMultiCore;
using Settings = BrowserUICore.Models.Settings;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI;
using Windows.Services.Store;
using System.Net.Http;
using System.ComponentModel.DataAnnotations;


namespace BrowserUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewTab : Page
    {
        //important bools
        bool isAuto;
        bool isMode;
        bool isNtp;
        //HomeViewModel
        public HomeViewModel HomeViewModel { get; set; }

        public NewTab()
        {
            this.InitializeComponent();
            HomeSync();
            this.Loaded += NewTab_Loaded;
        }

        BrowserUIMultiCore.Settings usersetts = UserFolderManager.LoadUserSettings(AuthService.CurrentUser);
        private void NewTab_Loaded(object sender, RoutedEventArgs e)
        {
            bool isNtp = usersetts.NtpDateTime == "1";
            DataTime.IsOn = isNtp;
            NtpEnable(isNtp);
        }

        public void HomeSync()
        {
            bool isAuto = usersetts.Auto == "1";
            Type.IsOn = isAuto;

            // Update the LightMode setting
            bool isMode = usersetts.LightMode == "1";
            Mode.IsOn = isMode;


            // Get Background and ColorBackground settings
            string backgroundSetting = usersetts.Background;
            string colorBackgroundSetting = usersetts.ColorBackground;
            string NtpColor = usersetts.NtpTextColor;

            var color = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), NtpColor);
            // ViewModel setup
            HomeViewModel = new HomeViewModel
            {
                BackgroundType = GetBackgroundType(backgroundSetting)
            };


            NtpTime.Foreground = NtpDate.Foreground = new SolidColorBrush(color);

            GridSelect.SelectedValue = HomeViewModel.BackgroundType.ToString();

            // Visibility setup based on LightMode setting
            SetVisibilityBasedOnLightMode(isMode);
        }

        private void SetVisibilityBasedOnLightMode(bool isLightMode)
        {
            NtpGrid.Visibility = isLightMode ? Visibility.Collapsed : Visibility.Visible;
            BigGrid.Visibility = isLightMode ? Visibility.Collapsed : Visibility.Visible;
        }
        private BrowserUICore.Models.Settings.NewTabBackground GetBackgroundType(string setting)
        {
            return setting switch
            {
                "2" => Settings.NewTabBackground.Costum,
                "1" => Settings.NewTabBackground.Featured,
                _ => Settings.NewTabBackground.None
            };
        }

        //copy this from the description
        public static Brush GetGridBackgroundAsync(Settings.NewTabBackground backgroundType, BrowserUIMultiCore.Settings usersettings)
        {
            string colorString = usersettings.ColorBackground.ToString();

            switch (backgroundType)
            {
                case Settings.NewTabBackground.None:
                    return new SolidColorBrush(Colors.Transparent);

                case Settings.NewTabBackground.Costum:
                    if (colorString == "#000000")
                    {
                        return new SolidColorBrush(Colors.Transparent);
                    }
                    else
                    {
                        var color = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), colorString);
                        return new SolidColorBrush(color);
                    }

                case Settings.NewTabBackground.Featured:
                    try
                    {
                        var client = new HttpClient();
                        var request = client.GetStringAsync(new Uri("http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1")).Result;

                        try
                        {
                            var images = System.Text.Json.JsonSerializer.Deserialize<ImageRoot>(request);
                            BitmapImage btpImg = new BitmapImage(new Uri("https://bing.com" + images.images[0].url));

                            // Use the downloaded image as a background
                            return new ImageBrush()
                            {
                                ImageSource = btpImg,
                                Stretch = Stretch.UniformToFill
                            };
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {
                        // Handle exceptions or return a default value
                    }
                    break;

            }

            return new SolidColorBrush();
        }

        private async void NtpEnable(bool isNtp)
        {
            while (isNtp)
            {
                await Task.Delay(100);

                if (NtpTime == null || NtpDate == null)
                {
                    break;
                }

                //update ui based on time and setts
                NtpTime.Visibility = NtpDate.Visibility = Visibility.Visible;
                NtpTime.Text = DateTime.Now.ToString("H:mm");
                NtpDate.Text = $"{DateTime.Today.DayOfWeek}, {DateTime.Today.ToString("MMMM d")}";
            }

            //dispose check
            if (NtpTime != null || NtpDate != null)
            {
                NtpTime.Visibility = NtpDate.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private class ImageRoot
        {
            public ImageTab[] images { get; set; }
        }

        private void Type_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                isAuto = toggleSwitch.IsOn;
                string AutoValue = isAuto ? "1" : "0";

                //the current user
                if (AuthService.CurrentUser != null)
                {
                    usersetts.Auto = AutoValue;
                    UserFolderManager.SaveUserSettings(AuthService.CurrentUser, usersetts);
                }
            }
            //copy for the toggle switches
        }

        private void Mode_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                isMode = toggleSwitch.IsOn;
                string AutoValue = isMode ? "1" : "0";

                //the current user
                if (AuthService.CurrentUser != null)
                {
                    usersetts.LightMode = AutoValue;
                    UserFolderManager.SaveUserSettings(AuthService.CurrentUser, usersetts);
                }
            }
        }

        private void DataTime_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                isNtp = toggleSwitch.IsOn;
                string AutoValue = isNtp ? "1" : "0";

                //the current user
                if (AuthService.CurrentUser != null)
                {
                    usersetts.NtpDateTime = AutoValue;
                    UserFolderManager.SaveUserSettings(AuthService.CurrentUser, usersetts);
                }
            }
        }

        private void GridSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = (sender as GridView).SelectedItem as GridViewItem;
            switch (selection.Tag)
            {
                case "None":
                    usersetts.Background = "0";
                    HomeViewModel.BackgroundType = Settings.NewTabBackground.None;

                    break;
                case "Featured":
                    usersetts.Background = "1";
                    HomeViewModel.BackgroundType = Settings.NewTabBackground.Featured;
                    break;
                case "Costum":
                    usersetts.Background = "2";
                    HomeViewModel.BackgroundType = Settings.NewTabBackground.Costum;
                    break;
            }
            UserFolderManager.SaveUserSettings(AuthService.CurrentUser, usersetts);
        }
    }
}

