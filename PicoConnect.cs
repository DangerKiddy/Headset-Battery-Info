using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
{
    internal class PicoConnect
    {
        private static readonly long batteryDischargeMaximumTime = 70000;
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
                int[] batteries = await Task.Run(int[]() =>
                {
                    return ParseLog(file);
                });
                if (batteries.Length == 0)
                    continue;

                MainWindow.SetDeviceBatteryLevel(DeviceType.Headset, batteries[2], false);
                MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerLeft, batteries[0], false);
                MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerRight, batteries[1], false);

                MainWindow.Instance.OnReceiveBatteryLevel(batteries[2], DeviceType.Headset);
                MainWindow.Instance.OnReceiveBatteryLevel(batteries[0], DeviceType.ControllerLeft);
                MainWindow.Instance.OnReceiveBatteryLevel(batteries[1], DeviceType.ControllerRight);

                await Task.Delay(TimeSpan.FromMilliseconds(3000));
            }
        }

        private static int[] ParseLog(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int[] percentage = new int[3];
                StreamReader sr = new StreamReader(fs);
                string[] strings = sr.ReadToEnd().Split('\n');
                string pattern = "(.*?) \\[info\\]  update battery info callback, battery_info: \\[{\"deviceType\":1,\"side\":0,\"active\":(.*?),\"percentage\":(.*?)},{\"deviceType\":1,\"side\":1,\"active\":(.*?),\"percentage\":(.*?)},{\"deviceType\":0,\"side\":-1,\"active\":true,\"percentage\":(.*?)}\\]";
                foreach (string s in strings)
                {
                    if (s.Contains("update battery info callback"))
                    {
                        try
                        {
                            int.TryParse(Regex.Match(s, pattern).Result("$3"), out percentage[0]);
                            int.TryParse(Regex.Match(s, pattern).Result("$5"), out percentage[1]);
                            int.TryParse(Regex.Match(s, pattern).Result("$6"), out percentage[2]);
                        }
                        catch (Exception ex)
                        {
                        }
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

        private static bool IsCharging(long timeSinceLastChange, int batteryLevelDifference)
        {
            return timeSinceLastChange >= batteryDischargeMaximumTime || batteryLevelDifference <= 0;
        }

        public static void Terminate()
        {
            isTeardown = true;
        }
    }
}
