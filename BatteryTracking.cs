using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
{
    public class BatteryTracking
    {
        public struct BatteryInfo
        {
            public int Level;
            public bool IsCharging;
            public bool IsConnected;
        }

        protected bool isTeardown = false;

        private static long predictionLastBatteryChangeTime = 0;
        private static int predictionLastLevel = 0;
        private static long predictionLastTimeDifference = 0;

        private BatteryInfo currentHeadsetBatteryInfo;
        private BatteryInfo currentLeftControllerBatteryInfo;
        private BatteryInfo currentRightControllerBatteryInfo;

        private Dictionary<DeviceType, BatteryInfo> batteryInfos;

        public virtual async void Init()
        {
            batteryInfos = new Dictionary<DeviceType, BatteryInfo>
            {
                [DeviceType.Headset] = currentHeadsetBatteryInfo,
                [DeviceType.ControllerLeft] = currentLeftControllerBatteryInfo,
                [DeviceType.ControllerRight] = currentRightControllerBatteryInfo
            };
        }
        public virtual void StartListening()
        {
            Tick();
        }
        private async void Tick()
        {
            while (!isTeardown)
            {
                await Listen();
            }
        }
        protected virtual async Task Listen()
        {
            await Task.Delay(1000);
        }

        protected void PredictChargeState(int currentBatteryLevel)
        {
            var curTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (predictionLastLevel != 0)
            {
                var timeSinceLastChange = curTime - predictionLastBatteryChangeTime;
                if (predictionLastLevel != currentBatteryLevel)
                {
                    var batteryLevelDifference = predictionLastLevel - currentBatteryLevel;
                    predictionLastBatteryChangeTime = curTime;

                    predictionLastTimeDifference = timeSinceLastChange;

                    bool isCharging = IsCharging(timeSinceLastChange, batteryLevelDifference);

                    BatteryInfoReceiver.OnReceiveBatteryState(isCharging, DeviceType.Headset);
                    SetBattery(DeviceType.Headset, -1, isCharging);
                }

                if (predictionLastTimeDifference != 0 && !BatteryInfoReceiver.IsHeadsetCharging() && IsCharging(timeSinceLastChange, 1))
                {
                    BatteryInfoReceiver.OnReceiveBatteryState(true, DeviceType.Headset);
                    SetBattery(DeviceType.Headset, -1, true);
                }
            }
            else
            {
                predictionLastBatteryChangeTime = curTime;
            }

            predictionLastLevel = currentBatteryLevel;
        }

        public static bool IsCharging(long timeSinceLastChange, int batteryLevelDifference)
        {
            return timeSinceLastChange >= Settings._config.batteryDischargeMaximumTime || batteryLevelDifference <= 0;
        }

        protected void SetBattery(DeviceType device, int level = -1, int isCharging = -1)
        {
            if (batteryInfos.TryGetValue(device, out BatteryInfo info))
            {
                info.IsConnected = true;

                if (level != -1)
                    info.Level = level;

                if (isCharging != -1)
                    info.IsCharging = isCharging == 0 ? false : true;
            }
        }

        protected void SetBattery(DeviceType device, int level, bool isCharging)
        {
            SetBattery(device, level, Convert.ToInt32(isCharging));
        }

        public void ForceSendData()
        {
            foreach (var kv in batteryInfos)
            {
                var device = kv.Key;
                var info = kv.Value;

                if (info.IsConnected)
                {
                    BatteryInfoReceiver.OnReceiveBatteryLevel(info.Level, device);
                    BatteryInfoReceiver.OnReceiveBatteryState(info.IsCharging, device);
                }
            }
        }

        public virtual void Terminate()
        {
            isTeardown = true;
        }
    }
}
