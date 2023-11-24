using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace HeadsetBatteryInfo
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        private bool useStreamingApp = true;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            InitDefaultUiValues();

            ShowStatusText();

            DeviceIcons.Init();

            if (useStreamingApp)
                InitStreamingAppListener();
            else
                InitHeadsetListener();
        }

        private void InitDefaultUiValues()
        {
            ControllerLeftBackground.Fill = DeviceIcons.GetGreyGradient();
            ControllerLeftDropDown.Background = DeviceIcons.GetGreyGradient();
            ControllerLeftText.Text = "Unknown";
            ControllerLeftText.Foreground = DeviceIcons.GetGreySolid();

            ControllerLeftProgress.Width = 0;

            ControllerRightBackground.Fill = DeviceIcons.GetGreyGradient();
            ControllerRightDropDown.Background = DeviceIcons.GetGreyGradient();
            ControllerRightText.Text = "Unknown";
            ControllerRightText.Foreground = DeviceIcons.GetGreySolid();

            ControllerRightProgress.Width = 0;

            CompanyLogo.Opacity = 0;
            Headset.Opacity = 0;
            ControllerLeft.Opacity = 0;
            ControllerRight.Opacity = 0;
        }

        bool confirmedHeadsetPair = false;
        private void ShowStatusText()
        {
            var showAnim = new DoubleAnimation();
            showAnim.From = 0;
            showAnim.To = 1;
            showAnim.Duration = TimeSpan.FromMilliseconds(1000);
            StatusText.BeginAnimation(OpacityProperty, showAnim);
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

        private static void UpdateDeviceUi(DeviceType device, int level, bool isCharging)
        {
            Rectangle progressBar = default;
            Rectangle background = default;
            System.Windows.Controls.Button dropdown = default;
            System.Windows.Controls.TextBlock text = default;

            switch (device)
            {
                case DeviceType.Headset:
                    progressBar = Instance.HeadsetProgress;
                    background = Instance.HeadsetBackground;
                    dropdown = Instance.HeadsetDropDown;
                    text = Instance.HeadsetText;

                    break;

                case DeviceType.ControllerLeft:
                    progressBar = Instance.ControllerLeftProgress;
                    background = Instance.ControllerLeftBackground;
                    dropdown = Instance.ControllerLeftDropDown;
                    text = Instance.ControllerLeftText;

                    break;

                case DeviceType.ControllerRight:
                    progressBar = Instance.ControllerRightProgress;
                    background = Instance.ControllerRightBackground;
                    dropdown = Instance.ControllerRightDropDown;
                    text = Instance.ControllerRightText;

                    break;
            }

            void SetColors(System.Windows.Media.LinearGradientBrush gradient, System.Windows.Media.SolidColorBrush solid)
            {
                System.Windows.Media.LinearGradientBrush useColor = gradient;
                if (!isCharging)
                    useColor = gradient;
                else
                    useColor = DeviceIcons.GetBlueGradient();

                background.Fill = useColor;
                dropdown.Background = useColor;
                text.Foreground = useColor;

                progressBar.Fill = solid;
            }

            if (level > 50)
            {
                SetColors(DeviceIcons.GetGreenGradient(), DeviceIcons.GetGreenSolid());
            }
            else if (level > 25)
            {
                SetColors(DeviceIcons.GetYellowGradient(), DeviceIcons.GetYellowSolid());
            }
            else
            {
                SetColors(DeviceIcons.GetRedGradient(), DeviceIcons.GetRedSolid());
            }
        }

        public static void SetDeviceBatteryLevel(DeviceType device, int level, bool isCharging)
        {
            level = Math.Max(Math.Min(level, 100), 0);

            string str = (isCharging ? "Charge " : "") + level + "%";
            float level01 = level / 100f;

            switch (device)
            {
                case DeviceType.Headset:
                    Instance.HeadsetText.Text = str;
                    Instance.HeadsetProgress.Width = 96 * level01;

                    break;

                case DeviceType.ControllerLeft:
                    Instance.ControllerLeftText.Text = str;
                    Instance.ControllerLeftProgress.Width = 96 * level01;

                    break;

                case DeviceType.ControllerRight:
                    Instance.ControllerRightText.Text = str;
                    Instance.ControllerRightProgress.Width = 96 * level01;

                    break;
            }

            UpdateDeviceUi(device, level, isCharging);
            Instance.ConfirmHeadsetPair();
        }

        private void InitStreamingAppListener()
        {
            SetStatusText("Waiting for streaming app...");

            StreamingAssistant.Init();
            StreamingAssistant.StartListening();
        }

        bool isCallbackInitialized = false;
        private void InitHeadsetListener()
        {
            bool isSuccess = OSC.Init();
            if (!isSuccess)
            {
                SetStatusText("Failed to init OSC!\nSomething else is listening to 28092 port?");

                return;
            }

            SetStatusText("Waiting for headset...");

            if (!isCallbackInitialized)
            {
                isCallbackInitialized = true;

                OSC.AddReceiveHeadsetCallback(OnOSCReceiveHeadset);
                OSC.AddReceiveBatteryLevelCallback(OnReceiveBatteryLevel);
                OSC.AddReceiveBatteryStateCallback(OnReceiveBatteryState);
                OSC.AddReceiveCompanyNameCallback(OnReceiveCompanyName);
            }

            OSC.StartListening();
        }

        private void OnOSCReceiveHeadset()
        {
            SetStatusText("Received headset, confirming...");
        }

        public void OnReceiveCompanyName(string company)
        {
            ConfirmHeadsetPair();

            DeviceIcons.SetCompany(company);
            SetupImages();
        }

        private void SetupImages()
        {
            var icons = DeviceIcons.GetCurrentDeviceIcons();
            CompanyLogo.Source = icons.companyLogo;
            HeadsetIcon.Source = icons.headset;
            ControllerLeftIcon.Source = icons.leftController;
            ControllerRightIcon.Source = icons.rightController;
        }

        public void OnReceiveBatteryLevel(int level, DeviceType device)
        {
            BatteryInfoReceiver.OnReceiveBatteryLevel(level, device);
        }

        public void OnReceiveBatteryState(bool isCharging, DeviceType device)
        {
            BatteryInfoReceiver.OnReceiveBatteryState(isCharging, device);
        }

        private void SetStatusText(string newStatus)
        {
            StatusText.Text = newStatus;
        }

        private void ConfirmHeadsetPair()
        {
            SetStatusText("Done!");
            HideStatusText();
            ShowMainContent();

            confirmedHeadsetPair = true;
        }

        bool mainContentHidden = true;
        private void ShowMainContent()
        {
            if (!mainContentHidden)
                return;

            mainContentHidden = false;

            var showAnim = new DoubleAnimation();
            showAnim.From = 0;
            showAnim.To = 1;
            showAnim.Duration = TimeSpan.FromSeconds(1);

            CompanyLogo.BeginAnimation(OpacityProperty, showAnim);
            Headset.BeginAnimation(OpacityProperty, showAnim);
            ControllerLeft.BeginAnimation(OpacityProperty, showAnim);
            ControllerRight.BeginAnimation(OpacityProperty, showAnim);
        }

        private void HideMainContent()
        {
            if (mainContentHidden)
                return;

            mainContentHidden = true;

            var showAnim = new DoubleAnimation();
            showAnim.From = 1;
            showAnim.To = 0;
            showAnim.Duration = TimeSpan.FromSeconds(1);

            CompanyLogo.BeginAnimation(OpacityProperty, showAnim);
            Headset.BeginAnimation(OpacityProperty, showAnim);
            ControllerLeft.BeginAnimation(OpacityProperty, showAnim);
            ControllerRight.BeginAnimation(OpacityProperty, showAnim);
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
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();
            
            var strAssist = new MenuItem();
            strAssist.Header = "(Pico) Use Streaming Assistant";
            strAssist.Click += (object _sender, RoutedEventArgs _e) =>
            {
                confirmedHeadsetPair = false;
                ShowStatusText();
                HideMainContent();

                OSC.Terminate();

                InitStreamingAppListener();
            };
            contextMenu.Items.Add(strAssist);

            var useApp = new MenuItem();
            useApp.Header = "(Any) Use Headset application";
            useApp.Click += (object _sender, RoutedEventArgs _e) =>
            {
                confirmedHeadsetPair = false;
                ShowStatusText();
                HideMainContent();

                StreamingAssistant.Terminate();

                InitHeadsetListener();
            };
            contextMenu.Items.Add(useApp);

            contextMenu.IsOpen = true;  
        }
    }
}
