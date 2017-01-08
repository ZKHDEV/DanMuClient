using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DanMuClient
{
    /// <summary>
    /// DanMuPage.xaml 的交互逻辑
    /// </summary>
    public partial class DanMuPage : Window
    {
        public DanMuPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Window penetration
        /// </summary>
        #region Code of window penetration
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }
        #endregion
        public double screenHeight = SystemParameters.PrimaryScreenHeight;
        public double screenWidth = SystemParameters.PrimaryScreenWidth;

        private bool enableShadowEffect = false;

        private DanMuManager dm = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //全屏显示
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Topmost = true;

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
        }

        public void Shoot(string text,OutlinedDanMu danmu, int entrynum, int speed)
        {
            if (dm == null)
            {
                dm = new DanMuManager(DanMuGrid, enableShadowEffect);
            }

            dm.Shoot(text, danmu, entrynum, speed);
        }
    }

    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
