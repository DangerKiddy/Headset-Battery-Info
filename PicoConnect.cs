using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
{
    internal class PicoConnect
    {
        private enum Side
        {
            Left,
            Right
        }
        private class BatteryInfoCallback
        {
            public DeviceType deviceType { get; set; }
            public Side side { get; set; }
            public bool active { get; set; }
            public int percentage { get; set; }
        }

        private static readonly string fileName = "pico_connect*.log";
        private static readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PICO Connect\\logs");

        private static bool picoConnectAppFound = false;

        private static bool isTeardown = true;

        public static async void Init()
        {
            isTeardown = false;
            
            while (!isTeardown)
            {
                var processes = Process.GetProcessesByName("PICO Connect");

                if (processes.Length > 0)
                {
                    MainWindow.Instance.OnReceiveCompanyName("pico");
                    picoConnectAppFound = true;
                }
                else
                { 
                    picoConnectAppFound = false;
                    continue;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        public static void StartListening()
        {
            Listen();
        }

        private static async void Listen()
        {
            while (!isTeardown)
            {
                if (!picoConnectAppFound)
                    continue;

                string file = GetLatestFileName(path, fileName);
                if (!File.Exists(file))
                    continue;

                var batteryInfo = await Task.Run(BatteryInfoCallback[]() =>
                {
                    return ParseLog(file);
                });

                if (batteryInfo.Length == 0)
                    continue;

                foreach (var battery in batteryInfo)
                {
                    if (Settings._config.predictCharging && battery.deviceType == DeviceType.Headset)
                        StreamingAssistant.PredictChargeState(battery.percentage);

                    if (battery.deviceType == DeviceType.ControllerLeft)
                    {
                        MainWindow.Instance.OnReceiveBatteryLevel(battery.percentage, battery.side == Side.Left ? DeviceType.ControllerLeft : DeviceType.ControllerRight);
                    }
                    else
                    {
                        MainWindow.Instance.OnReceiveBatteryLevel(battery.percentage, battery.deviceType);
                    }
                }


                await Task.Delay(TimeSpan.FromMilliseconds(3000));
            }
        }

        private static BatteryInfoCallback[] ParseLog(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BatteryInfoCallback[] percentage = new BatteryInfoCallback[3];
                StreamReader sr = new StreamReader(fs);
                string[] strings = sr.ReadToEnd().Split('\n');

                string pattern = "(.*?) \\[info\\]  update battery info callback, battery_info: (.+)\\r";
                for (int i = strings.Length-1; i > 0; i--)
                {
                    string s = strings[i];

                    if (s.Contains("update battery info callback"))
                    {
                        var res = Regex.Match(s, pattern).Result("$2");
                        BatteryInfoCallback[] info = JsonSerializer.Deserialize<BatteryInfoCallback[]>(res);

                        return info;
                    }
                }
                
                return percentage;
            }
        }

        private class FileTimeInfo
        {
            public string FileName;
            public DateTime FileCreateTime;
        }

        private static string GetLatestFileName(string path, string fileName)
        {
            DirectoryInfo d = new DirectoryInfo(path);
            List<FileTimeInfo> list = new List<FileTimeInfo>();

            foreach (FileInfo file in d.GetFiles(fileName))
            {
                list.Add(new FileTimeInfo()
                {
                    FileName = file.FullName,
                    FileCreateTime = file.CreationTime
                });
            }
            var qry = from x in list
                      orderby x.FileCreateTime
                      select x;

            return qry.LastOrDefault().FileName;
        }

        public static void Terminate()
        {
            isTeardown = true;
        }
    }
}
