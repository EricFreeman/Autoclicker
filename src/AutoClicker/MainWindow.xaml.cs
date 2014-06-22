using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace AutoClicker
{
    public partial class MainWindow
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        private DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var p = GetMousePositionWindowsForms();

            mouse_event(MOUSEEVENTF_LEFTDOWN, (int)p.X, (int)p.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, (int)p.X, (int)p.Y, 0, 0);
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        public static Point GetMousePositionWindowsForms()
        {
            System.Drawing.Point point = Control.MousePosition;
            return new Point(point.X, point.Y);
        }
    }
}
