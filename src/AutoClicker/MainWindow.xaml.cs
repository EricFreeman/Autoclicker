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
            get
            {
                int temp;
                int.TryParse(MillisecondsEdit.Text, out temp);
                return temp;
            }
            set { MillisecondsEdit.Text = value.ToString(); }
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

            _ni.Icon = new System.Drawing.Icon("Logo.ico");
            _ni.Visible = true;
            _ni.DoubleClick +=
                delegate
                {
                    Show();
                    WindowState = WindowState.Normal;
                };

            VirtualKeyboard.StartInterceptor();
            VirtualKeyboard.KeyDown += VirtualKeyboardOnKeyDown;

            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _milliseconds = 10;
            SelectedKey = Keys.F2;
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
        }

        #endregion

        #region Keyboard Hook

        private void VirtualKeyboardOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.KeyCode == SelectedKey)
            {
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, _milliseconds);
                if (!_dispatcherTimer.IsEnabled) _dispatcherTimer.Start();
                else _dispatcherTimer.Stop();
            }
        }

        #endregion

        #region Click Mouse

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
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
    }
}