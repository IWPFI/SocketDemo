using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

                //有可能卡死在while(true)循环里面-->开启一个新的线程不停的接收服务端发来的消息
                Thread th = new Thread(Recive);
                th.IsBackground = true;
                th.Start();
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        /// <summary>
        /// 接收客户端发送过来的消息
        /// </summary>
        private void Recive()
        {
            while (true)
            {
                try
                {
                    //客户端连接成功后，服务器应该接收客户端发来的消息
                    byte[] buffer = new byte[1024 * 1024 * 3];//用来保存接收的数据--接收的是字节类型 

                    //实际接收到的有效字节数
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }

                    //表示发送的是文字消息
                    if (buffer[0] == 0)
                    {
                        string str = Encoding.UTF8.GetString(buffer, 1, r - 1);//转化成能读懂的字符串类型
                        ShowMsg(socketSend.RemoteEndPoint + ":" + str);
                    }
                    else if (buffer[0] == 1)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.InitialDirectory = @"E:\";
                        sfd.Title = "请选择要保存的文件";
                        sfd.Filter = "所有文件|*.*";
                        sfd.ShowDialog();
                        //sfd.ShowDialog(this);
                        string path = sfd.FileName;
                        using (FileStream fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fsWrite.Write(buffer, 1, r - 1);
                        }
                        MessageBox.Show("保存成功");
                    }
                    else if (buffer[0] == 2)
                    {
                        Vibration();
                    }

                }
                catch (Exception ex) { ShowMsg(ex.Message); }
            }

        }

        /// <summary>
        /// 发送按钮
        /// </summary>
        /// <remarks>客户端给服务器发送消息</remarks>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = MsgTextBox.Text.Trim();
                byte[] buffet = System.Text.Encoding.UTF8.GetBytes(str);
                socketSend.Send(buffet);
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        /// <summary>
        /// 震动
        /// </summary>
        private void Vibration()
        {
            try
            {
                for (int i = 0; i < 500; i++)
                {
                    //this.window.WindowStartupLocation = new Point(200, 200);
                    //this.Location = new Point(280, 280);
                    this.window.Dispatcher.Invoke(new Action(delegate
                    {
                        window.Top = 200;
                        window.Top = 220;
                    }));

                }
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        private void ShowMsg(string str)
        {
            try
            {
                this.LogTextBox.Dispatcher.Invoke(new Action(delegate
                    {
                        this.LogTextBox.AppendText(str + "\r\n");
                    }));
            }
            catch { }
        }
    }
}
