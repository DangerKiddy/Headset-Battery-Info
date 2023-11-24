using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;

namespace HeadsetBatteryInfo
{
    public enum DeviceType
    {
        Headset,
        ControllerLeft,
        ControllerRight,
    }

    public static class DeviceIcons
    {
        public struct Icons
        {
            public ImageSource highBattery;
            public ImageSource mediumBattery;
            public ImageSource lowBattery;
        }

        public struct HeadsetIcons
        {
            public ImageSource headset;
            public ImageSource headsetCharging;

            public ImageSource leftController;
            public ImageSource rightController;
            public ImageSource companyLogo;
        }
        private struct CompanyImages
        {
            public BitmapSource headsetIcon;
            public BitmapSource headsetChargingIcon;
            public BitmapSource leftControllerIcon;
            public BitmapSource leftControllerChargingIcon;
            public BitmapSource rightControllerIcon;
            public BitmapSource rightControllerChargingIcon;
        }

        private enum Company
        {
            Unknown = -1,

            Pico,
            Meta
        }

        public static Color lowBattery = Color.FromArgb(255, 255, 95, 95);
        public static Color mediumBattery = Color.FromArgb(255, 252, 142, 0);
        public static Color highBattery = Color.FromArgb(255, 154, 209, 43);
        public static Color charging = Color.FromArgb(255, 27, 132, 212);

        public static HeadsetIcons Unknown;
        public static HeadsetIcons Pico;
        public static HeadsetIcons Meta;

        private static SolidColorBrush blueSolid = new SolidColorBrush();
        private static SolidColorBrush greenSolid = new SolidColorBrush();
        private static SolidColorBrush yellowSolid = new SolidColorBrush();
        private static SolidColorBrush redSolid = new SolidColorBrush();
        private static SolidColorBrush greySolid = new SolidColorBrush();
        private static LinearGradientBrush blueGradientBrush = new LinearGradientBrush();
        private static LinearGradientBrush greenGradientBrush = new LinearGradientBrush();
        private static LinearGradientBrush yellowGradientBrush = new LinearGradientBrush();
        private static LinearGradientBrush redGradientBrush = new LinearGradientBrush();
        private static LinearGradientBrush greyGradientBrush = new LinearGradientBrush();

        private static Company headsetCompany;
        public static HeadsetIcons GetCurrentDeviceIcons()
        {
            switch (headsetCompany)
            {
                case Company.Pico:
                    return Pico;

                case Company.Meta:
                    return Meta;

                default:
                    return Unknown;
            }
        }

        public static void SetCompany(string company)
        {
            if (company == "pico")
                headsetCompany = Company.Pico;
            else if (company == "meta")
                headsetCompany = Company.Meta;
            else
                headsetCompany = Company.Unknown;
        }

        public static LinearGradientBrush GetBlueGradient() => blueGradientBrush;
        public static LinearGradientBrush GetGreenGradient() => greenGradientBrush;
        public static LinearGradientBrush GetYellowGradient() => yellowGradientBrush;
        public static LinearGradientBrush GetRedGradient() => redGradientBrush;
        public static LinearGradientBrush GetGreyGradient() => greyGradientBrush;
        public static SolidColorBrush GetBlueSolid() => blueSolid;
        public static SolidColorBrush GetGreenSolid() => greenSolid;
        public static SolidColorBrush GetYellowSolid() => yellowSolid;
        public static SolidColorBrush GetRedSolid() => redSolid;
        public static SolidColorBrush GetGreySolid() => greySolid;

        public static void Init()
        {
            InitDefaultIcons();
            InitPicoIcons();
            InitMetaIcons();
            InitGradient();
        }

        private static void InitDefaultIcons()
        {
            var controllerIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/unknown/controller.png")) as BitmapSource;
            var controllerChargingIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/unknown/controller_charging.png")) as BitmapSource;

            CompanyImages icons = new CompanyImages();
            icons.headsetIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/unknown/headset.png")) as BitmapSource;
            icons.leftControllerIcon = controllerIcon;
            icons.rightControllerIcon = controllerIcon;

            icons.headsetChargingIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/unknown/headset_charging.png")) as BitmapSource;
            icons.leftControllerChargingIcon = controllerChargingIcon;
            icons.rightControllerChargingIcon = controllerChargingIcon;

            Unknown.companyLogo = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/unknown/logo.png"));
        }
        private static void InitPicoIcons()
        {
            Pico.headset = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/headset.png")) as BitmapSource;
            Pico.leftController = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/left_controller.png")) as BitmapSource;
            Pico.rightController = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/right_controller.png")) as BitmapSource;

            Pico.headsetCharging = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/headset_charging.png")) as BitmapSource;
            Pico.companyLogo = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/logo.png"));
        }
        private static void InitMetaIcons()
        {
            Meta.headset = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/meta/headset.png")) as BitmapSource;
            Meta.leftController = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/meta/left_controller.png")) as BitmapSource;
            Meta.rightController = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/meta/right_controller.png")) as BitmapSource;

            Meta.headsetCharging = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/meta/headset_charging.png")) as BitmapSource;
            Meta.companyLogo = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/meta/logo.png"));
        }
        private static void InitGradient()
        {
            blueSolid.Color = Color.FromRgb(55, 152, 231);
            blueGradientBrush = new LinearGradientBrush();
            {
                GradientStop stop1 = new GradientStop(Color.FromRgb(34, 94, 206), 0);
                GradientStop stop2 = new GradientStop(Color.FromRgb(55, 152, 231), 1);
                blueGradientBrush.GradientStops.Add(stop1);
                blueGradientBrush.GradientStops.Add(stop2);
            }

            greenSolid.Color = Color.FromRgb(76, 199, 48);
            greenGradientBrush = new LinearGradientBrush();
            {
                GradientStop stop1 = new GradientStop(Color.FromRgb(24, 178, 79), 0);
                GradientStop stop2 = new GradientStop(Color.FromRgb(76, 199, 48), 1);
                greenGradientBrush.GradientStops.Add(stop1);
                greenGradientBrush.GradientStops.Add(stop2);
            }

            yellowSolid.Color = Color.FromRgb(199, 193, 48);
            yellowGradientBrush = new LinearGradientBrush();
            {
                GradientStop stop1 = new GradientStop(Color.FromRgb(178, 172, 24), 0);
                GradientStop stop2 = new GradientStop(Color.FromRgb(199, 193, 48), 1);
                yellowGradientBrush.GradientStops.Add(stop1);
                yellowGradientBrush.GradientStops.Add(stop2);
            }

            redSolid.Color = Color.FromRgb(199, 76, 48);
            redGradientBrush = new LinearGradientBrush();
            {
                GradientStop stop1 = new GradientStop(Color.FromRgb(178, 24, 79), 0);
                GradientStop stop2 = new GradientStop(Color.FromRgb(199, 76, 48), 1);
                redGradientBrush.GradientStops.Add(stop1);
                redGradientBrush.GradientStops.Add(stop2);
            }

            greySolid.Color = Color.FromRgb(100, 100, 100);
            greyGradientBrush = new LinearGradientBrush();
            {
                GradientStop stop1 = new GradientStop(Color.FromRgb(150, 150, 150), 0);
                GradientStop stop2 = new GradientStop(Color.FromRgb(100, 100, 100), 1);
                greyGradientBrush.GradientStops.Add(stop1);
                greyGradientBrush.GradientStops.Add(stop2);
            }
        }

        private static ImageSource ConvertBitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
            bitmapImage.EndInit();

            return bitmapImage;
        }
        private static ImageSource ChangeImageColor(BitmapSource originalImageSource, Color newColor)
        {
            int width = originalImageSource.PixelWidth;
            int height = originalImageSource.PixelHeight;

            WriteableBitmap writableBitmap = new WriteableBitmap(originalImageSource);
            writableBitmap.Lock();

            byte r = newColor.R;
            byte g = newColor.G;
            byte b = newColor.B;

            int stride = writableBitmap.BackBufferStride;

            unsafe
            {
                for (int y = 0; y < height; y++)
                {
                    byte* row = (byte*)writableBitmap.BackBuffer + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        byte* pixel = row + x * 4;

                        pixel[2] = r;
                        pixel[1] = g;
                        pixel[0] = b;
                    }
                }
            }

            writableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            writableBitmap.Unlock();

            return writableBitmap;
        }
    }
}
