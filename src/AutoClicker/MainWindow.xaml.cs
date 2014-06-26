using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using VirtualInput;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Control = System.Windows.Forms.Control;
using MenuItem = System.Windows.Forms.MenuItem;

namespace AutoClicker
{
    public partial class MainWindow
    {
        #region Properties

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, uint dx, uint dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void SetCursorPos(int X, int Y);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        private NotifyIcon _ni = new NotifyIcon();

        private int _milliseconds
        {
            get { return GetInteger(MillisecondsEdit.Text); }
            set { MillisecondsEdit.Text = value.ToString(); }
        }

        private int _seconds
        {
            get { return GetInteger(SecondsEdit.Text); }
            set { SecondsEdit.Text = value.ToString(); }
        }

        private int _random
        {
            get { return GetInteger(RandomEdit.Text); }
            set { RandomEdit.Text = value.ToString(); }
        }

        public Keys SelectedKey
        {
            get
            {
                Keys temp;
                Enum.TryParse(SelectedKeyEdit.Text, out temp);
                return temp;
            }
            set
            {
                var kc = new KeysConverter();
                SelectedKeyEdit.Text = kc.ConvertToString(value);
            }
        }

        public ClickPlacement ClickPlacement
        {
            get
            {
                ClickPlacement temp;
                Enum.TryParse(((ComboBoxItem)(ClickPlacementEdit.SelectedItem)).Content.ToString(), out temp);
                return temp;
            }

            set
            {
                ClickPlacementEdit.Text = value.ToString();
            }
        }

        public int ClickX
        {
            get { return GetInteger(XEdit.Text); }
            set { XEdit.Text = value.ToString(); }
        }

        public int ClickY
        {
            get { return GetInteger(YEdit.Text); }
            set { YEdit.Text = value.ToString(); }
        }

        public Keys SetCustomKey
        {
            get
            {
                Keys temp;
                Enum.TryParse(SetCustomKeyEdit.Text, out temp);
                return temp;
            }
            set
            {
                var kc = new KeysConverter();
                SetCustomKeyEdit.Text = kc.ConvertToString(value);
            }
        }

        #endregion

        #region Window

        public MainWindow()
        {
            InitializeComponent();
            InitializeNotifyIcon();

            VirtualKeyboard.StartInterceptor();
            VirtualKeyboard.KeyDown += VirtualKeyboardOnKeyDown;

            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _seconds = 0;
            _milliseconds = 10;
            _random = 0;
            SelectedKey = Keys.F2;
            SetCustomKey = Keys.F3;
            ClickPlacement = ClickPlacement.Mouse;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _ni.Visible = false;
            _ni.Dispose();
        }

        #endregion

        #region Notify Icon

        private void InitializeNotifyIcon()
        {
            _ni.Icon = new System.Drawing.Icon("Logo.ico");
            _ni.Visible = true;
            _ni.DoubleClick += delegate { OpenWindow(); };
            _ni.Text = "Eric's AutoClicker";

            var cm = new ContextMenu();
            cm.MenuItems.Add(0, new MenuItem("Open", delegate { OpenWindow(); }));
            cm.MenuItems.Add(1, new MenuItem("Exit", (sender, args) => Close()));
            _ni.ContextMenu = cm;
        }

        private void OpenWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Focus();
        }

        #endregion

        #region Keyboard Hook

        private void VirtualKeyboardOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.KeyCode == SelectedKey)
            {
                SetInterval();
                if (!_dispatcherTimer.IsEnabled) _dispatcherTimer.Start();
                else _dispatcherTimer.Stop();
            }
            else if (keyEventArgs.KeyCode == SetCustomKey)
            {
                var pos = GetMousePositionWindowsForms();
                ClickX = (int)pos.X;
                ClickY = (int)pos.Y;
            }
        }

        #endregion

        #region Click Mouse

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_random > 0)
                SetInterval();

            int x, y;

            var p = GetMousePositionWindowsForms();
            x = (int) p.X;
            y = (int) p.Y;

            if (ClickPlacement == ClickPlacement.Custom)
                SetCursorPos(ClickX, ClickY);

            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            SetCursorPos(x, y);
        }

        public static Point GetMousePositionWindowsForms()
        {
            System.Drawing.Point point = Control.MousePosition;
            return new Point(point.X, point.Y);
        }

        #endregion

        #region Setting SelectedKey

        private void SelectedKeyEdit_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Keys temp;
            Enum.TryParse(e.Key.ToString(), out temp);
            SelectedKey = temp;
        }

        #endregion

        #region Helper

        public int GetInteger(string value)
        {
            int temp;
            int.TryParse(value, out temp);
            return temp;
        }

        public void SetInterval()
        {
            var r = new Random();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, _seconds, _milliseconds + r.Next(0, _random));
        }

        #endregion

        #region Click Placement

        private void ClickPlacementEdit_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlacementPanel.IsEnabled = ClickPlacement == ClickPlacement.Custom;
        }

        private void SetCustomKeyEdit_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Keys temp;
            Enum.TryParse(e.Key.ToString(), out temp);
            SetCustomKey = temp;
        }

        #endregion
    }
}