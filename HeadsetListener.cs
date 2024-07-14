using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
{
    internal class HeadsetListener : BatteryTracking
    {
        public override void Init()
        {
            base.Init();

            OSC.OnReceiveHeadset += OnOSCReceiveHeadset;
            OSC.OnReceiveBatteryLevel += OnReceiveBatteryLevel;
            OSC.OnReceiveBatteryState += OnReceiveBatteryState;
            OSC.OnReceiveCompanyName += OnReceiveCompanyName;

            OSC.StartListening();
        }
        private void OnOSCReceiveHeadset()
        {
            MainWindow.Instance.SetStatusText("Received headset, confirming...");
        }
        private void OnReceiveBatteryLevel(int level, DeviceType device)
        {
            BatteryInfoReceiver.OnReceiveBatteryLevel(level, device);

            SetBattery(device, level);
        }
        private void OnReceiveBatteryState(bool isCharging, DeviceType device)
        {
            BatteryInfoReceiver.OnReceiveBatteryState(isCharging, device);

            SetBattery(device, -1, isCharging);
        }
        private void OnReceiveCompanyName(string companyName)
        {
            MainWindow.Instance.SetCompany(companyName);
        }

        public override void Terminate()
        {
            base.Terminate();
            OSC.Terminate();

            OSC.OnReceiveHeadset -= OnOSCReceiveHeadset;
            OSC.OnReceiveBatteryLevel -= OnReceiveBatteryLevel;
            OSC.OnReceiveBatteryState -= OnReceiveBatteryState;
            OSC.OnReceiveCompanyName -= OnReceiveCompanyName;
        }

        protected override async Task Listen()
        {
            await base.Listen();

            await Task.Delay(1000);
        }
    }
}
