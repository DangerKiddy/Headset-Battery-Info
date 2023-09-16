using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HeadsetBatteryInfo
{
    internal class OSC
    {
        private const string ipAddress = "127.0.0.1";
        private const int port = 28092;

        private static UdpClient udp;
        public static bool Init()
        {
            bool isSuccess = false;

            try
            {
                udp = new UdpClient(port);

                isSuccess = true;
            }
            catch
            {
                Console.WriteLine($"Failed to create listen socket! (Another app listening on {ipAddress}:{port} already?)");
            }

            return isSuccess;
        }

        public delegate void receiveHeadset();
        private static receiveHeadset onReceiveHeadset;
        public static void AddReceiveHeadsetCallback(receiveHeadset callback)
        {
            onReceiveHeadset += callback;
        }

        public delegate void receiveBatteryLevel(int level, DeviceType device);
        private static receiveBatteryLevel onReceiveBatteryLevel;
        public static void AddReceiveBatteryLevelCallback(receiveBatteryLevel callback)
        {
            onReceiveBatteryLevel += callback;
        }

        public delegate void receiveBatteryState(bool isCharging, DeviceType device);
        private static receiveBatteryState onReceiveBatteryState;
        public static void AddReceiveBatteryStateCallback(receiveBatteryState callback)
        {
            onReceiveBatteryState += callback;
        }

        public static void StartListening()
        {
            Listen();
        }

        private static async void Listen()
        {
            while (true)
            {
                Receive();

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }

        private static async void Receive()
        {
            try
            {
                var incomingIP = new IPEndPoint(IPAddress.Any, 0);
                var data = await udp.ReceiveAsync();
                incomingIP =  data.RemoteEndPoint;

                Msg msg = ParseOSC(data.Buffer, data.Buffer.Length);

                if (msg.success)
                {
                    switch (msg.address)
                    {
                        case "/netLocalIpAddress":
                            var sendBack = "/confirmAddress";
                            AlignStringBytes(ref sendBack);

                            sendBack += ",s";

                            AlignStringBytes(ref sendBack);
                            sendBack += msg.value;

                            AlignStringBytes(ref sendBack);

                            var buffer = Encoding.ASCII.GetBytes(sendBack);
                            udp.Send(buffer, buffer.Length, new IPEndPoint(incomingIP.Address, port));

                            onReceiveHeadset();

                            break;

                        case "/battery/headset/level":
                            onReceiveBatteryLevel((int)msg.value, DeviceType.Headset);

                            break;

                        case "/battery/headset/charging":
                            onReceiveBatteryState((bool)msg.value, DeviceType.Headset);

                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void AlignStringBytes(ref string str)
        {
            int strLen = str.Length;
            if (strLen % 4 != 0)
            {
                strLen += 4 - (strLen % 4);
            }

            for (int i = str.Length; i < strLen; i++)
            {
                str += '\0';
            }
        }

        private static byte[] EncodeString(string str)
        {
            int len = str.Length + (4 - str.Length % 4);
            if (len <= str.Length)
                len = len + 4;

            byte[] msg = new byte[len];

            var bytes = Encoding.ASCII.GetBytes(str);
            bytes.CopyTo(msg, 0);

            return msg;
        }

        struct Msg
        {
            public string address;
            public object value;
            public bool success;
        }
        private static Msg ParseOSC(byte[] buffer, int length)
        {
            Msg msg = new Msg();
            msg.success = false;

            if (length < 4)
                return msg;

            int bufferPosition = 0;
            string address = ParseString(buffer, length, ref bufferPosition);
            if (address == "")
                return msg;

            msg.address = address;

            // checking for ',' char
            if (buffer[bufferPosition] != 44)
                return msg;
            bufferPosition++; // skipping ',' character

            char valueType = (char)buffer[bufferPosition];
            bufferPosition++;

            object value = null;
            switch (valueType)
            {
                case 'f':
                    value = ParesFloat(buffer, length, bufferPosition);

                    break;

                case 'i':
                    value = ParseInt(buffer, length, bufferPosition);

                    break;

                case 'F':
                    value = false;

                    break;

                case 'T':
                    value = true;

                    break;

                case 's':
                    if (bufferPosition % 4 != 0)
                    {
                        bufferPosition += 4 - (bufferPosition % 4);
                    }

                    value = ParseString(buffer, length, ref bufferPosition, true);

                    break;

                default:
                    break;
            }

            msg.value = value ?? 0;
            msg.success = true;

            return msg;
        }

        private static string ParseString(byte[] buffer, int length, ref int bufferPosition, bool useBufferPos = false)
        {
            string address = "";

            // first character must be '/'
            if (buffer[0] != 47)
                return address;

            for (int i = useBufferPos ? bufferPosition : 0; i < length; i++)
            {
                if (buffer[i] == 0)
                {
                    bufferPosition = i + 1;

                    if (bufferPosition % 4 != 0)
                    {
                        bufferPosition += 4 - (bufferPosition % 4);
                    }

                    break;
                }

                address += (char)buffer[i];
            }

            return address;
        }

        private static float ParesFloat(byte[] buffer, int length, int bufferPosition)
        {
            var valueBuffer = new byte[length - bufferPosition];

            int j = 0;
            for (int i = bufferPosition; i < length; i++)
            {
                valueBuffer[j] = buffer[i];

                j++;
            }

            float value = bytesToFLoat(valueBuffer);
            return value;
        }

        private static float bytesToFLoat(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Convert big endian to little endian
            }

            float val = BitConverter.ToSingle(bytes, 0);
            return val;
        }

        private static int ParseInt(byte[] buffer, int length, int bufferPosition)
        {
            var valueBuffer = new byte[length - bufferPosition];

            int j = 0;
            for (int i = bufferPosition; i < length; i++)
            {
                valueBuffer[j] = buffer[i];

                j++;
            }

            int value = bytesToInt(valueBuffer);
            return value;
        }

        private static int bytesToInt(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Convert big endian to little endian
            }

            int val = BitConverter.ToInt32(bytes, 0);
            return val;
        }
    }
}
