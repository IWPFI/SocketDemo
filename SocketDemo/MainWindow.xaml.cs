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

namespace SocketDemo
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //当点击开始监听时 在服务器创建一个负责IP地址跟端口号的Socket
            Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Any;//IPAddress.Parse(serverTextBox.Text);
            //创建端口号对象
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(portTextBox.Text));
            //监听
            socketWatch.Bind(point);
            ShowMsg("监听成功");
            socketWatch.Listen(10);//设置服务端  时间点内最大容纳数

            //等待客户端的连接 并且创建一个负责通信的Socket
            Socket socketSend = socketWatch.Accept();

            //cmd连接服务端命令：telnet 172.28.112.1 50000
            ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功");
        }

        private void ShowMsg(string str)
        {
            LogTextBox.AppendText(str + "\r\n");
        }
    }
}
