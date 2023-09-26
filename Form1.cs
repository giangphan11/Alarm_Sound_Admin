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
using System.Xml;
namespace SoundClient
{
    public partial class Form1 : Form
    {
        private UdpClient udpClient;
        private IPEndPoint endPoint;

        List<AddressItem> ipAddressList = new List<AddressItem>();
        List<AddressItem> ipAddressListRenameFromXML = new List<AddressItem>();
        List<AddressItem> ipBaseList = new List<AddressItem>();
        List<AddressItem> ipAddressListSelected = new List<AddressItem>();

        private static string fileName = "ListIP.xml";
        private static string rootXMLName = "IPs";

        /// <summary>
        /// TEST
        /// </summary>
        //private static string fileXMLPath = @"E:\Download\IT Study\CSharp\listip.xml";

        /// <summary>
        /// BUILD
        /// </summary>
       private static string fileXMLPath = @"C:\Program Files (x86)\GIANG PHAN BA\SoundAdminSetup\file\listip.xml";

        // Create XML document
        private XmlDocument xmlDoc = new XmlDocument();
        private string ipChangeSelected = "";

        private static int port = 12345;
        public Form1()
        {
            InitializeComponent();
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            loadListRenameFromXML();
            getLocalHost();
        }

        private void loadListRenameFromXML()
        {
            try
            {
                xmlDoc.Load(fileXMLPath);

                // Check if the root element exists
                XmlElement rootElement = xmlDoc.DocumentElement;
                if (rootElement != null && rootElement.Name == rootXMLName)
                {
                    // Get all AD elements in the XML
                    XmlNodeList adNodes = xmlDoc.SelectNodes("//AD");

                    foreach (XmlNode adNode in adNodes)
                    {
                        // Extract ID and Name from each AD element
                        string id = adNode.Attributes["ID"].Value;
                        string name = adNode.SelectSingleNode("Name").InnerText;

                        // Create an IPAddressInfo object and add it to the list
                        ipAddressListRenameFromXML.Add(new AddressItem { ip = id, name = name });
                    }
                }

            }
            catch (Exception ex)
            {
            } 
        }

        private void getLocalHost()
        {
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
                    String ip = e.Reply.Address.ToString().Trim();
                    //if (this.ipAddressList.Contains(ip) == false)
                    if (this.ipAddressList.FirstOrDefault(x => x.ip == ip) == null)
                    {
                        AddressItem addressItem = new AddressItem();
                        addressItem.ip = ip;
                        addressItem.name = this.mapNameFromXML(addressItem.ip);
                        ipAddressList.Add(addressItem);
                        CheckBox cb = new CheckBox();
                        //cb.Text = addressItem.ip;
                        cb.Tag = addressItem;
                        cb.Text = addressItem.name;
                        cb.Click += cb_CheckedChanged;
                        cb.AutoSize = true;
                        flAddress.Controls.Add(cb);

                        // Add items to the ComboBox
                        cbCurrentIP.Items.Add(ip);
                    }
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
                        String ip = ipAddress.ToString().Trim();
                        //if (this.ipAddressList.Contains(ip) == false) {
                        if (this.ipAddressList.FirstOrDefault(x => x.ip == ip) == null) {
                            AddressItem addressItem = new AddressItem();
                            addressItem.ip = ip;
                            addressItem.name = this.mapNameFromXML(addressItem.ip);
                            ipAddressList.Add(addressItem);
                            CheckBox cb = new CheckBox();
                            cb.Tag = addressItem;
                            cb.Text = addressItem.ip;
                            cb.Click += cb_CheckedChanged;
                            cb.AutoSize = true;
                            flAddress.Controls.Add(cb);
                        }
                    }
                }

                //txtIpAddress.Text = string.Join(";", ipAddressList);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while getting IP addresses: " + ex.Message);
            }
        }

        private String mapNameFromXML(String ip) {
            if (this.ipAddressListRenameFromXML.Count == 0) {
                return ip;
            }
            AddressItem addressItem = this.ipAddressListRenameFromXML.FirstOrDefault(x => x.ip == ip);
            if (addressItem == null) {
                return ip;
            }
            return addressItem.name;
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {

            if (sender is CheckBox)
            {
                CheckBox cbClick = (CheckBox)sender;
                
                AddressItem addressItem = (AddressItem) cbClick.Tag;
                if (addressItem == null) { return; }
                if (cbClick.Checked)
                {
                    
                    this.ipAddressListSelected.Add(addressItem);
                }
                else
                {
                    this.ipAddressListSelected.Remove(addressItem);
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

                foreach (AddressItem addressItem in this.ipAddressListSelected)
                {
                    // Chuyển đổi địa chỉ IP và cổng thành IPEndPoint
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(addressItem.ip), port);

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
            ipChangeSelected = selectedValue;
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (isValiIP())
            {
                // Xoá hết data trước đó
                // Neesu
                //ipAddressList.Clear();
                // Clear any existing items
                //cbCurrentIP.Items.Clear();

                //flAddress.Controls.Clear();

                string ipbase = tbIpBase.Text.Trim();
                
                //if (this.ipBaseList.Contains(ipbase))
                if (this.ipBaseList.FirstOrDefault(x => x.ip == ipbase) != null)
                {
                    // Trong danh sách tìm kiếm đã có thì không tìm kiếm nữa
                    return;
                }
                
                string[] ips = ipbase.Split('.');
                String ipCut = "";
                if (ips.Length >= 3)
                {
                    ipCut = string.Join(".", ips[0], ips[1], ips[2]);
                }
                GetAllConnectedIPs(ipCut);
                AddressItem addressItem = new AddressItem();
                addressItem.ip = ipbase;
                addressItem.name = "";
                this.ipBaseList.Add(addressItem);
            }
            else
            {
                MessageBox.Show("Địa chỉ IP không hợp lệ!");
            }
        }



        private void createXML()
        {
            // Create XML document
            XmlDocument xmlDoc = new XmlDocument();

            // Create the root element
            XmlElement rootElement = xmlDoc.CreateElement(rootXMLName);

            xmlDoc.AppendChild(rootElement);

            // Create book elements and add them to the root
            for (int i = 1; i <= 3; i++)
            {
                XmlElement bookElement = xmlDoc.CreateElement("Book");
                bookElement.SetAttribute("ID", i.ToString());

                XmlElement titleElement = xmlDoc.CreateElement("Title");
                titleElement.InnerText = $"Book Title {i}";

                XmlElement authorElement = xmlDoc.CreateElement("Author");
                authorElement.InnerText = $"Author {i}";

                bookElement.AppendChild(titleElement);
                bookElement.AppendChild(authorElement);

                rootElement.AppendChild(bookElement);
            }

            // Save the XML document to a file
            xmlDoc.Save(@"E:\Download\IT Study\CSharp\listip.xml");

            Console.WriteLine("XML data has been written to the file.");
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            // thực hiện đổi định danh
            if (this.ipChangeSelected.Trim().Length <= 0) {
                showMessage("Vui lòng chọn IP cần đổi!");
                return;
            }

            if (this.tbNewIPName.Text.Trim().Length <= 0) {
                showMessage("Tên định danh không được để trống");
                return;
            }
            // Ý tưởng
            // Lưu vào file xml
            // key là ID và value là tên mới
            saveIPName(this.ipChangeSelected.Trim().ToString(), this.tbNewIPName.Text.Trim().ToString());
            
        }

        /// <summary>
        /// Lưu định danh theo IP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="ipName"></param>
        private void saveIPName(string ip, string ipName) {
            try
            {
                xmlDoc.Load(fileXMLPath);

                // Check if the root element exists
                XmlElement rootElement = xmlDoc.DocumentElement;
                if (rootElement != null && rootElement.Name == rootXMLName)
                {
                    

                    // Find the AD element with the specified ID
                    XmlNode adNode = xmlDoc.SelectSingleNode($"//AD[@ID='{ip}']");

                    if (adNode != null)
                    {
                        // ton tai
                        // Extract the Name element's text
                        XmlNode nameNode = adNode.SelectSingleNode("Name");
                       
                        if (nameNode != null)
                        {
                            nameNode.InnerText = ipName;
                            xmlDoc.Save(fileXMLPath);
                            MessageBox.Show("Cập nhật thành công");
                        }
                    }
                    else {
                        XmlElement bookElement = xmlDoc.CreateElement("AD");
                        bookElement.SetAttribute("ID", ip);

                        XmlElement titleElement = xmlDoc.CreateElement("Name");
                        titleElement.InnerText = ipName;
                        bookElement.AppendChild(titleElement);
                        rootElement.AppendChild(bookElement);
                        xmlDoc.AppendChild(rootElement);
                        xmlDoc.Save(fileXMLPath);
                        MessageBox.Show("Lưu thành công");
                    }
                    


                    // Read and display the XML data
                    //XmlNodeList bookNodes = rootElement.SelectNodes("AD");

                    //foreach (XmlNode bookNode in bookNodes)
                    //{
                    //    string id = bookNode.Attributes["ID"].Value;
                    //    string name = bookNode.SelectSingleNode("Name").InnerText;
                        
                    //   // listBoxBooks.Items.Add($"ID: {id}, Title: {title}, Author: {author}");
                    //}
                }
                else
                {
                    // Tạo mới
                    // Create the root element
                    rootElement = xmlDoc.CreateElement(rootXMLName);
                    

                    XmlElement bookElement = xmlDoc.CreateElement("AD");
                    bookElement.SetAttribute("ID", ip);

                    XmlElement titleElement = xmlDoc.CreateElement("Name");
                    titleElement.InnerText = ipName;
                    bookElement.AppendChild(titleElement);
                    rootElement.AppendChild(bookElement);
                    xmlDoc.AppendChild(rootElement);
                    xmlDoc.Save(fileXMLPath);
                    MessageBox.Show("Lưu thành công");
                }
            }
            catch (Exception ex)
            {
                // Tạo mới
                // Create the root element
                XmlElement rootElement = xmlDoc.CreateElement(rootXMLName);

                XmlElement bookElement = xmlDoc.CreateElement("AD");
                bookElement.SetAttribute("ID", ip);

                XmlElement titleElement = xmlDoc.CreateElement("Name");
                titleElement.InnerText = ipName;
                bookElement.AppendChild(titleElement);
                rootElement.AppendChild(bookElement);
                xmlDoc.AppendChild(rootElement);
                xmlDoc.Save(fileXMLPath);
                MessageBox.Show("Lưu thành công");
            }
        }

        /// <summary>
        /// Show thông báo
        /// </summary>
        /// <param name="content"></param>
        private void showMessage(string content) {
            MessageBox.Show(content);
        }
    }
}
