namespace HeadsetBatteryInfo
{
    internal static class BatteryInfoReceiver
    {
        public static void OnReceiveBatteryLevel(int level, DeviceType device)
        {
            switch (device)
            {
                case DeviceType.Headset:
                    OnHeadsetBatteryLevelChanged(level);

                    break;

                case DeviceType.ControllerLeft:

                    break;

                case DeviceType.ControllerRight:

                    break;

                default:
                    break;
            }
        }

        public static void OnReceiveBatteryState(bool isCharging, DeviceType device)
        {
            isHeadsetCharging = isCharging;

            switch (device)
            {
                case DeviceType.Headset:
                    OnHeadsetBatteryStateChanged(isCharging);

                    break;

                case DeviceType.ControllerLeft:

                    break;

                case DeviceType.ControllerRight:

                    break;

                default:
                    break;
            }
        }

        private static bool isHeadsetCharging = false;
        public static void OnHeadsetBatteryStateChanged(bool isCharging)
        {
            isHeadsetCharging = isCharging;
        }

        public static void OnHeadsetBatteryLevelChanged(int level)
        {
            MainWindow.SetHeadsetText(level + "%");

            DeviceIcons.HeadsetIcons deviceIcons = DeviceIcons.GetCurrentDeviceIcons();
            DeviceIcons.Icons icons = deviceIcons.headset;
            if (isHeadsetCharging)
                icons = deviceIcons.headsetCharging;

            if (level > 50)
            {
                MainWindow.ChangeHeadsetIcon(icons.highBattery);
            }
            else if (level > 25)
            {
                MainWindow.ChangeHeadsetIcon(icons.mediumBattery);
            }
            else
            {
                MainWindow.ChangeHeadsetIcon(icons.lowBattery);
            }
        }
    }
}
