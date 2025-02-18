using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Security.DataProtection;

namespace BrowserUIMultiCore;

public static class UserDataManager
{
    // the values for the data manager
    public static readonly string DocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static readonly string CoreFolderPath = Path.Combine(DocumentsFolder, "WinUi3UserCore");
    public static readonly string UsersFolderPath = "Users";
    public static readonly string CoreFileName = "UserCore.json";

    public static UserDataResult LoadUsers()
    {
        string coreFilePath = Path.Combine(CoreFolderPath, CoreFileName);

        if(!File.Exists(coreFilePath))
        {
            return new UserDataResult
            {
                Users = new List<User>(),
                CurrentUsername = string.Empty,
            };
        }

        string coreJson = File.ReadAllText(coreFilePath);
        var users = JsonSerializer.Deserialize<List<User>>(coreJson);

        string currentUsername = GetCurrentUsername();

        return new UserDataResult
        { 
            Users = users,
            CurrentUsername = currentUsername,
        };
    }

    public static string GetCurrentUsername()
    {
        if (AuthService.IsUserAuthenticated)
        {
            return AuthService.CurrentUser.Username;
        }

        return "Geust";
    }

    public static void SaveUsers(List<User> users)
    {
        if(!Directory.Exists(CoreFolderPath))
        {
            Directory.CreateDirectory(CoreFolderPath);
        }

        string coreFilePath = Path.Combine(CoreFolderPath, CoreFileName);
        string coreJson = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(coreFilePath, coreJson);
    }
}
