using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
using Windows.Media.Capture;
using Windows.Media.Core;
using ZXing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace qrcode
{
    public sealed partial class MainWindow : Window
    {
        private MediaCapture? _capture;
        private MediaFrameReader? _frameReader;
        private MediaSource? _mediaSource;
        private readonly SoftwareBitmapBarcodeReader _reader;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize ZXing barcode reader
            _reader = new SoftwareBitmapBarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                    TryHarder = true
                }
            };
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_capture == null)
            {
                await InitializeCaptureAsync();
            }
            else
            {
                await TerminateCaptureAsync();
            }
        }

        private async Task InitializeCaptureAsync()
        {
            try
            {
                var sourceGroup = (await MediaFrameSourceGroup.FindAllAsync())?.FirstOrDefault();
                if (sourceGroup == null)
                {
                    textBox.Text = "No compatible camera found.";
                    return;
                }

                _capture = new MediaCapture();
                await _capture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    SourceGroup = sourceGroup,
                    SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu
                });

                var source = _capture.FrameSources[sourceGroup.SourceInfos[0].Id];
                _frameReader = await _capture.CreateFrameReaderAsync(source, Windows.Media.MediaProperties.MediaEncodingSubtypes.Bgra8);
                _frameReader.FrameArrived += OnFrameArrived;

                await _frameReader.StartAsync();
                _mediaSource = MediaSource.CreateFromMediaFrameSource(source);
                player.Source = _mediaSource;
            }
            catch (Exception ex)
            {
                textBox.Text = $"Error initializing capture: {ex.Message}";
            }
        }

        private async void OnFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            using var frame = sender.TryAcquireLatestFrame();
            var bitmap = frame?.VideoMediaFrame?.SoftwareBitmap;
            if (bitmap == null) return;

            var result = _reader.Decode(bitmap);
            if (result != null && Uri.TryCreate(result.Text, UriKind.Absolute, out var uriResult))
            {
                // Launch the browser to open the link
                await Windows.System.Launcher.LaunchUriAsync(uriResult);

                DispatcherQueue.TryEnqueue(() =>
                {
                    textBox.Text = $"Opening: {result.Text}";
                });
            }
        }

        private async Task TerminateCaptureAsync()
        {
            player.Source = null;
            _mediaSource?.Dispose();
            _mediaSource = null;

            if (_frameReader != null)
            {
                _frameReader.FrameArrived -= OnFrameArrived;
                await _frameReader.StopAsync();
                _frameReader.Dispose();
                _frameReader = null;
            }

            _capture?.Dispose();
            _capture = null;

            textBox.Text = "Capture stopped.";
        }
    }

    public class SoftwareBitmapBarcodeReader : BarcodeReader<SoftwareBitmap>
    {
        public SoftwareBitmapBarcodeReader()
            : base(bitmap => new SoftwareBitmapLuminanceSource(bitmap))
        {
        }
    }

    public class SoftwareBitmapLuminanceSource : BaseLuminanceSource
    {
        public SoftwareBitmapLuminanceSource(SoftwareBitmap bitmap)
            : base(bitmap.PixelWidth, bitmap.PixelHeight)
        {
            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Gray8)
            {
                using var convertedBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
                SetLuminancesFromBitmap(convertedBitmap);
            }
            else
            {
                SetLuminancesFromBitmap(bitmap);
            }
        }

        private void SetLuminancesFromBitmap(SoftwareBitmap bitmap)
        {
            var buffer = new byte[bitmap.PixelWidth * bitmap.PixelHeight];
            bitmap.CopyToBuffer(buffer.AsBuffer());
            luminances = buffer;
        }

        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            var source = new SoftwareBitmapLuminanceSource(width, height);
            source.SetLuminances(newLuminances);
            return source;
        }

        private SoftwareBitmapLuminanceSource(int width, int height)
            : base(width, height)
        {
        }

        private void SetLuminances(byte[] newLuminances)
        {
            luminances = newLuminances;
        }
    }
}
