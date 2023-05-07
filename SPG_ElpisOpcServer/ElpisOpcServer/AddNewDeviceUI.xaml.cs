#region Namespaces
using System.Windows;
using OPCEngine;
using System.Net;
using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Elpis.Windows.OPC.Server;
using System.Text;
using OPCEngine.Connectors.Allen_Bradley;
#endregion End Of Namespaces

#region Elpis OPC Server Namespace
namespace ElpisOpcServer
{
    #region DevicesWindow
    /// <summary>
    /// Interaction logic for Devices.xaml
    /// </summary>
    public partial class AddNewDeviceUI : Window
    {
       
        #region Variable Members and Properties
        public dynamic SelectedProtocol { get; set; }
        public ModbusEthernetDevice modbusEthernetDevice { get; set; }
        public ModbusSerialDevice modbusSerialDevice { get; set; }
        public ABControlLogicDevice abControlLogicDevice { get; set; }
        public ABMicrologixEthernetDevice abMicroLogicEDevice { get; set; }

        public TcpSocketDevice tcpSocketDevice { get; set; }
        public DeviceBase obj { get; set; }
        public bool flag { get; set; }

        #endregion End Of Variable Memebers and Properties

        #region Constructor
        public AddNewDeviceUI(dynamic Protocol)
        {
            try
            {

                InitializeComponent();
                SelectedProtocol = Protocol;
                SelectDeviceType(Protocol);
            }
            catch (Exception ex)
            {

                ElpisServer.Addlogs("Device", @"Device", ex.Message, LogStatus.Error);
            }
            //DevicePropertyGrid.SelectedObject = modbusEthernetDevice;

        }

        private void SelectDeviceType(dynamic connector)
        {
            // SelectDeviceType(Protocol);
            if (connector.TypeofConnector == ConnectorType.ModbusEthernet)
            {
                obj = DeviceFactory.GetDeviceObj(ConnectorType.ModbusEthernet);

                modbusEthernetDevice = obj as ModbusEthernetDevice;
                int noOfDevices = 0;
                if (connector.DeviceCollection != null)
                    noOfDevices = connector.DeviceCollection.Count;
                modbusEthernetDevice.DeviceName = "Device" + ++noOfDevices;//change deviceNaming
                modbusEthernetDevice.DeviceType = DeviceType.ModbusEthernet;
               //modbusEthernetDevice.ConnectorAssignment = connector.ConnectorName;
                modbusEthernetDevice.RetryCount = 3;
                DevicePropertyGrid.SelectedObject = modbusEthernetDevice;
            }

            else if (connector.TypeofConnector == ConnectorType.ModbusSerial)
            {
                obj = DeviceFactory.GetDeviceObj(ConnectorType.ModbusSerial);
                modbusSerialDevice = obj as ModbusSerialDevice;
                int noOfDevices = 0;
                if (connector.DeviceCollection != null)
                    noOfDevices = connector.DeviceCollection.Count;
                modbusSerialDevice.DeviceName = "Device" + ++noOfDevices; ;
                modbusSerialDevice.DeviceType = DeviceType.ModbusSerial;
               // modbusSerialDevice.ConnectorAssignment = connector.ConnectorName;
                modbusSerialDevice.BaudRate = 9600;
                modbusSerialDevice.COMPort = "COM1";
                modbusSerialDevice.ConnectorParityBit = System.IO.Ports.Parity.None;
                modbusSerialDevice.ConnectorStopBits = System.IO.Ports.StopBits.One;
                modbusSerialDevice.DataBits = 8;
                modbusSerialDevice.RetryCount = 3;
                modbusSerialDevice.SlaveId = 1;//Convert.ToByte(noOfDevices);
                DevicePropertyGrid.SelectedObject = modbusSerialDevice;
            }
            else if (connector.TypeofConnector == ConnectorType.ABControlLogix)
            {
                obj = DeviceFactory.GetDeviceObj(ConnectorType.ABControlLogix);
                abControlLogicDevice = obj as ABControlLogicDevice;
                int noOfDevices = 0;
                if (connector.DeviceCollection != null)
                    noOfDevices = connector.DeviceCollection.Count;
                abControlLogicDevice.DeviceName = "Device" + ++noOfDevices; ;
                abControlLogicDevice.DeviceType = DeviceType.ABControlLogix;
                abControlLogicDevice.RetryCount = 3;
                //abControlLogicDevice.ConnectorAssignment = connector.ConnectorName;
                DevicePropertyGrid.SelectedObject = abControlLogicDevice;
            }

            else if(connector.TypeofConnector == ConnectorType.ABMicroLogixEthernet)
            {
                obj = DeviceFactory.GetDeviceObj(ConnectorType.ABMicroLogixEthernet);
                abMicroLogicEDevice = obj as ABMicrologixEthernetDevice;
                int noOfDevices = 0;
                if (connector.DeviceCollection != null)
                    noOfDevices = connector.DeviceCollection.Count;
                abMicroLogicEDevice.DeviceName = "Device" + ++noOfDevices; 
                abMicroLogicEDevice.DeviceType = DeviceType.ABMicroLogixEthernet;
                abMicroLogicEDevice.RetryCount = 3;
                //abMicroLogicEDevice.ConnectorAssignment = connector.ConnectorName;
                DevicePropertyGrid.SelectedObject = null;
                DevicePropertyGrid.SelectedObject = abMicroLogicEDevice;
            }
            else if(connector.TypeofConnector==ConnectorType.TcpSocket)
            {
                obj = DeviceFactory.GetDeviceObj(ConnectorType.TcpSocket);
                tcpSocketDevice = obj as TcpSocketDevice;
                int noOfDevice = 0;
                if (connector.DeviceCollection != null)
                    noOfDevice = connector.DeviceCollection.Count;
                tcpSocketDevice.DeviceName = "Device" + ++noOfDevice;
                tcpSocketDevice.DeviceType = DeviceType.TcpSocketDevice;
                tcpSocketDevice.RetryCount = 3;
                //tcpSocketDevice.ConnectorAssignment = connector.ConnectorName;
                DevicePropertyGrid.SelectedObject = null;
                DevicePropertyGrid.SelectedObject = tcpSocketDevice;
            }

        }

        public AddNewDeviceUI(dynamic selectedProtocol, DeviceBase selectedDevice)
        {
            InitializeComponent();
            SelectedProtocol = selectedProtocol;
            DevicePropertyGrid.SelectedObject = selectedDevice;
        }

        #endregion End Of Constructor

        #region Reset Function
        public void Reset()
        {
            modbusEthernetDevice = null;
            abControlLogicDevice = null;
        }
        #endregion Reset Function

        #region FinishBtn Click Event
        private void FinishBtn_click(object sender, RoutedEventArgs e)
        {
            dynamic currentObject = DevicePropertyGrid.SelectedObject as object;
            #region Tag Group Validation
            if (DevicePropertyGrid.SelectedObjectType.Name == "TagGroup")
            {
                this.Title = "Add New Tag Group";
                if (currentObject.GroupName != null)
                {
                    bool validName = Util.CheckValidName(currentObject.GroupName);
                    if (validName)
                    {

                        if (cmboxDevice.SelectedIndex != -1)
                        {
                            if (currentObject.GroupName.ToLower() == "none")
                            {
                                StringBuilder message = new StringBuilder();
                                message.AppendLine(@"Group Name cannot contains 'None'.");
                                message.AppendLine("The Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.");
                                MessageBox.Show(message.ToString(), "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                flag = true;
                                this.Hide();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please Select Device Name from the list to create a Tag Group.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid Tag Group Name.\nThe Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Tag Group Name will not be empty.\nThe Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            #endregion Tag Group Validation

            else
            {
                if (this.Title == "Add New Device")
                {
                    if (currentObject.DeviceName != "" & currentObject.DeviceName != null)
                    {
                        //if (ConfigurationSettingsUI.isEdited == false)
                        //{
                        bool validName = Util.CheckValidName(currentObject.DeviceName);
                        if (validName)
                        {
                            bool isNewDevice = ConfigurationSettingsUI.elpisServer.IsNewDevice(SelectedProtocol, currentObject);

                            if (isNewDevice == true)
                            {
                                if (currentObject.DeviceType == DeviceType.ModbusEthernet || currentObject.DeviceType==DeviceType.ABMicroLogixEthernet|| currentObject.DeviceType==DeviceType.TcpSocketDevice)
                                {
                                    if (currentObject.IPAddress != "" & currentObject.IPAddress != null)
                                    {
                                        //Regular Expression for IP Address
                                        bool isCorrectId = Regex.IsMatch(currentObject.IPAddress, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
                                        if (isCorrectId == true)
                                        {
                                            try
                                            {
                                                string str = currentObject.IPAddress;
                                                bool check = str.Contains("..");
                                                if (check) { return; }
                                                var ipAddress = IPAddress.Parse(currentObject.IPAddress).GetAddressBytes();
                                                long m_Address = ((ipAddress[3] << 24 | ipAddress[2] << 16 | ipAddress[1] << 8 | ipAddress[0]) & 0x0FFFFFFFF);
                                                int one = ipAddress[0];
                                            }
                                            catch (Exception)
                                            {
                                                MessageBox.Show("Please enter the correct IP Address format. Ex: 127.0.0.1");
                                                return;
                                            }

                                            //if (currentObject.Port.ToString() != "" & currentObject.Port != null)
                                            if (currentObject.Port > 0 && currentObject.Port < 65535)
                                            {
                                                bool isCorrectPort = Regex.IsMatch(currentObject.Port.ToString(), "^[0-9]+$");
                                                if (isCorrectPort == true)
                                                {
                                                    flag = true;
                                                    //this.Close();
                                                    this.Hide();
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Please enter the correct port number");
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show("Please enter the port number");
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Please enter the correct IP Address");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Enter required fields");
                                    }
                                }

                                else if (currentObject.DeviceType == DeviceType.ModbusSerial)
                                {
                                    flag = true;
                                    //this.Close();
                                    this.Hide();
                                }
                                
                            }
                            else
                            {
                                MessageBox.Show("Please enter different device name");
                            }
                            //}
                            //else { this.Close();flag = true; }
                        }
                        else
                        {
                            MessageBox.Show("Please enter valid Device Name.\nThe Device Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter the Name");
                    }
                }
                else
                {
                    if (currentObject.DeviceName != "" & currentObject.DeviceName != null)
                    {
                        bool validName = Util.CheckValidName(currentObject.DeviceName);
                        if (validName)
                        {
                            if (currentObject.IPAddress != "" & currentObject.IPAddress != null)
                            {
                                //Regular Expression for IP Address
                                bool isCorrectId = Regex.IsMatch(currentObject.IPAddress, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
                                if (isCorrectId == true)
                                {
                                    try
                                    {
                                        string str = currentObject.IPAddress;
                                        bool check = str.Contains("..");
                                        if (check) { return; }
                                        var ipAddress = IPAddress.Parse(currentObject.IPAddress).GetAddressBytes();
                                        long m_Address = ((ipAddress[3] << 24 | ipAddress[2] << 16 | ipAddress[1] << 8 | ipAddress[0]) & 0x0FFFFFFFF);
                                        int one = ipAddress[0];
                                    }
                                    catch (Exception)
                                    {
                                        MessageBox.Show("Please enter the correct IP Address format. Ex: 127.0.0.1");
                                        return;
                                    }

                                    if (currentObject.Port != "" & currentObject.Port != null)
                                    {
                                        bool isCorrectPort = Regex.IsMatch(currentObject.Port, "^[0-9]+$");
                                        if (isCorrectPort == true)
                                        {
                                            this.Hide();
                                        }
                                        else
                                        {
                                            MessageBox.Show("Please enter the correct port number");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Please enter the port number");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Please enter the correct IP Address");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Enter Device IP Address");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please enter valid Device Name.\nThe Device Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter the Name");
                    }
                }
            }
        }

        #endregion FinishBtn Click Event

        #region CancelBtn Click Event
        private void CancelBtn_click(object sender, RoutedEventArgs e)
        {
            Reset();
            this.Close();
            CancelBtn.Background = Brushes.Red;
        }
        #endregion CancelBtn Click Event

        #region Window_Closing Event

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (flag == false) { Reset(); }
            CancelBtn.Background = Brushes.Red;//Based on button background color we are Adding/Deleting/Editing the Connector/Device/Tag.
        }
        #endregion Window_Closing Event

        /// <summary>
        /// User checked the Radio Button Tag Group this event is raised, and change the property grid source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdbtnTagGroup_Checked(object sender, RoutedEventArgs e)
        {
            TagGroup tagGroup = new TagGroup();
            gridDeviceHeader.Visibility = Visibility.Visible;
            gridHeader.Visibility = Visibility.Hidden;
            cmboxDevice.ItemsSource = null;
            cmboxDevice.ItemsSource = SelectedProtocol.DeviceCollection;
            cmboxDevice.DisplayMemberPath = "DeviceName";
            DevicePropertyGrid.SelectedObject = tagGroup;
            this.Title = "Add New Tag Group";
        }

        private void rdbtnDevice_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedProtocol != null && DevicePropertyGrid != null)
            {
                SelectDeviceType(SelectedProtocol);
                //DevicePropertyGrid.SelectedObject = modbusEthernetDevice;
                gridDeviceHeader.Visibility = Visibility.Hidden;
                gridHeader.Visibility = Visibility.Visible;
                this.Title = "Add New Device";
            }
        }
    }

    #endregion End Of DevicesWindow
}
#endregion End Of Elpis OPC Server Namespace