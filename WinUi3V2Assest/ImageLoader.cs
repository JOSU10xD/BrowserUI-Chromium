using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;

namespace WinUi3V2Assest
{
    public class ImageLoader : MarkupExtension
    {
        public static readonly Dictionary<string, BitmapImage> ImageCache = new();

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

            var url = new Uri($"ms-appx:///Assets/{ImageName}");
            var image = new BitmapImage(url);
            ImageCache[ImageName] = image;  // Fix variable name
            return image;  // Fix variable name
        }
    }
}
