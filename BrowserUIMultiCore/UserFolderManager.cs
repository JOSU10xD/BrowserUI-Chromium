using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrowserUIMultiCore;

public static  class UserFolderManager
{
    public static void CreateUserFolders(User user)
    {
        string userFolderPath = Path.Combine(UserDataManager.CoreFolderPath, UserDataManager.UsersFolderPath, user.Username);

        Directory.CreateDirectory(userFolderPath);
        Directory.CreateDirectory(Path.Combine(userFolderPath, "Settings"));
        Directory.CreateDirectory(Path.Combine(userFolderPath, "Database"));
        Directory.CreateDirectory(Path.Combine(userFolderPath, "Browser"));
        Directory.CreateDirectory(Path.Combine(userFolderPath, "Modules"));

        //classes coming up next
        CreateSettingsFile(user.Username);
        CreateDatabaseFile(user.Username);
    }

    public static void CreateSettingsFile(string username)
    {
        //values for the creating
        string userFolderPath = Path.Combine(UserDataManager.CoreFolderPath, UserDataManager.UsersFolderPath, username);
        string settingsFolderPath = Path.Combine(userFolderPath, "Settings");
        string settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");

        Directory.CreateDirectory(settingsFolderPath);

        //values are in description
        var settings = new Settings
        {
            DisableJavaScript = "0",
            DisablePassSave = "0",
            DisableWebMess = "0",
            DisableGenAutoFill = "0",
            ColorBackground = "#000000",
            StatusBar = "1",
            BrowserKeys = "1",
            BrowserScripts = "1",
            Useragent = "WebView",
            LightMode = "0",
            OpSw = "1",
            EngineFriendlyName = "Google",
            SearchUrl = "https://www.google.com/search?q=",
            ColorTool = "#000000",
            ColorTV = "#000000",
            Background = "1",
            Auto = "0",
            Lang = "nl-NL",
            ReadButton = "1",
            AdblockBtn = "1",
            Downloads = "1",
            Translate = "1",
            Favorites = "1",
            Historybtn = "1",
            QrCode = "1",
            FavoritesL = "1",
            ToolIcon = "1",
            DarkIcon = "1",
            OpenTabHandel = "0",
            NtpDateTime = "0",
            ExitDialog = "0",
            NtpTextColor = "#000000",
        };

        string jsonString = System.Text.Json.JsonSerializer.Serialize(settings);
        File.WriteAllText(settingsFilePath, jsonString);
    }

    public static void CreateDatabaseFile(string username)
    {
        string userFolderPath = Path.Combine(UserDataManager.CoreFolderPath, UserDataManager.UsersFolderPath, username);
        string DatabaseFolderPath = Path.Combine(userFolderPath, "Database");
        string DatabaseFilePath = Path.Combine(DatabaseFolderPath, "History.db");

        Directory.CreateDirectory(DatabaseFolderPath);

        using (var connection = new SqliteConnection($"Data Source={DatabaseFilePath}"))
        {
            connection.Open();

            using (var transaction = connection.BeginTransaction())
            {
                var craeteTableCommand = connection.CreateCommand();

                craeteTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS urls (" + "id INTEGER PRIMARY KEY," + "last_visit_time TEXT," + "url TEXT," + "title TEXT," + "visit_count INTEGER NOT NULL DEFAULT 0," +
                    "typed_count INTEGER NOT NULL DEFAULT 0," + "hidden INTEGER NOT NULL DEFAULT 0" + ")";

                craeteTableCommand.ExecuteNonQuery();

                transaction.Commit();
            }

            Console.WriteLine("Sqlite database and 'urls' table created Success.");
        }
    }

    //now save and load settings 

    public static Settings LoadUserSettings(User user)
    {
        string settingsFilePath = Path.Combine(UserDataManager.CoreFolderPath, UserDataManager.UsersFolderPath, user.Username, "Settings", "settings.json");

        if(File.Exists(settingsFilePath))
        {
            string settingsjson = File.ReadAllText(settingsFilePath);
            return System.Text.Json.JsonSerializer.Deserialize<Settings>(settingsjson);
        }

        return new Settings();
    }

    public static void SaveUserSettings(User user, Settings settings)
    {
        try
        {
            string settingsFilePath = Path.Combine(UserDataManager.CoreFolderPath, UserDataManager.UsersFolderPath, user.Username, "Settings", "settings.json");
            string settingsjson = System.Text.Json.JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(settingsFilePath, settingsjson);
        }
        catch (Exception ex) 
        {
            Debug.WriteLine("Error Saving Data");
        }
    }
}
