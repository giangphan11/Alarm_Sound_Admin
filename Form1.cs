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
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;

namespace SoundClient
{
    public partial class Form1 : Form
    {
        private UdpClient udpClient;
        private IPEndPoint endPoint;

        List<string> ipAddressList = new List<string>();
        List<string> ipAddressListSelected = new List<string>();

        private static int port = 12345;
        public Form1()
        {
            InitializeComponent();
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            //scanLanDevice();
            // test();
            //GetAllConnectedIPs();
            //sanLanDevice2();
            getLocalHost();
            
        }

        private void getLocalHost() {
            // Get the local host's IP address
            string hostName = Dns.GetHostName();
            IPAddress[] localIPs = Dns.GetHostAddresses(hostName);

            foreach (var ipAddress in localIPs)
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    Console.WriteLine($"Local IP Address: {ipAddress}");
                    tbIpBase.Text = $"{ipAddress}";
                    break;
                }
            }
        }

        private void GetAllConnectedIPs(string ipBase)
        {
            // Get a list of all devices in the network
            string subnet = ipBase;
            for (int i = 1; i < 255; i++)
            {
                string targetIP = $"{subnet}.{i}";
                PingDevice(targetIP);
            }
        }

        static string GetSubnet(IPAddress ipAddress)
        {
            string[] octets = ipAddress.ToString().Split('.');
            if (octets.Length == 0)
            {
                return "";
            }
            if (octets.Length == 1)
            {
                return $"{octets[0]}";
            }
            if (octets.Length == 2)
            {
                return $"{octets[0]}.{octets[1]}";
            }
            return $"{octets[0]}.{octets[1]}.{octets[2]}";
        }

        void PingDevice(string targetIP)
        {
            Ping ping = new Ping();
            ping.PingCompleted += (sender, e) =>
            {
                if (e.Reply == null)
                {
                    return;
                }
                if (e.Reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"Connected IP: {e.Reply.Address}");
                    ipAddressList.Add(e.Reply.Address.ToString());
                    CheckBox cb = new CheckBox();
                    cb.Text = e.Reply.Address.ToString();
                    cb.Click += cb_CheckedChanged;
                    cb.AutoSize = true;
                    flAddress.Controls.Add(cb);
                    
                    // Add items to the ComboBox
                    cbCurrentIP.Items.Add(e.Reply.Address.ToString());
                    
                }
            };

            ping.SendAsync(targetIP, 1000, null);
        }


        private void scanLanDevice()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);

                foreach (IPAddress ipAddress in ipAddresses)
                {
                    if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipAddressList.Add(ipAddress.ToString());
                        CheckBox cb = new CheckBox();
                        cb.Text = ipAddress.ToString();
                        cb.Click += cb_CheckedChanged;
                        cb.AutoSize = true;
                        flAddress.Controls.Add(cb);
                    }
                }

                //txtIpAddress.Text = string.Join(";", ipAddressList);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while getting IP addresses: " + ex.Message);
            }
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {

            if (sender is CheckBox)
            {
                CheckBox cbClick = (CheckBox)sender;
                String ipAddress = cbClick.Text.ToString().Trim();
                if (cbClick.Checked)
                {
                    this.ipAddressListSelected.Add(ipAddress);
                }
                else
                {
                    this.ipAddressListSelected.Remove(ipAddress);
                }
            }
        }

        private void playSound_Click(object sender, EventArgs e)
        {
            //1. Check xem có đang click gửi tất cả hay không?
            if (cbSendAll.Checked)
            {
                // Gửi tất cả
                byte[] data = System.Text.Encoding.ASCII.GetBytes("PlaySound");
                udpClient.Send(data, data.Length, endPoint);
                MessageBox.Show("Đã gửi tín hiệu đến tất cả các thiết bị");
            }
            else
            {
                // Gửi theo từng địa chỉ ip đã chọn
                if (this.ipAddressListSelected.Count <= 0)
                {
                    MessageBox.Show("Vui lòng chọn IP!");
                    return;
                }
                actionOnlySend("PlaySound");
                MessageBox.Show("IP đã chọn" + string.Join(";", this.ipAddressListSelected));
            }

        }

        private void stopSound_Click(object sender, EventArgs e)
        {
            //1. Check xem có đang click gửi tất cả hay không?
            if (cbSendAll.Checked)
            {
                // Gửi tất cả
                byte[] data = System.Text.Encoding.ASCII.GetBytes("StopSound");
                udpClient.Send(data, data.Length, endPoint);
                MessageBox.Show("Đã dừng âm báo");
            }
            else
            {
                // Gửi theo từng địa chỉ ip đã chọn
                if (this.ipAddressListSelected.Count <= 0)
                {
                    MessageBox.Show("Vui lòng chọn IP!");
                    return;
                }
                actionOnlySend("StopSound");
                MessageBox.Show("IP đã chọn" + string.Join(";", this.ipAddressListSelected) + " dừng!");
            }


        }


        private void actionOnlySend(string key)
        {
            try
            {
                // string[] ipAddressList = txtIpAddress.Text.Split(';'); // Lấy danh sách địa chỉ IP từ người dùng, phân tách bằng dấu chấm phẩy (;)
                int port = 12345; // Cổng mà máy nhận sẽ lắng nghe (giống như ở ứng dụng nhận)

                foreach (string ipAddress in this.ipAddressListSelected)
                {
                    // Chuyển đổi địa chỉ IP và cổng thành IPEndPoint
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

                    using (UdpClient udpClient = new UdpClient())
                    {
                        // Gửi thông điệp báo còi thông qua giao thức UDP
                        byte[] data = System.Text.Encoding.ASCII.GetBytes(key);
                        udpClient.Send(data, data.Length, endPoint);
                    }

                    //MessageBox.Show("Horn signal sent to " + ipAddress + ":" + port);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while sending horn signal: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("StopSound");
            udpClient.Send(data, data.Length, endPoint);
        }

        private bool isValiIP()
        {
            if (tbIpBase.Text.Trim().Length <= 0)
            {
                return false;
            }

            string ipAddressPattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\."
                               + @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\."
                               + @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\."
                               + @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

            Regex regex = new Regex(ipAddressPattern);

            Console.Write("Enter an IP address: ");
            string input = tbIpBase.Text.Trim().ToString();
            if (regex.IsMatch(input))
            {
                Console.WriteLine("Valid IP address.");
                return true;
            }
            else
            {
                Console.WriteLine("Invalid IP address.");
                return false;
            }
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle the selection change event here
            // For example:
             string selectedValue = cbCurrentIP.SelectedItem.ToString();
            MessageBox.Show("Selected: " + selectedValue);
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (isValiIP())
            {
                // Xoá hết data trước đó
                ipAddressList.Clear();
                // Clear any existing items
                cbCurrentIP.Items.Clear();

                flAddress.Controls.Clear();
                string ipbase = "";
                string[] ips = tbIpBase.Text.Trim().Split('.');
                if (ips.Length >= 3)
                {
                    ipbase = string.Join(".", ips[0], ips[1], ips[2]);
                }
                GetAllConnectedIPs(ipbase);
            }
            else
            {
                MessageBox.Show("Địa chỉ IP không hợp lệ!");
            }
        }
    }
}
