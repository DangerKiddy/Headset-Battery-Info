using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
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

    }
}
