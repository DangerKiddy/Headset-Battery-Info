﻿namespace HeadsetBatteryInfo
{
    public class Config
    {
        public int receiveMode { get; set; }
        public bool predictCharging { get; set; }
        public bool notifyOnHeadsetLowBattery { get; set; }
        public bool notifyOnControllerLowBattery { get; set; }
        public bool notifyOnHeadsetStopCharging { get; set; }
        public bool ovrToolkitSupport { get; set; }
        public bool xsOverlaySupport { get; set; }
        public bool enableOSC { get; set; }
        public int OSCport { get; set; }
        public int batteryDischargeMaximumTime { get; set; }
        public int lowBatteryNotifyLevel { get; set; }
        public bool enableRepetitiveNotification { get; set; }
        public int repetitiveMillisecondPeriod { get; set; }
    }
}
