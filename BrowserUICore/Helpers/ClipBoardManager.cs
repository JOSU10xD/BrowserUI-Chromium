using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace BrowserUICore.Helpers
{
    public static class ClipboardManager
    {
        private static readonly string FilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ClipboardSnippets.json");

        // Load saved snippets
        public static List<string> LoadSnippets()
        {
            if (!File.Exists(FilePath))
                return new List<string>();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        // Save snippets to file
        public static void SaveSnippets(List<string> snippets)
        {
            string json = JsonSerializer.Serialize(snippets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        // Copy snippet to clipboard
        public static void CopyToClipboard(string text)
        {
            var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
    }
}
