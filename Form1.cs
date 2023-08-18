using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace SoundClient
{
    public partial class Form1 : Form
    {
        private UdpClient udpClient;
        private IPEndPoint endPoint;

        private static int port = 12345;
        public Form1()
        {
            InitializeComponent();
            // scanIPDevice();

            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            scan2();
        }

        private void scan2() {
            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);

                List<string> ipAddressList = new List<string>();

                foreach (IPAddress ipAddress in ipAddresses)
                {
                    if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipAddressList.Add(ipAddress.ToString());
                        CheckBox cb = new CheckBox();
                        cb.Text = ipAddress.ToString();
                        flAddress.Controls.Add(cb);
                    }
                }

                txtIpAddress.Text = string.Join(";", ipAddressList);
                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while getting IP addresses: " + ex.Message);
            }
        }

        private void scanIPDevice() {
            // Địa chỉ IP mạng LAN của bạn, thay đổi địa chỉ này tùy theo dải mạng của bạn
            string lanIpBase = "192.168.0.";
            // int port = port; // Port của ứng dụng đang chạy trên máy tính mục tiêu

            Console.WriteLine("Danh sách máy tính đang kết nối trong cùng một dải mạng LAN:");

            for (int i = 1; i < 255; i++)
            {
                string ipAddress = lanIpBase + i.ToString();
                try
                {
                    UdpClient udpClient = new UdpClient();
                    udpClient.Connect(ipAddress, port);

                    // Gửi gói tin UDP tới địa chỉ IP của máy tính mục tiêu
                    byte[] sendBytes = new byte[1];
                    udpClient.Send(sendBytes, sendBytes.Length);

                    // Nhận gói tin UDP từ máy tính mục tiêu (Timeout 100ms)
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
                    udpClient.Client.ReceiveTimeout = 100;
                    byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);

                    // Nếu không có lỗi xảy ra, máy tính mục tiêu đang hoạt động
                    Console.WriteLine($"Máy tính có địa chỉ IP: {ipAddress} đang hoạt động.");

                    udpClient.Close();
                }
                catch (SocketException)
                {
                    // Ignored, máy tính không phản hồi hoặc có lỗi
                }
            }

            Console.ReadLine();
        }

        private void playSound_Click(object sender, EventArgs e)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("PlaySound");
            udpClient.Send(data, data.Length, endPoint);
        }

        private void stopSound_Click(object sender, EventArgs e)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("StopSound");
            udpClient.Send(data, data.Length, endPoint);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
