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
            bool isSuccess = OSC.Init(); // required for sending stuff to vrc
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

            if (Settings.GetValue<bool>("useStreamingApp"))
                InitStreamingAppListener();
            else
                InitHeadsetListener();
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

        private struct ContextMenuOption
        {
            public string Name;
            public string Setting;
            public bool Default;
            public bool Mirror;

            public Action CustomAction;
            public Func<bool> CustomCheck; // Return false to prevent appearing option in context menu and executing action when clicking on option
        }

        private void BuildContextMenu(ContextMenu menu, List<ContextMenuOption> options)
        {
            foreach (var option in options)
            {
                if (option.CustomCheck != null && !option.CustomCheck())
                    continue;

                if (option.Name == "")
                {
                    menu.Items.Add(new Separator());

                    continue;
                }

                var value = Settings.GetValue<bool>(option.Setting, option.Default);
                value = option.Mirror ? !value : value;

                var btn = new MenuItem();
                btn.Header = option.Name;
                btn.Click += (object _sender, RoutedEventArgs _e) =>
                {
                    if (option.CustomCheck != null && !option.CustomCheck())
                        return;

                    if (option.CustomAction != null)
                    {
                        option.CustomAction();
                    }
                    else
                    {
                        Settings.SetValue(option.Setting, !Settings.GetValue<bool>(option.Setting, option.Default));
                    }
                };
                btn.IsChecked = value;

                menu.Items.Add(btn);
            }
        }

        private List<ContextMenuOption> settingsDropdownOptions = new List<ContextMenuOption>()
        {
            new ContextMenuOption()
            {
                Name = "(Pico) Use Streaming Assistant",
                Setting = Settings.Setting_UseStreamingApp,
                Default = false,

                CustomAction = () =>
                {
                    Settings.SetValue(Settings.Setting_UseStreamingApp, true);

                    Instance.confirmedHeadsetPair = false;
                    Instance.ShowStatusText();
                    Instance.HideMainContent();

                    OSC.Terminate();

                    Instance.InitStreamingAppListener();
                }
            },

            new ContextMenuOption()
            {
                Name = "(Any) Use Headset application",
                Setting = Settings.Setting_UseStreamingApp,
                Mirror = true,

                CustomAction = () =>
                {
                    Settings.SetValue(Settings.Setting_UseStreamingApp, false);

                    Instance.confirmedHeadsetPair = false;
                    Instance.ShowStatusText();
                    Instance.HideMainContent();

                    StreamingAssistant.Terminate();

                    Instance.InitHeadsetListener();
                }
            },

            new ContextMenuOption() { Name = "" },

            new ContextMenuOption()
            {
                Name = "(OVR Toolkit) Send windows notifications about battery state",
                Setting = Settings.Setting_OVRToolkitNotification,
                Default = true,
            },

            new ContextMenuOption()
            {
                Name = "(XSOverlay) Send notifications about battery state",
                Setting = Settings.Setting_XSOverlayNotification,
                Default = true,
            },
        };

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();

            BuildContextMenu(contextMenu, settingsDropdownOptions);

            contextMenu.IsOpen = true;
        }

        private List<ContextMenuOption> headsetDropdownOptions = new List<ContextMenuOption>()
        {
            new ContextMenuOption()
            {
                Name = "(Streaming Assistant) Predict charging state",
                Setting = Settings.Setting_PredictHeadsetCharge,
                Default = true,

                CustomCheck = () =>
                {
                    return Settings.GetValue<bool>(Settings.Setting_UseStreamingApp);
                },
            },

            new ContextMenuOption()
            {
                Name = "Notify on low battery",
                Setting = Settings.Setting_HeadsetNotifyLowBattery,
                Default = true,
            },

            new ContextMenuOption()
            {
                Name = "Notify on charge stop",
                Setting = Settings.Setting_HeadsetNotifyStopCharging,
                Default = true,
            },
        };
        private void HeadsetDropDown_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();

            BuildContextMenu(contextMenu, headsetDropdownOptions);

            contextMenu.IsOpen = true;
        }

        private List<ContextMenuOption> controllerDropdownOptions = new List<ContextMenuOption>()
        {
            new ContextMenuOption()
            {
                Name = "Notify on low battery",
                Setting = Settings.Setting_ControllersNotifyLowBattery,
                Default = true
            },
        };
        private void ControllerDropDown_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();

            BuildContextMenu(contextMenu, controllerDropdownOptions);

            contextMenu.IsOpen = true;
        }
    }
}
