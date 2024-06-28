using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage.Pickers.Provider;

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
            notifyOnHeadsetStopCharging = false,
            ovrToolkitSupport = false,
            xsOverlaySupport = false,
            enableOSC = true,
            OSCport = 9000
        };

        private static readonly string fileName = "settings.json";

        public static void Save()
        {
            File.WriteAllText(fileName, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void Load()
        {
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
                File.WriteAllText(fileName, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
            }

            _config = JsonSerializer.Deserialize<Config>(File.ReadAllText(fileName));
        }
    }
}
