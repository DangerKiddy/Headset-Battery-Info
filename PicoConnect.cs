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
    internal class PicoConnect : BatteryTracking
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

        private const string fileName = "pico_connect*.log";
        private readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PICO Connect\\logs");

        private bool picoConnectAppFound = false;
        public override async void Init()
        {
            base.Init();

            while (!isTeardown)
            {
                var processes = Process.GetProcessesByName("PICO Connect");

                if (processes.Length > 0)
                {
                    MainWindow.Instance.SetCompany("pico");
                    picoConnectAppFound = true;

                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        protected override async Task Listen()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(3000));

            if (!CanListen())
                return;

            var batteryInfo = await GetBatteryInfo();
            foreach (var battery in batteryInfo)
            {
                if (Settings._config.predictCharging && battery.deviceType == DeviceType.Headset)
                    PredictChargeState(battery.percentage);

                if (battery.deviceType == DeviceType.ControllerLeft)
                {
                    BatteryInfoReceiver.OnReceiveBatteryLevel(battery.percentage, battery.side == Side.Left ? DeviceType.ControllerLeft : DeviceType.ControllerRight);
                }
                else
                {
                    BatteryInfoReceiver.OnReceiveBatteryLevel(battery.percentage, battery.deviceType);
                }

                SetBattery(battery.deviceType, battery.percentage, battery.deviceType == DeviceType.Headset ? -1 : 0);
            }
        }

        private bool CanListen()
        {
            if (isTeardown || !picoConnectAppFound)
                return false;

            return true;
        }

        private async Task<BatteryInfoCallback[]> GetBatteryInfo()
        {
            var batteryInfo = new BatteryInfoCallback[0];

            string file = GetLatestFileName(path, fileName);
            if (!File.Exists(file))
                return batteryInfo;

            await Task.Run(() =>
            {
                batteryInfo = ParseLog(file);
            });

            return batteryInfo;
        }

        private BatteryInfoCallback[] ParseLog(string fileName)
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

        private string GetLatestFileName(string path, string fileName)
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

        public override void Terminate()
        {
            base.Terminate();

            picoConnectAppFound = false;
        }
    }
}
