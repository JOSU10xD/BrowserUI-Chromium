using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace BrowserUI.Controls
{
    public sealed partial class FluentIcon : CommunityToolkit.WinUI.UI.FontIconExtension
    {
        public FluentIcon()
        {
            FontFamily = (FontFamily)Application.Current.Resources["FluentIcons"];
        }
    }
}
