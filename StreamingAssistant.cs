using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
{
    internal class StreamingAssistant
    {
        private static long batteryDischargeMaximumTime = 70000;

        private static Process streamingAssistant;
        private static IntPtr clientManagerPtr;
        private static IntPtr streamingAssistantHandle;
        private static IntPtr THREADSTACK0;

        private static int[] offsets = { 0x28, 0x8, 0x10, 0x18, 0x8, 0x10 };
        private static int headsetBatteryOffset = 0x64;
        private static int leftControllerBatteryOffset = 0x68;
        private static int rightControllerBatteryOffset = 0x6C;
        
        private static bool isActive = false;

        private enum ControllerBatteryLevel
        {
            NotConnected = 0,
            VeryLow = 1,
            Low = 2,
            Half = 3,
            Okay = 4,
            Full = 5
        }

        private static Dictionary<ControllerBatteryLevel, int> batteryEnumToLevel = new Dictionary<ControllerBatteryLevel, int>()
        {
            [ControllerBatteryLevel.NotConnected] = 0,
            [ControllerBatteryLevel.VeryLow] = 20,
            [ControllerBatteryLevel.Low] = 40,
            [ControllerBatteryLevel.Half] = 60,
            [ControllerBatteryLevel.Okay] = 80,
            [ControllerBatteryLevel.Full] = 100,
        };

        public static int GetHeadsetBattery()
        {
            return ReadInt32(clientManagerPtr + headsetBatteryOffset);
        }

        private static int ConvertControllerBatteryToPercent(ControllerBatteryLevel batteryLevel)
        {
            int percent;

            if (!batteryEnumToLevel.TryGetValue(batteryLevel, out percent))
                percent = 0;

            return percent;
        }

        private static ControllerBatteryLevel GetControllerBatteryState(DeviceType controller)
        {
            if (controller == DeviceType.ControllerLeft)
                return (ControllerBatteryLevel)ReadInt32(clientManagerPtr + leftControllerBatteryOffset);
            else
                return (ControllerBatteryLevel)ReadInt32(clientManagerPtr + rightControllerBatteryOffset);
        }

        public static int GetLeftControllerBattery()
        {
            ControllerBatteryLevel batteryState = GetControllerBatteryState(DeviceType.ControllerLeft);
            int batteryLevel = ConvertControllerBatteryToPercent(batteryState);

            return batteryLevel;
        }

        public static int GetRightControllerBattery()
        {
            ControllerBatteryLevel batteryState = GetControllerBatteryState(DeviceType.ControllerRight);
            int batteryLevel = ConvertControllerBatteryToPercent(batteryState);

            return batteryLevel;
        }

        public static async void Init()
        {
            isActive = true;

            bool streamingAppFound = false;
            while (!streamingAppFound)
            {
                if (!isActive || !MainWindow.IsRunAsAdmin())
                    break;

                var processes = Process.GetProcessesByName("Streaming Assistant");

                if (processes.Length > 0)
                {
                    streamingAssistant = processes[0];
                    streamingAssistantHandle = streamingAssistant.Handle;

                    var threadstack0 = await getThread0Address(streamingAssistant);
                    THREADSTACK0 = threadstack0;

                    UpdateClientManagerPointer(threadstack0);

                    MainWindow.Instance.OnReceiveCompanyName("pico");

                    MainWindow.SetDeviceBatteryLevel(DeviceType.Headset, GetHeadsetBattery(), false);
                    MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerLeft, GetLeftControllerBattery(), false);
                    MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerRight, GetRightControllerBattery(), false);

                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }
        private static void UpdateClientManagerPointer(IntPtr threadstack0)
        {
            IntPtr curAdd = (IntPtr)ReadInt64(threadstack0 - 0x00000170);
            foreach (int x in offsets)
                curAdd = (IntPtr)ReadInt64(curAdd + x);

            clientManagerPtr = curAdd;
        }
        private static int ReadInt32(IntPtr addr)
        {
            byte[] results = new byte[4];
            ReadProcessMemory(streamingAssistantHandle, addr, results, results.Length, out _);

            return BitConverter.ToInt32(results, 0);
        }
        private static long ReadInt64(IntPtr addr)
        {
            byte[] results = new byte[8];
            ReadProcessMemory(streamingAssistantHandle, addr, results, results.Length, out _);

            return BitConverter.ToInt64(results, 0);
        }
        private static Task<IntPtr> getThread0Address(Process process)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "Assets/threadstack.exe",
                    Arguments = process.Id + "",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("THREADSTACK 0 BASE ADDRESS: "))
                {
                    line = line.Substring(line.LastIndexOf(":") + 2);
                    line = line.Substring(2);
                    return Task.FromResult((IntPtr)long.Parse(line, System.Globalization.NumberStyles.HexNumber));
                }
            }
            return Task.FromResult((IntPtr)(long)0);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        public static void StartListening()
        {
            Listen();
        }

        private static async void Listen()
        {
            while (true)
            {
                if (!isActive)
                    break;

                // Not completely sure, but if not updating pointers every X calls then it breaks and reads all numbers zero
                UpdateClientManagerPointer(THREADSTACK0);

                if (clientManagerPtr != IntPtr.Zero)
                {
                    int headsetBatteryLevel = GetHeadsetBattery();

                    if (headsetBatteryLevel >= 0 && headsetBatteryLevel <= 100)
                    {
                        if (Settings._config.predictCharging)
                            PredictChargeState(headsetBatteryLevel);

                        MainWindow.Instance.OnReceiveBatteryLevel(headsetBatteryLevel, DeviceType.Headset);
                        MainWindow.Instance.OnReceiveBatteryLevel(GetLeftControllerBattery(), DeviceType.ControllerLeft);
                        MainWindow.Instance.OnReceiveBatteryLevel(GetRightControllerBattery(), DeviceType.ControllerRight);
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(3000));
            }
        }

        private static long lastBatteryChange = 0;
        private static int lastLevel = 0;
        private static long lastTimeDifference = 0;
        private static void PredictChargeState(int currentBatteryLevel)
        {
            var curTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (lastLevel != 0)
            {
                var timeSinceLastChange = curTime - lastBatteryChange;
                if (lastLevel != currentBatteryLevel)
                {
                    var batteryLevelDifference = lastLevel - currentBatteryLevel;
                    lastBatteryChange = curTime;

                    lastTimeDifference = timeSinceLastChange;

                    bool isCharging = IsCharging(timeSinceLastChange, batteryLevelDifference);

                    MainWindow.Instance.OnReceiveBatteryState(isCharging, DeviceType.Headset);
                }

                if (lastTimeDifference != 0 && !BatteryInfoReceiver.IsHeadsetCharging() && IsCharging(timeSinceLastChange, 1))
                {
                    MainWindow.Instance.OnReceiveBatteryState(true, DeviceType.Headset);
                }
            }
            else
            {
                lastBatteryChange = curTime;
            }

            lastLevel = currentBatteryLevel;
        }

        private static bool IsCharging(long timeSinceLastChange, int batteryLevelDifference)
        {
            return timeSinceLastChange >= batteryDischargeMaximumTime || batteryLevelDifference <= 0;
        }

        public static void Terminate()
        {
            streamingAssistant = null;
            clientManagerPtr = IntPtr.Zero;
            streamingAssistantHandle = IntPtr.Zero;
            THREADSTACK0 = IntPtr.Zero;

            isActive = false;
        }
    }
}
