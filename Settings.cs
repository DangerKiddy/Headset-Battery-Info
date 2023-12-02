using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HeadsetBatteryInfo
{
    internal class Settings
    {
        private static Dictionary<string, object> settingsValues = new Dictionary<string, object>();

        public const string Setting_UseStreamingApp = "useStreamingApp";
        public const string Setting_PredictHeadsetCharge = "predictCharging";

        public const string Setting_HeadsetNotifyLowBattery = "notifyOnHeadsetLowBattery";
        public const string Setting_ControllersNotifyLowBattery = "notifyOnControllerLowBattery";

        public const string Setting_HeadsetNotifyStopCharging = "notifyOnHeadsetStopCharging";

        public const string Setting_OVRToolkitNotification = "ovrToolkitSupport";
        public const string Setting_XSOverlayNotification = "xsOverlaySupport";

        public static T GetValue<T>(string key, T _default = default(T))
        {
            object o;
            if (!settingsValues.TryGetValue(key, out o))
               return _default;

            return (T)o;
        }
        public static void SetValue(string key, object value, bool noSave = false)
        {
            settingsValues[key] = value;

            if (!noSave)
                Save();
        }

        public static void Save()
        {
            string settings = "";

            foreach (KeyValuePair<string, object> kv in settingsValues)
            {
                settings += $"{kv.Key}: {kv.Value}\n";
            }

            File.WriteAllText("settings.txt", settings);
        }

        public static void Load()
        {
            if (!File.Exists("settings.txt"))
                return;

            var settings = File.ReadAllLines("settings.txt");

            foreach (var setting in settings)
            {
                var seperatorIndex = setting.IndexOf(':');
                if (seperatorIndex == -1)
                    continue;

                string key = setting.Substring(0, seperatorIndex).Trim();
                string value = setting.Substring(seperatorIndex + 1).Trim();

                if (value == "True" || value == "False")
                {
                    SetValue(key, bool.Parse(value), true);
                }
                else if (value.StartsWith("\""))
                {
                    value = value.Trim('"');
                    SetValue(key, value, true);
                }
                else if (value.Contains(','))
                {
                    SetValue(key, float.Parse(value), true);
                }
                else
                {
                    SetValue(key, int.Parse(value), true);
                }
            }
        }
    }
}
