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
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BrowserUIMultiCore;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BrowserUISetup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupUser : Page
    {
        public SetupUser()
        {
            this.InitializeComponent();
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            await CreateUserOnStartup();
            Thread.Sleep(500);
            Frame.Navigate(typeof(SetupUi));
        }

        private async Task CreateUserOnStartup()
        {
            // Create a new user object with a unique username.
            User newUser = new User
            {
                Username = UserName.Text, // Generate a unique username.                                                              // Add other user properties as needed.
            };

            // Create a list of users and add the new user to it.
            List<User> users = new List<User>();
            users.Add(newUser);

            // Create the user folders.
            UserFolderManager.CreateUserFolders(newUser);

            // Save the list of users to the JSON file.
            UserDataManager.SaveUsers(users);
            //AuthService.InitAuthService();
            // Authenticate the new user (if needed).
            AuthService.Authenticate(newUser.Username);
        }
    }
}
