using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System;
using System.Windows;

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
        public struct HeadsetIcons
        {
            public ImageSource headset;
            public ImageSource headsetCharging;

            public ImageSource leftController;
            public ImageSource rightController;
            public ImageSource companyLogo;
        }

        public enum Company
        {
            Unknown = -1,

            Pico,
            Meta
        }

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

        public static Company GetCompany()
        {
            return headsetCompany;
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
            InitFolder("unknown", true, ref Unknown);

            InitFolder("pico", false, ref Pico);
            InitFolder("meta", false, ref Meta);

            InitGradient();
        }

        private static void InitFolder(string folder, bool singleControllerImage, ref HeadsetIcons icons)
        {
            var leftControllerName = singleControllerImage ? "controller" : "left_controller";
            var rightControllerName = singleControllerImage ? "controller" : "right_controller";

            icons.headset = ConvertBitmapToImageSource(GetImageBitmap($"Assets/Images/{folder}/headset.png")) as BitmapSource;
            icons.leftController = ConvertBitmapToImageSource(GetImageBitmap($"Assets/Images/{folder}/{leftControllerName}.png")) as BitmapSource;
            icons.rightController = ConvertBitmapToImageSource(GetImageBitmap($"Assets/Images/{folder}/{rightControllerName}.png")) as BitmapSource;

            icons.headsetCharging = ConvertBitmapToImageSource(GetImageBitmap($"Assets/Images/{folder}/headset_charging.png")) as BitmapSource;
            icons.companyLogo = ConvertBitmapToImageSource(GetImageBitmap($"Assets/Images/{folder}/logo.png"));
        }

        private static System.Drawing.Bitmap GetImageBitmap(string path)
        {
            return new System.Drawing.Bitmap(GetImageFileStream(path));
        }

        private static Stream GetImageFileStream(string path)
        {
            System.Windows.Resources.StreamResourceInfo res = Application.GetResourceStream(new Uri(path, UriKind.RelativeOrAbsolute));
            return res.Stream;
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
    }
}
