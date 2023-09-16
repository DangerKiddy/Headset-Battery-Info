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

            public ImageSource charging;
        }

        public struct HeadsetIcons
        {
            public Icons headset;
            public Icons headsetCharging;

            public Icons leftController;
            public Icons rightController;
            public ImageSource companyLogo;
        }

        public static Color lowBattery = Color.FromArgb(255, 255, 95, 95);
        public static Color mediumBattery = Color.FromArgb(255, 252, 142, 0);
        public static Color highBattery = Color.FromArgb(255, 154, 209, 43);
        public static Color charging = Color.FromArgb(255, 27, 132, 212);

        public static HeadsetIcons Pico;
        public static HeadsetIcons Meta;

        public static HeadsetIcons GetCurrentDeviceIcons()
        {
            return Pico;
        }

        public static void Init()
        {
            InitPicoIcons();
        }
        private static void InitPicoIcons()
        {
            var headsetSource = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/headset.png"));
            var lControllerSource = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/left_controller.png"));
            var rControllerSource = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/right_controller.png"));

            var headsetChargingSource = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/headset_charging.png"));
            var lControllerChargingSource = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/left_controller_charging.png"));
            var rControllerChargingSource = ConvertBitmapToImageSource(new System.Drawing.Bitmap("Assets/Images/pico/right_controller_charging.png"));

            Pico.headset.highBattery = ChangeImageColor(headsetSource as BitmapSource, highBattery);
            Pico.headset.mediumBattery = ChangeImageColor(headsetSource as BitmapSource, mediumBattery);
            Pico.headset.lowBattery = ChangeImageColor(headsetSource as BitmapSource, lowBattery);

            Pico.headsetCharging.highBattery = ChangeImageColor(headsetChargingSource as BitmapSource, highBattery);
            Pico.headsetCharging.mediumBattery = ChangeImageColor(headsetChargingSource as BitmapSource, mediumBattery);
            Pico.headsetCharging.lowBattery = ChangeImageColor(headsetChargingSource as BitmapSource, lowBattery);

            Pico.headset.charging = ChangeImageColor(headsetChargingSource as BitmapSource, charging);

            Pico.leftController.highBattery = ChangeImageColor(lControllerSource as BitmapSource, highBattery);
            Pico.leftController.mediumBattery = ChangeImageColor(lControllerSource as BitmapSource, mediumBattery);
            Pico.leftController.lowBattery = ChangeImageColor(lControllerSource as BitmapSource, lowBattery);
            Pico.leftController.charging = ChangeImageColor(lControllerChargingSource as BitmapSource, charging);

            Pico.rightController.highBattery = ChangeImageColor(rControllerSource as BitmapSource, highBattery);
            Pico.rightController.mediumBattery = ChangeImageColor(rControllerSource as BitmapSource, mediumBattery);
            Pico.rightController.lowBattery = ChangeImageColor(rControllerSource as BitmapSource, lowBattery);
            Pico.rightController.charging = ChangeImageColor(rControllerChargingSource as BitmapSource, charging);
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
