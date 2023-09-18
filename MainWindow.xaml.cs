using System;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HeadsetBatteryInfo
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            CompanyLogo.Opacity = 0;
            Headset.Opacity = 0;
            ControllerLeft.Opacity = 0;
            ControllerRight.Opacity = 0;

            Instance = this;

            var showAnim = new DoubleAnimation();
            showAnim.From = 0;
            showAnim.To = 1;
            showAnim.Duration = TimeSpan.FromMilliseconds(1000);

            StatusText.BeginAnimation(OpacityProperty, showAnim);

            bool isSuccess = OSC.Init();
            if (!isSuccess)
            {
                SetStatusText("Failed to init OSC!\nSomething else is listening to 28092 port?");

                return;
            }

            DeviceIcons.Init();

            SetStatusText("Waiting for headset...");

            OSC.AddReceiveHeadsetCallback(OnOSCReceiveHeadset);
            OSC.AddReceiveBatteryLevelCallback(OnReceiveBatteryLevel);
            OSC.AddReceiveBatteryStateCallback(OnReceiveBatteryState);
            OSC.AddReceiveCompanyNameCallback(OnReceiveCompanyName);

            OSC.StartListening();
        }

        private void OnOSCReceiveHeadset()
        {
            SetStatusText("Received headset, confirming...");
        }

        private void OnReceiveCompanyName(string company)
        {
            ConfirmHeadsetPair();

            DeviceIcons.SetCompany(company);
        }

        private void OnReceiveBatteryLevel(int level, DeviceType device)
        {
            ConfirmHeadsetPair();

            BatteryInfoReceiver.OnReceiveBatteryLevel(level, device);
        }

        private void OnReceiveBatteryState(bool isCharging, DeviceType device)
        {
            ConfirmHeadsetPair();

            BatteryInfoReceiver.OnReceiveBatteryState(isCharging, device);
        }

        private void SetStatusText(string newStatus)
        {
            StatusText.Text = newStatus;
        }

        internal static void ChangeHeadsetIcon(ImageSource icon)
        {
            Instance.HeadsetIcon.Source = icon;
        }

        internal static void SetHeadsetText(string txt)
        {
            Instance.HeadsetText.Text = txt;
        }

        bool confirmedHeadsetPair = false;
        private void ConfirmHeadsetPair()
        {
            SetStatusText("Done!");
            HideStatusText();
            ShowMainContent();

            confirmedHeadsetPair = true;
        }

        private void HideStatusText()
        {
            if (confirmedHeadsetPair)
                return;

            var hideAnim = new DoubleAnimation();
            hideAnim.From = 1;
            hideAnim.To = 0;
            hideAnim.Duration = TimeSpan.FromMilliseconds(500);

            StatusText.BeginAnimation(OpacityProperty, hideAnim);
        }

        private void ShowMainContent()
        {
            if (confirmedHeadsetPair)
                return;

            var showAnim = new DoubleAnimation();
            showAnim.From = 0;
            showAnim.To = 1;
            showAnim.Duration = TimeSpan.FromSeconds(1);

            CompanyLogo.BeginAnimation(OpacityProperty, showAnim);
            Headset.BeginAnimation(OpacityProperty, showAnim);
            //ControllerLeft.BeginAnimation(OpacityProperty, showAnim);
            //ControllerRight.BeginAnimation(OpacityProperty, showAnim);
        }

        private static SoundPlayer sound;
        public static void PlayBatteryStateSound()
        {
            if (sound == null)
            {
                sound = new SoundPlayer();
                sound.SoundLocation = "Assets/Sounds/charging_stopped.wav";
                sound.Load();
            }

            sound.Play();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
