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
            public Icons headset;
            public Icons headsetCharging;

            public Icons leftController;
            public Icons rightController;
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

        public static void Init()
        {
            InitDefaultIcons();
            InitPicoIcons();
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

            PaintIcons(ref Unknown, icons);
        }

        private static void InitPicoIcons()
        {
            CompanyImages icons = new CompanyImages();
            icons.headsetIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/headset.png")) as BitmapSource;
            icons.leftControllerIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/left_controller.png")) as BitmapSource;
            icons.rightControllerIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/right_controller.png")) as BitmapSource;

            icons.headsetChargingIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/headset_charging.png")) as BitmapSource;
            icons.leftControllerChargingIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/left_controller_charging.png")) as BitmapSource;
            icons.rightControllerChargingIcon = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/right_controller_charging.png")) as BitmapSource;

            PaintIcons(ref Pico, icons);
        }

        private static void PaintIcons(ref HeadsetIcons headsetIcons, CompanyImages icons)
        {
            headsetIcons.headset.highBattery = ChangeImageColor(icons.headsetIcon, highBattery);
            headsetIcons.headset.mediumBattery = ChangeImageColor(icons.headsetIcon, mediumBattery);
            headsetIcons.headset.lowBattery = ChangeImageColor(icons.headsetIcon, lowBattery);

            headsetIcons.headsetCharging.highBattery = ChangeImageColor(icons.headsetChargingIcon, highBattery);
            headsetIcons.headsetCharging.mediumBattery = ChangeImageColor(icons.headsetChargingIcon, mediumBattery);
            headsetIcons.headsetCharging.lowBattery = ChangeImageColor(icons.headsetChargingIcon, lowBattery);

            headsetIcons.leftController.highBattery = ChangeImageColor(icons.headsetIcon, highBattery);
            headsetIcons.leftController.mediumBattery = ChangeImageColor(icons.headsetIcon, mediumBattery);
            headsetIcons.leftController.lowBattery = ChangeImageColor(icons.headsetIcon, lowBattery);

            headsetIcons.rightController.highBattery = ChangeImageColor(icons.headsetIcon, highBattery);
            headsetIcons.rightController.mediumBattery = ChangeImageColor(icons.headsetIcon, mediumBattery);
            headsetIcons.rightController.lowBattery = ChangeImageColor(icons.headsetIcon, lowBattery);
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
