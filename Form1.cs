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

        List<string> ipAddressList = new List<string>();
        List<string> ipAddressListSelected = new List<string>();

        private static int port = 12345;
        public Form1()
        {
            InitializeComponent();
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            scanLanDevice();
        }

        private void scanLanDevice() {
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
            
            if (sender is CheckBox) {
                CheckBox cbClick = (CheckBox)sender;
                String ipAddress = cbClick.Text.ToString().Trim();
                if (cbClick.Checked)
                {
                    this.ipAddressListSelected.Add(ipAddress);
                }
                else {
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
            else {
                // Gửi theo từng địa chỉ ip đã chọn
                if (this.ipAddressListSelected.Count <= 0) {
                    MessageBox.Show("Vui lòng chọn IP!");
                    return;
                }
                MessageBox.Show("IP đã chọn" + string.Join(";", this.ipAddressListSelected));
                actionOnlySend();
            }
            
        }

        private void stopSound_Click(object sender, EventArgs e)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("StopSound");
            udpClient.Send(data, data.Length, endPoint);
            MessageBox.Show("Đã dừng âm báo");
        }


        private void actionOnlySend()
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
                        byte[] data = System.Text.Encoding.ASCII.GetBytes("PlaySound");
                        udpClient.Send(data, data.Length, endPoint);
                    }

                    MessageBox.Show("Horn signal sent to " + ipAddress + ":" + port);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while sending horn signal: " + ex.Message);
            }
        }
    }
}
