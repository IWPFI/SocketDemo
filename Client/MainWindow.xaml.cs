using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 负责通信的Socket
        /// </summary>
        Socket socketSend;

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //创建负责通信的Socket
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(serverTextBox.Text);//IPAddress.Any;
                //创建端口号对象
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(portTextBox.Text));
                //获取要连接的远程服务器应用程序的IP地址和端口号
                socketSend.Connect(point);
                ShowMsg("连接成功");
            }
            catch { }
        }

        /// <summary>
        /// 发送按钮
        /// </summary>
        /// <remarks>客户端给服务器发送消息</remarks>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string str = MsgTextBox.Text.Trim();
            byte[] buffet = System.Text.Encoding.UTF8.GetBytes(str);
            socketSend.Send(buffet);
        }

        private void ShowMsg(string str)
        {
            this.LogTextBox.Dispatcher.Invoke(new Action(delegate
            {
                this.LogTextBox.AppendText(str + "\r\n");
            }));
        }
    }
}
