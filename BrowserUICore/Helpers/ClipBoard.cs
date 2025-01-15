using Windows.ApplicationModel.DataTransfer;

namespace BrowserUICore.Helpers
{
    public class ClipBoard
    {
        public static void WriteStringToClipboard(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "";
            }

            var dataPackage = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
    }
}
