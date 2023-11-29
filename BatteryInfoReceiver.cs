using System;

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
                    OnControllerLeftBatteryLevelChanged(level);

                    break;

                case DeviceType.ControllerRight:
                    OnControllerRightBatteryLevelChanged(level);

                    break;

                default:
                    break;
            }
        }

        public static void OnReceiveBatteryState(bool isCharging, DeviceType device)
        {
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

        private static int currentHeadsetLevel = 100;
        private static bool isHeadsetCharging = false;
        public static void OnHeadsetBatteryStateChanged(bool isCharging)
        {
            isHeadsetCharging = isCharging;

            OnHeadsetBatteryLevelChanged(currentHeadsetLevel); // required for updating icon and letting vrc know about latest battery lvl

            if (!isCharging && Settings.GetValue<bool>(Settings.Setting_HeadsetNotifyLowBattery, true))
            {
                MainWindow.PlayBatteryStateSound();
            }

            OSC.SendBoolToVRC(OSC.vrcHeadsetBatteryStateAddress, isHeadsetCharging);
        }

        public static bool IsHeadsetCharging()
        {
            return isHeadsetCharging;
        }

        public static void OnHeadsetBatteryLevelChanged(int level)
        {
            currentHeadsetLevel = level;
            MainWindow.SetDeviceBatteryLevel(DeviceType.Headset, currentHeadsetLevel, isHeadsetCharging);

            OSC.SendFloatToVRC(OSC.vrcHeadsetBatteryLvlAddress, currentHeadsetLevel / 100f);
        }

        private static int currentControllerLeftLevel = 100;
        private static bool isControllerLeftCharging = false;
        public static void OnControllerLeftBatteryLevelChanged(int level)
        {
            currentControllerLeftLevel = level;
            MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerLeft, currentControllerLeftLevel, isControllerLeftCharging);

            OSC.SendFloatToVRC(OSC.vrcControllerLeftBatteryLvlAddress, currentControllerLeftLevel / 100f);
        }

        private static int currentControllerRightLevel = 100;
        private static bool isControllerRightCharging = false;
        public static void OnControllerRightBatteryLevelChanged(int level)
        {
            currentControllerRightLevel = level;
            MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerRight, currentControllerRightLevel, isControllerRightCharging);

            OSC.SendFloatToVRC(OSC.vrcControllerRightBatteryLvlAddress, currentControllerRightLevel / 100f);
        }
    }
}
