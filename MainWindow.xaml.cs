using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Windows.System;

namespace HeadsetBatteryInfo
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            InitDefaultUiValues();

            ShowStatusText();

            DeviceIcons.Init();
            bool isSuccess = OSC.Init();
            if (!isSuccess)
            {
                SetStatusText("Failed to init OSC!\nSomething else is listening to 28092 port?");

                return;
            }

            Settings.Load();
            Overlay.Init();

            HeadsetDropDown.IsEnabled = false;
            ControllerLeftDropDown.IsEnabled = false;
            ControllerRightDropDown.IsEnabled = false;

            switch (Settings._config.receiveMode)
            {
                case 0:
                    InitHeadsetListener();
                    break;
                case 1:
                    InitStreamingAppListener();
                    break;
                case 2:
                    InitPicoConnectListener();
                    break;
            }
        }

        public static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
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
                if (isCharging)
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
            if (!IsRunAsAdmin())
            {
                SetStatusText("Application must be ran as administrator!");

                return;
            }
            SetStatusText("Waiting for streaming app...");

            StreamingAssistant.Init();
            StreamingAssistant.StartListening();
        }

        private void InitPicoConnectListener()
        {
            SetStatusText("Waiting for Pico Connect app...");

            PicoConnect.Init();
            PicoConnect.StartListening();
        }

        bool isCallbackInitialized = false;
        private void InitHeadsetListener()
        {
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

            HeadsetDropDown.IsEnabled = true;
            ControllerLeftDropDown.IsEnabled = true;
            ControllerRightDropDown.IsEnabled = true;

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

            HeadsetDropDown.IsEnabled = false;
            ControllerLeftDropDown.IsEnabled = false;
            ControllerRightDropDown.IsEnabled = false;

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

        private void BuildMenuItem(ContextMenu menu, string header, RoutedEventHandler clickHandler, bool isChecked)
        {
            var menuItem = new MenuItem
            {
                Header = header,
                IsChecked = isChecked
            };
            menuItem.Click += clickHandler += (s, e) =>
            {
                Settings.Save();
            };
            menu.Items.Add(menuItem);
        }

        private void BuildMainSettingMenu(ContextMenu menu)
        {
            AddReceiveModeMenuItem(menu, "(Pico) Use PICO Connect", 2, () =>
            {
                Instance.InitPicoConnectListener();
            });
            AddReceiveModeMenuItem(menu, "(Pico) Use Streaming Assistant", 1, () =>
            {
                Instance.InitStreamingAppListener();
            });
            AddReceiveModeMenuItem(menu, "(Any) Use Headset application", 0, () =>
            {
                Instance.InitHeadsetListener();
            });
            menu.Items.Add(new Separator());
            BuildMenuItem(menu, "(OVR Toolkit) Send windows notifications about battery state",
                (s, e) => ToggleSetting(() => Settings._config.ovrToolkitSupport, val => Settings._config.ovrToolkitSupport = val), Settings._config.ovrToolkitSupport);
            BuildMenuItem(menu, "(XSOverlay) Send notifications about battery state",
                (s, e) => ToggleSetting(() => Settings._config.xsOverlaySupport, val => Settings._config.xsOverlaySupport = val), Settings._config.xsOverlaySupport);
            BuildMenuItem(menu, "(VRChat) Send OSC data to VRChat",
                (s, e) => ToggleSetting(() => Settings._config.enableOSC, val => Settings._config.enableOSC = val), Settings._config.enableOSC);
        }

        private void AddReceiveModeMenuItem(ContextMenu menu, string header, int mode, Action action)
        {
            BuildMenuItem(menu, header, (s, e) =>
            {
                OSC.Terminate();
                PicoConnect.Terminate();
                StreamingAssistant.Terminate();
                Settings._config.receiveMode = mode;
                UpdateInstanceSettings();
                action();
            }, Settings._config.receiveMode == mode);
        }

        private void BuildHeadsetSettingMenu(ContextMenu menu)
        {
            BuildMenuItem(menu, "(Streaming Assistant) Predict charging state",
                (s, e) => ToggleSetting(() => Settings._config.predictCharging, val => Settings._config.predictCharging = val), Settings._config.predictCharging);
            BuildMenuItem(menu, "Notify on low battery",
                (s, e) => ToggleSetting(() => Settings._config.notifyOnHeadsetLowBattery, val => Settings._config.notifyOnHeadsetLowBattery = val), Settings._config.notifyOnHeadsetLowBattery);
            BuildMenuItem(menu, "Notify on charge stop",
                (s, e) => ToggleSetting(() => Settings._config.notifyOnHeadsetStopCharging, val => Settings._config.notifyOnHeadsetStopCharging = val), Settings._config.notifyOnHeadsetStopCharging);
        }

        private void BuildControllerSettingMenu(ContextMenu menu)
        {
            BuildMenuItem(menu, "Notify on low battery",
                (s, e) => ToggleSetting(() => Settings._config.notifyOnControllerLowBattery, val => Settings._config.notifyOnControllerLowBattery = val), Settings._config.notifyOnControllerLowBattery);
        }

        private void UpdateInstanceSettings()
        {
            Instance.confirmedHeadsetPair = false;
            Instance.ShowStatusText();
            Instance.HideMainContent();
        }

        private void ToggleSetting(Func<bool> getSetting, Action<bool> setSetting)
        {
            setSetting(!getSetting());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();
            BuildMainSettingMenu(contextMenu);
            contextMenu.IsOpen = true;
        }

        private void HeadsetDropDown_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();
            BuildHeadsetSettingMenu(contextMenu);
            contextMenu.IsOpen = true;
        }

        private void ControllerDropDown_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();
            BuildControllerSettingMenu(contextMenu);
            contextMenu.IsOpen = true;
        }

    }
}