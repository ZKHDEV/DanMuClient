using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DanMuClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            dmp = new DanMuPage();
            dmp.Show();
            buffer = new byte[1024];
        }

        private Socket client;
        private byte[] buffer;

        #region 弹幕设置
        private DanMuPage dmp;

        //private void StartBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (StartBtn.Content.Equals("开始播放弹幕"))
        //    {
        //        dmp.Show();
        //        StartBtn.Content = "停止播放";
        //    }
        //    else
        //    {
        //        dmp.Hide();
        //        StartBtn.Content = "开始播放弹幕";
        //    }
        //}

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShootDanMu(string text, OutlinedDanMu danmu, int entrynum, int speed)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                dmp.Shoot(text, danmu, entrynum, speed);
            }));
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = UserNameTextBox.Text.Trim();
            string message = DanMuTextBox.Text.Trim();
            if (message != "")
            {
                Send(username+":"+message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //字体样式列表
            foreach (FontFamily _f in Fonts.SystemFontFamilies)
            {
                LanguageSpecificStringDictionary _fontDic = _f.FamilyNames;
                if (_fontDic.ContainsKey(XmlLanguage.GetLanguage("zh-cn")))
                {
                    string _fontName = null;
                    if (_fontDic.TryGetValue(XmlLanguage.GetLanguage("zh-cn"), out _fontName))
                    {
                        FontStyleBox.Items.Add(_fontName);
                    }
                }
                else
                {
                    string _fontName = null;
                    if (_fontDic.TryGetValue(XmlLanguage.GetLanguage("en-us"), out _fontName))
                    {
                        FontStyleBox.Items.Add(_fontName);
                    }
                }
            }

            //字体大小列表
            for (int i = 5; i <= 72; i += 5)
            {
                FontSizeBox.Items.Add(i.ToString());
            }

            FontStyleBox.SelectedValue = "宋体";
            FontSizeBox.SelectedValue = "45";
        }
        #endregion

        #region socket接收数据
        /// <summary>
        /// 建立连接
        /// </summary>
        private void Connect(string ip, string port)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress serverIP = IPAddress.Parse(ip);
            int serverPort = int.Parse(port);
            IPEndPoint serverAddress = new IPEndPoint(serverIP, serverPort);

            try
            {
                IAsyncResult result = client.BeginConnect(serverAddress, null, null);
                client.EndConnect(result);
            }
            catch
            {
                DisconnectOperate();
                return;
            }

            //开始异步接收信息
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Recived), client);
        }

        /// <summary>
        /// 接收弹幕
        /// </summary>
        /// <param name="result"></param>
        private void Recived(IAsyncResult result)
        {
            try
            {
                int length = client.EndReceive(result);
                string message = Encoding.UTF8.GetString(buffer, 0, length);

                if (message.Trim() != "")
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ShowDanMu(message);
                    }));
                }

                buffer = null;
                buffer = new byte[1024];
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Recived), client);
            }
            catch
            {
                DisconnectOperate();
            }
        }

        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <param name="txt"></param>
        private void Send(string txt)
        {
            byte[] sendbuffer = new byte[1024];
            sendbuffer = Encoding.UTF8.GetBytes(txt);
            try
            {
                IAsyncResult result = client.BeginSend(sendbuffer, 0, sendbuffer.Length, SocketFlags.None, null, null);
                client.EndSend(result);
            }
            catch
            {
                DisconnectOperate();
            }
        }

        /// <summary>
        /// 显示弹幕
        /// </summary>
        /// <param name="text"></param>
        private void ShowDanMu(string text)
        {
            int entryNum = Int32.Parse(EntryNumSlider.Value.ToString().Trim());
            int speed = Int32.Parse(SpeedSlider.Value.ToString().Trim());

            OutlinedDanMu DanMu = new OutlinedDanMu(text);
            DanMu.FontSize = Int32.Parse(FontSizeBox.Text.Trim());
            DanMu.FontFamily = new FontFamily(FontStyleBox.Text.Trim());

            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);

            if (CustomColorBtn.IsChecked == true && ColorPickerBox.Text != "")
            {
                string color = ColorPickerBox.Text.Trim();

                color = color.Substring(color.LastIndexOf(" "));
                DanMu.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            }
            else
            {
                //随机颜色
                int int_Red = RandomNum_First.Next(256);
                int int_Green = RandomNum_Sencond.Next(256);
                int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
                int_Blue = (int_Blue > 255) ? 255 : int_Blue;
                DanMu.Fill = new SolidColorBrush(Color.FromRgb((byte)int_Red, (byte)int_Green, (byte)int_Blue));
            }

            ShootDanMu(text, DanMu, entryNum, speed);
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            string serverip = ServerIPTextBox.Text.Trim();
            string port = PortTextBox.Text.Trim();
            string username =  UserNameTextBox.Text.Trim();

            if (serverip != "" && port != "" && username != "")
            {
                if (Regex.IsMatch(serverip, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}")
                    && Regex.IsMatch(port, "[0-9]{1,5}")
                    && Convert.ToInt32(port) < 65536
                    && Convert.ToInt32(port) > 0)
                {
                    ConnectBtn.IsEnabled = false;
                    CloseBtn.IsEnabled = true;
                    ServerIPTextBox.IsReadOnly = true;
                    PortTextBox.IsReadOnly = true;
                    UserNameTextBox.IsReadOnly = true;
                    Task task = Task.Factory.StartNew(new Action(()=> {
                        Connect(serverip,port);
                    }));
                }
                else
                {
                    MessageBox.Show("服务器IP地址或端口格式错误！", "提示", MessageBoxButton.OK,MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("服务器地址、端口或用户名不能为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DisconnectOperate()
        {
            if (client != null)
            {
                client.Close();
                client.Dispose();
                client = null;
            }

            Dispatcher.Invoke(new Action(()=> {
                ConnectBtn.IsEnabled = true;
                CloseBtn.IsEnabled = false;
                ServerIPTextBox.IsReadOnly = false;
                PortTextBox.IsReadOnly = false;
                UserNameTextBox.IsReadOnly = false;
            }));

            MessageBox.Show("连接断开", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DisconnectOperate();
        }

        #endregion

    }
}
