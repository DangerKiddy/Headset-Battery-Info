using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
{
    internal class StreamingAssistant : BatteryTracking
    {
        private Process streamingAssistant;
        private IntPtr clientManagerPtr;
        private IntPtr streamingAssistantHandle;
        private IntPtr THREADSTACK0;

        private int[] offsets = { 0x28, 0x8, 0x10, 0x18, 0x8, 0x10 };
        private int headsetBatteryOffset = 0x64;
        private int leftControllerBatteryOffset = 0x68;
        private int rightControllerBatteryOffset = 0x6C;
        
        private enum ControllerBatteryLevel
        {
            NotConnected = 0,
            VeryLow = 1,
            Low = 2,
            Half = 3,
            Okay = 4,
            Full = 5
        }

        private Dictionary<ControllerBatteryLevel, int> batteryEnumToLevel = new Dictionary<ControllerBatteryLevel, int>()
        {
            [ControllerBatteryLevel.NotConnected] = 0,
            [ControllerBatteryLevel.VeryLow] = 20,
            [ControllerBatteryLevel.Low] = 40,
            [ControllerBatteryLevel.Half] = 60,
            [ControllerBatteryLevel.Okay] = 80,
            [ControllerBatteryLevel.Full] = 100,
        };

        private int GetHeadsetBattery()
        {
            return ReadInt32(clientManagerPtr + headsetBatteryOffset);
        }

        private int ConvertControllerBatteryToPercent(ControllerBatteryLevel batteryLevel)
        {
            int percent;

            if (!batteryEnumToLevel.TryGetValue(batteryLevel, out percent))
                percent = 0;

            return percent;
        }

        private ControllerBatteryLevel GetControllerBatteryState(DeviceType controller)
        {
            if (controller == DeviceType.ControllerLeft)
                return (ControllerBatteryLevel)ReadInt32(clientManagerPtr + leftControllerBatteryOffset);
            else
                return (ControllerBatteryLevel)ReadInt32(clientManagerPtr + rightControllerBatteryOffset);
        }

        private int GetLeftControllerBattery()
        {
            ControllerBatteryLevel batteryState = GetControllerBatteryState(DeviceType.ControllerLeft);
            int batteryLevel = ConvertControllerBatteryToPercent(batteryState);

            return batteryLevel;
        }

        private int GetRightControllerBattery()
        {
            ControllerBatteryLevel batteryState = GetControllerBatteryState(DeviceType.ControllerRight);
            int batteryLevel = ConvertControllerBatteryToPercent(batteryState);

            return batteryLevel;
        }

        public override async void Init()
        {
            base.Init();

            bool streamingAppFound = false;
            while (!streamingAppFound)
            {
                if (!isTeardown || !MainWindow.IsRunAsAdmin())
                    break;

                var processes = Process.GetProcessesByName("Streaming Assistant");

                if (processes.Length > 0)
                {
                    streamingAssistant = processes[0];
                    streamingAssistantHandle = streamingAssistant.Handle;

                    var threadstack0 = await getThread0Address(streamingAssistant);
                    THREADSTACK0 = threadstack0;

                    UpdateClientManagerPointer(threadstack0);

                    MainWindow.Instance.SetCompany("pico");

                    MainWindow.SetDeviceBatteryLevel(DeviceType.Headset, GetHeadsetBattery(), false);
                    MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerLeft, GetLeftControllerBattery(), false);
                    MainWindow.SetDeviceBatteryLevel(DeviceType.ControllerRight, GetRightControllerBattery(), false);

                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }
        private void UpdateClientManagerPointer(IntPtr threadstack0)
        {
            IntPtr curAdd = (IntPtr)ReadInt64(threadstack0 - 0x00000170);
            foreach (int x in offsets)
                curAdd = (IntPtr)ReadInt64(curAdd + x);

            clientManagerPtr = curAdd;
        }
        private int ReadInt32(IntPtr addr)
        {
            byte[] results = new byte[4];
            ReadProcessMemory(streamingAssistantHandle, addr, results, results.Length, out _);

            return BitConverter.ToInt32(results, 0);
        }
        private long ReadInt64(IntPtr addr)
        {
            byte[] results = new byte[8];
            ReadProcessMemory(streamingAssistantHandle, addr, results, results.Length, out _);

            return BitConverter.ToInt64(results, 0);
        }
        private Task<IntPtr> getThread0Address(Process process)
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

        protected override async Task Listen()
        {
            // Not completely sure, but if not updating pointers every X calls then it breaks and reads all numbers as zero
            UpdateClientManagerPointer(THREADSTACK0);

            if (clientManagerPtr != IntPtr.Zero)
            {
                int headsetBatteryLevel = GetHeadsetBattery();

                if (headsetBatteryLevel >= 0 && headsetBatteryLevel <= 100)
                {
                    if (Settings._config.predictCharging)
                        PredictChargeState(headsetBatteryLevel);

                    int leftControllerBatteryLevel = GetLeftControllerBattery();
                    int rightControllerBatteryLevel = GetRightControllerBattery();

                    BatteryInfoReceiver.OnReceiveBatteryLevel(headsetBatteryLevel, DeviceType.Headset);
                    BatteryInfoReceiver.OnReceiveBatteryLevel(leftControllerBatteryLevel, DeviceType.ControllerLeft);
                    BatteryInfoReceiver.OnReceiveBatteryLevel(rightControllerBatteryLevel, DeviceType.ControllerRight);

                    SetBattery(DeviceType.Headset, headsetBatteryLevel);
                    SetBattery(DeviceType.ControllerLeft, leftControllerBatteryLevel);
                    SetBattery(DeviceType.ControllerRight, rightControllerBatteryLevel);
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(3000));
        }

        public override void Terminate()
        {
            base.Terminate();

            streamingAssistant = null;
            clientManagerPtr = IntPtr.Zero;
            streamingAssistantHandle = IntPtr.Zero;
            THREADSTACK0 = IntPtr.Zero;
        }
    }
}
