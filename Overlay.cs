using System;
using XSNotifications;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Uwp.Notifications;

namespace HeadsetBatteryInfo
{
    interface IOverlay
    { 
        void SendNotification(string message);
        void UpdateWristInfo(DeviceType device, int batteryLevel, bool batteryState);
    }

    internal class XSOverlay : IOverlay
    {

        public void SendNotification(string message)
        {
            new XSNotifier().SendNotification(new XSNotification()
            {
                Title = "Headset Battery Info",
                Content = message,
                Timeout = 3,

                Volume = .5f,
                AudioPath = "warning",
                Icon = "warning"
            });
        }

        public void UpdateWristInfo(DeviceType device, int batteryLevel, bool batteryState)
        {
            // TODO: Wait for such feature
        }
    }

    internal class OVRToolKit : IOverlay
    {
        private const int port = 28093;

        private struct DevicePacket
        {
            public DeviceType device;
            public bool isCharging;
            public int batteryLevel;
            public DeviceIcons.Company company;
        }

        public void SendNotification(string message)
        {
            new ToastContentBuilder()
                .AddText("Headset Battery Info")
                .AddText(message)
                .Show();
        }

        public void UpdateWristInfo(DeviceType device, int batteryLevel, bool batteryState)
        {
            var packet = new DevicePacket
            {
                device = device,
                isCharging = batteryState,
                batteryLevel = batteryLevel,
                company = DeviceIcons.GetCompany()
            };

            byte[] buffer = GetPacketBytes(packet);

            OSC.SendBytesToPort(buffer, port);
        }
        private byte[] GetPacketBytes(DevicePacket packet)
        {
            int size = Marshal.SizeOf(packet);
            byte[] arr = new byte[size];

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(packet, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }
    }

    internal class Overlay
    {
        private static IOverlay ovrToolkit;
        private static IOverlay xsOverlay;
        public static void Init()
        {
            ovrToolkit = new OVRToolKit();
            xsOverlay = new XSOverlay();
        }

        public static void SendNotification(string message)
        {
            if (Settings._config.ovrToolkitSupport)
            {
                ovrToolkit.SendNotification(message);
            }

            if (Settings._config.xsOverlaySupport)
            {
                xsOverlay.SendNotification(message);
            }
        }

        public static void UpdateWristInfo(DeviceType device, int batteryLevel, bool batteryState)
        {
            ovrToolkit.UpdateWristInfo(device, batteryLevel, batteryState);
            xsOverlay.UpdateWristInfo(device, batteryLevel, batteryState);
        }
    }
}