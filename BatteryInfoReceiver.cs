using System;

namespace HeadsetBatteryInfo
{
    internal static class BatteryInfoReceiver
    {
        private static int lowBatteryPercent = 25; // if battery percent below this value then notify about it

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
        private static bool previousHeadsetChargingState = false;
        public static void OnHeadsetBatteryStateChanged(bool isCharging)
        {
            isHeadsetCharging = isCharging;

            OnHeadsetBatteryLevelChanged(currentHeadsetLevel); // required for updating icon and letting vrc know about latest battery lvl

            if (!isCharging && Settings._config.notifyOnHeadsetStopCharging && previousHeadsetChargingState)
            {
                MainWindow.PlayBatteryStateSound();

                Overlay.SendNotification("Headset is not charging anymore!");
            }
            previousHeadsetChargingState = isCharging;
            
            OSC.SendBoolToVRC(OSC.vrcHeadsetBatteryStateAddress, isHeadsetCharging);
            Overlay.UpdateWristInfo(DeviceType.Headset, currentHeadsetLevel, isCharging);
        }

        public static bool IsHeadsetCharging()
        {
            return isHeadsetCharging;
        }

        private static bool notifiedAboutHeadsetLowBattery = false;
        public static void OnHeadsetBatteryLevelChanged(int level)
        {
            currentHeadsetLevel = level;
            MainWindow.SetDeviceBatteryLevel(DeviceType.Headset, currentHeadsetLevel, isHeadsetCharging);

            if (level < lowBatteryPercent && Settings._config.notifyOnHeadsetLowBattery)
            {
                if (!notifiedAboutHeadsetLowBattery)
                    NotifyLowBattery(DeviceType.Headset);

                notifiedAboutHeadsetLowBattery = true;
            }
            else
            {
                notifiedAboutHeadsetLowBattery = false;
            }

            OSC.SendFloatToVRC(OSC.vrcHeadsetBatteryLvlAddress, currentHeadsetLevel / 100f);
            Overlay.UpdateWristInfo(DeviceType.Headset, level, IsHeadsetCharging());
        }

        private static int currentControllerLeftLevel = 100;
        private static bool isControllerLeftCharging = false;
        public static void OnControllerLeftBatteryLevelChanged(int level)
        {
            currentControllerLeftLevel = level;
            MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerLeft, currentControllerLeftLevel, isControllerLeftCharging);

            CheckAndNotifyControllerLowBattery();

            OSC.SendFloatToVRC(OSC.vrcControllerLeftBatteryLvlAddress, currentControllerLeftLevel / 100f);
            Overlay.UpdateWristInfo(DeviceType.ControllerLeft, level, false);
        }

        private static int currentControllerRightLevel = 100;
        private static bool isControllerRightCharging = false;
        public static void OnControllerRightBatteryLevelChanged(int level)
        {
            currentControllerRightLevel = level;
            MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerRight, currentControllerRightLevel, isControllerRightCharging);

            CheckAndNotifyControllerLowBattery();

            OSC.SendFloatToVRC(OSC.vrcControllerRightBatteryLvlAddress, currentControllerRightLevel / 100f);
            Overlay.UpdateWristInfo(DeviceType.ControllerRight, level, false);
        }

        private static bool notifiedAboutControllerLowBattery = false;
        private static void CheckAndNotifyControllerLowBattery()
        {
            var lowestLevel = Math.Min(currentControllerRightLevel, currentControllerLeftLevel);

            if (lowestLevel < lowBatteryPercent && Settings._config.notifyOnControllerLowBattery)
            {
                if (!notifiedAboutControllerLowBattery)
                    NotifyLowBattery(DeviceType.ControllerLeft);

                notifiedAboutControllerLowBattery = true;
            }
            else
            {
                notifiedAboutControllerLowBattery = false;
            }
        }

        private static void NotifyLowBattery(DeviceType device)
        {
            MainWindow.PlayBatteryStateSound();

            Overlay.SendNotification(device == DeviceType.Headset ? "Headset has low battery!" : "Controller has low battery!");
        }
    }
}
