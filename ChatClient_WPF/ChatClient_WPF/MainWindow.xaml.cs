﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;
using System.Net.Sockets;
using System.Threading;

namespace ChatClient_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient clientSocket = new TcpClient();
        NetworkStream netstream = default(NetworkStream);
        string indata = null;
        bool fir = true;
        public MainWindow()
        {
            InitializeComponent();
        }
        public bool connectToServer()
        { return clientSocket.Connected; }


        private void userName_MouseEnter(object sender, MouseEventArgs e)
        {
            if(fir)
                userName.Text = "";
            fir = false;
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                userName.IsReadOnly = true;
                clientSocket.Connect("140.112.18.208", 8888);
                netstream = clientSocket.GetStream();
                byte[] outdata = System.Text.Encoding.ASCII.GetBytes((userName.Text+"\x01").ToCharArray());
                netstream.Write(outdata, 0, outdata.Length);
                netstream.Flush();
                Thread ctThread = new Thread(getMessage);
                ctThread.Start();
            }
            catch
            {
                MessageBox.Show("Something's Wrong!");
            }
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            if (connectToServer())
            {
                try
                {
                    string outdata = userName.Text.ToString() + "\x01" + chatText.Text.ToString() + "\x01";
                    byte[] outSt = new byte[clientSocket.ReceiveBufferSize];
                    outSt = System.Text.Encoding.ASCII.GetBytes(outdata.ToCharArray());
                    netstream.Write(outSt, 0, outdata.Length);
                    netstream.Flush();
                }
                catch
                {
                    MessageBox.Show("Stupid Daeno!");
                }
            }
            else
            {
                indata = ">> "+ userName.Text.ToString() + " says: " + chatText.Text.ToString();
                msg();
            }
            chatText.Text = null;
        }

        private void msg()
        {
            if(chatDisplay.Dispatcher.CheckAccess())
            {
                chatDisplay.Text = chatDisplay.Text + Environment.NewLine + indata;
                chatDisplay.ScrollToEnd();
            }
            else
            {
                chatDisplay.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(msg));
            }
        }

        private void getMessage()
        {
            while (true)
            {
                try
                {
                    if (clientSocket.Connected)
                    {
                        netstream = clientSocket.GetStream();
                        byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
                        netstream.Read(inStream, 0, clientSocket.ReceiveBufferSize);
                        indata = System.Text.Encoding.ASCII.GetString(inStream);
                        indata = indata.Substring(0, indata.IndexOf('\x01'));
                        msg();
                    }
                    else
                    {
                        break;
                    }
                }
                catch(Exception e)
                {
                    //MessageBox.Show(e.ToString());
                    return;
                }
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.clientSocket.Close();
            base.OnClosing(e);
        }

        private void chatText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonSend.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent,buttonSend));
            }
        }

        private void userName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


    }
}
