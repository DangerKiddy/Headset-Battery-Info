using System;
using System.Diagnostics;

namespace HeadsetBatteryInfo
{
    internal class HeadsetListener : BatteryTracking
    {
        public override void Init()
        {
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
        }
        private void OnReceiveBatteryState(bool isCharging, DeviceType device)
        {
            BatteryInfoReceiver.OnReceiveBatteryState(isCharging, device);

            Trace.WriteLine($"Received {isCharging}");
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
    }
}
