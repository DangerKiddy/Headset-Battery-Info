using System;
using System.IO;
using System.Text.Json;

namespace HeadsetBatteryInfo
{
    internal class Settings
    {
        public static Config _config = new Config
        {
            receiveMode = 0,
            predictCharging = false,
            notifyOnControllerLowBattery = false,
            notifyOnHeadsetLowBattery = false,
            notifyOnHeadsetStopCharging = true,
            ovrToolkitSupport = false,
            xsOverlaySupport = false,
            enableOSC = true,
            OSCport = 9000,
            batteryDischargeMaximumTime = 70000,
            lowBatteryNotifyLevel = 30,
            enableRepetitiveNotification = false,
            repetitiveMillisecondPeriod = 600000,
        };

        private static readonly string filePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

        public static void Save()
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void Load()
        {
            if (!File.Exists(filePath))
                CreateNewConfig();

            try
            {
                _config = JsonSerializer.Deserialize<Config>(File.ReadAllText(filePath));

                if (_config.repetitiveMillisecondPeriod == 0)
                    _config.repetitiveMillisecondPeriod = 600000;
            }
            catch
            {
                CreateNewConfig();
            }
        }

        private static void CreateNewConfig()
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
