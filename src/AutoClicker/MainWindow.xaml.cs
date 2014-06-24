using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using VirtualInput;

namespace AutoClicker
{
    public partial class MainWindow
    {
        #region Properties

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

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
        }

        protected override void OnStateChanged(EventArgs e)
        {
            _ni.Visible = WindowState == WindowState.Minimized;
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
            _ni.Visible = false;
            _ni.DoubleClick +=
                delegate
                {
                    Show();
                    WindowState = WindowState.Normal;
                };
            _ni.Text = "Eric's AutoClicker";

            var cm = new ContextMenu();
            cm.MenuItems.Add(0, new MenuItem("Open", delegate
            {
                Show();
                WindowState = WindowState.Normal;
            }));
            cm.MenuItems.Add(1, new MenuItem("Exit", (sender, args) => Close()));
            _ni.ContextMenu = cm;
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
        }

        #endregion

        #region Click Mouse

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if(_random > 0)
                SetInterval();

            var p = GetMousePositionWindowsForms();
            mouse_event(MOUSEEVENTF_LEFTDOWN, (int)p.X, (int)p.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, (int)p.X, (int)p.Y, 0, 0);
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
            SelectedKey = (Keys)Enum.Parse(typeof(Keys), e.Key.ToString());
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
    }
}