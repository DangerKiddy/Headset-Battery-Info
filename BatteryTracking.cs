using System;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
{
    public class BatteryTracking
    {
        protected bool isTeardown = false;

        private static long predictionLastBatteryChangeTime = 0;
        private static int predictionLastLevel = 0;
        private static long predictionLastTimeDifference = 0;

        public virtual async void Init()
        {
            await Task.Delay(1);
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

        protected static void PredictChargeState(int currentBatteryLevel)
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
                }

                if (predictionLastTimeDifference != 0 && !BatteryInfoReceiver.IsHeadsetCharging() && IsCharging(timeSinceLastChange, 1))
                {
                    BatteryInfoReceiver.OnReceiveBatteryState(true, DeviceType.Headset);
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

        public virtual void Terminate()
        {
            isTeardown = true;
        }
    }
}
