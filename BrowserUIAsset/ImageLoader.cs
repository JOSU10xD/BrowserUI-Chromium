using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BrowserUIAsset;
public class ImageLoader : MarkupExtension
{
    //set <Langversion> Latest </Langversion> in .csproj
    private static readonly Dictionary<string, BitmapImage> ImageCache = new();

    public string ImageName { get; set; }

    protected override object ProvideValue()
    {
        if (string.IsNullOrEmpty(ImageName))
        {
            return null;
        }
        if (ImageCache.TryGetValue(ImageName, out var cachedImage))
        {
            return cachedImage;
        }

        var uri = new Uri($"ms-appx:///Assets//Assets/{ImageName}");
        var image = new BitmapImage(uri);
        ImageCache[ImageName] = image;

        return image;
    }
}
