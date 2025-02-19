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
        public NewTab()
        {
            this.InitializeComponent();
        }
    }

}
