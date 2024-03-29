﻿using HandyControl.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Window = System.Windows.Window;

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

        /// <summary>
        /// 负责通信的Socket
        /// </summary>
        Socket socketSend;

        /// <summary>
        /// 将远程连接的客户端的IP地址和Socket存入集合中
        /// </summary>
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //当点击开始监听时 在服务器创建一个负责IP地址跟端口号的Socket
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Any;//IPAddress.Parse(serverTextBox.Text);

                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(portTextBox.Text));//创建端口号对象

                //监听
                socketWatch.Bind(point);
                ShowMsg("监听成功");
                socketWatch.Listen(10);//设置服务端  时间点内最大容纳数

                Thread th = new Thread(Listen);
                th.IsBackground = true;//设置后台运行
                th.Start(socketWatch);
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        /// <summary>
        /// 等待客户端的连接 并且创建一个负责通信的Socket
        /// </summary>
        private void Listen(object o)
        {
            try
            {
                Socket socketWatch = o as Socket; //线程只能传递一个object的参数,将传进来的参数进行装换

                //等待客户端的连接 并且创建一个负责通信的Socket
                while (true)
                {
                    socketSend = socketWatch.Accept();

                    //将远程连接的客户端的IP地址和Socket存入集合中
                    dicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);

                    // 在新的线程中检测客户端是否掉线
                    Task newTask = Task.Run(() => CheckClientStatus(socketSend));

                    //将远程连接的客户端的IP地址和端口号存入下拉框中
                    this.UsersCombox.Dispatcher.Invoke(new Action(delegate
                    {
                        UsersCombox.Items.Add(socketSend.RemoteEndPoint.ToString());
                    }));

                    //cmd连接服务端命令：telnet 172.28.112.1 50000
                    ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功");

                    Thread th = new Thread(Recive);
                    th.IsBackground = true;
                    th.Start(socketSend);

                }
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        /// <summary>
        /// 服务端检查客户端是否掉线
        /// </summary>
        /// <param name="clientSocket"></param>
        private void CheckClientStatus(Socket clientSocket)
        {
            while (true)
            {
                // 检查客户端连接状态
                bool isClientConnected = clientSocket.Poll(1000, SelectMode.SelectRead);

                if (isClientConnected)
                {
                    Console.WriteLine("客户端已掉线");

                    // 从字典中移除已断开连接的客户端
                    dicSocket.Remove(clientSocket.RemoteEndPoint.ToString());

                    //将远程连接的客户端的IP地址和端口号移除下拉框
                    this.UsersCombox.Dispatcher.Invoke(new Action(delegate
                    {
                        UsersCombox.Items.Remove(socketSend.RemoteEndPoint.ToString());
                    }));

                    break;
                }
            }
        }

        /// <summary>
        /// 接收客户端发送过来的消息
        /// </summary>
        private void Recive(object o)
        {
            try
            {
                Socket socketSend = o as Socket;
                while (true)
                {
                    //客户端连接成功后，服务器应该接收客户端发来的消息
                    byte[] buffer = new byte[1024 * 1024 * 2];//用来保存接收的数据--接收的是字节类型

                    int r = socketSend.Receive(buffer);//实际接收到的有效字节数

                    if (r == 0)
                    {
                        break;
                    }

                    string str = Encoding.UTF8.GetString(buffer, 0, r);//转化成能读懂的字符串类型
                    ShowMsg(socketSend.RemoteEndPoint + ":" + str);
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

        /// <summary>
        /// 发送按钮
        /// </summary>
        /// <remarks>服务器给客户端发送消息</remarks>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = MsgTextBox.Text;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);//转化为二进制数组发送

                //socketSend.Send(buffet);

                List<byte> list = new List<byte>();
                list.Add(0);
                list.AddRange(buffer);
                //将泛型集合转换未数组
                byte[] newBuffer = list.ToArray();

                //获取用户在下拉框中选中的Ip地址
                string ip = UsersCombox.SelectedItem.ToString();
                dicSocket[ip].Send(newBuffer);
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        /// <summary>
        /// 选择要发送的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"E:\桌面";
                ofd.Title = "请选择要发送的问价";
                ofd.Filter = "所有文件|*.*";
                ofd.ShowDialog();

                pathTextBox.Text = ofd.FileName;
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        /// <summary>
        /// 发送文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //获取要发送文件的路径
                string path = pathTextBox.Text;
                using (FileStream fsRead = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[1024 * 1024 * 5];
                    int r = fsRead.Read(buffer, 0, buffer.Length);

                    List<byte> list = new List<byte>();
                    list.Add(1);
                    list.AddRange(buffer);
                    byte[] newBuffer = list.ToArray();

                    dicSocket[UsersCombox.SelectedItem.ToString()].Send(newBuffer, 0, r + 1, SocketFlags.None);
                }
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        /// <summary>
        /// 震动按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VibrationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] buffer = new byte[1];
                buffer[0] = 2;
                dicSocket[UsersCombox.SelectedItem.ToString()].Send(buffer);
            }
            catch (Exception ex) { ShowMsg(ex.Message); }
        }

        private void EmptyTextButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = string.Empty;
        }
    }
}
