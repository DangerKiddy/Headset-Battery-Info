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
            lowBatteryNotifyLevel = 30
        };

        private static readonly string fileName = "settings.json";

        public static void Save()
        {
            File.WriteAllText(fileName, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void Load()
        {
            if (!File.Exists(fileName))
                CreateNewConfig();

            try
            {
                _config = JsonSerializer.Deserialize<Config>(File.ReadAllText(fileName));
            }
            catch
            {
                CreateNewConfig();
            }
        }

        private static void CreateNewConfig()
        {
            File.WriteAllText(fileName, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
