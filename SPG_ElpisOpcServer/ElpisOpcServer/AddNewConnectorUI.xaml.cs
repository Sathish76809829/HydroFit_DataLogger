#region Namespaces
using System.Windows;
using OPCEngine;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using Elpis.Windows.OPC.Server;
using OPCEngine.Connectors.Allen_Bradley;

#endregion End Of Namespaces

#region Elpis OPC Server Namespace
namespace ElpisOpcServer
{
    #region AddNewConnectorUI Class
    /// <summary>
    /// Interaction logic for AddNewProtocolUI.xaml
    /// </summary>
    public partial class AddNewConnectorUI : Window
    {
        #region  Field
        public ModbusEthernetConnector ModbusEthernet { get; set; }
        public ModbusSerialConnector ModbusSerial { get; set; }
        public ABMicrologixEthernetConnector ABMicrologixE { get; set; }
        public ABControlLogicConnector ABCL { get; set; }
        
        public TcpSocketConnector TcpSocketConnector { get; set; }
        public IConnector obj { get; set; }
        public int SelectedProtocolIndex { get; set; }
        public bool flag { get; set; }
      
        
        #endregion End Of Field

        #region Constructor
        /// <summary>
        ///  Constructor
        /// </summary>
        public AddNewConnectorUI()
        {
            InitializeComponent();
            //ProtocolPropertyGrid.SelectedObject = modbusEthernet;
            //this.Title = "Add New Connector";
            CancelBtn.Background = Brushes.White;

            if (ConnectorTypeCmbBox.SelectedIndex == 3)
            {
                ProtocolPropertyGrid.SelectedObject = TcpSocketConnector;
                //SelectedProtocolIndex = ProtocolTypeCmbBox.SelectedIndex;
                ////SelectProtocolGrid.Visibility = Visibility.Hidden;
                ////PropertyGrid.Visibility = Visibility.Visible;


                //obj = ConnectorFactory.GetConnectorObj(ProtocolType.ModbusEthernet);
                //if (modbusEthernet == null)
                //{
                //    modbusEthernet = obj as ModbusEthernet;
                //}

                ////modbusEthernet.ConnectorName = "Protocol1";
                //modbusEthernet.ProtocolType = "ModBus TCP/IP Ethernet";
                //ProtocolPropertyGrid.SelectedObject = modbusEthernet;
            }

            else if (ConnectorTypeCmbBox.SelectedIndex == 1)
            {
                ////SelectedProtocolIndex = ProtocolTypeCmbBox.SelectedIndex;
                ////SelectProtocolGrid.Visibility = Visibility.Hidden;
                //PropertyGrid.Visibility = Visibility.Visible;

                //obj = ConnectorFactory.GetConnectorObj(ProtocolType.ABControlLogic);
                //ABCL = obj as ABControlLogic;


                //ABCL.ProtocolType = "AB-ControlLogixEthernet";
                this.Height = 500;
                ProtocolPropertyGrid.SelectedObject = ModbusSerial;

            }
            else if(ConnectorTypeCmbBox.SelectedIndex==2)
            {
                this.Height = 500;
                ProtocolPropertyGrid.SelectedObject = ModbusSerial;
            }
        }

        public AddNewConnectorUI(ConnectorBase selectedProtocol)
        {
            InitializeComponent();
            ProtocolPropertyGrid.SelectedObject = selectedProtocol;
            CancelBtn.Background = Brushes.White;          
            
        }

        #endregion End Of Constructor


        private void FinishBtn_click(object sender, RoutedEventArgs e)
        {
            dynamic currentObject = ProtocolPropertyGrid.SelectedObject as object;
            
            if (this.Title.Contains("Add"))
            {
                if (currentObject.ConnectorName != "" & currentObject.ConnectorName != null)
                {
                    bool validName = Util.CheckValidName(currentObject.ConnectorName);
                    if (validName)
                    {
                        bool isNewProtocol = ConfigurationSettingsUI.elpisServer.IsNewConnector(currentObject);
                        if (isNewProtocol == true)
                        {
                            flag = true;
                            this.Hide();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Please enter different Connector Name", "Error");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid Connector Name.\nThe connector name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
                else
                {
                    MessageBox.Show("Please enter the Connector Name");
                }
            }
            else
            {
                if (currentObject.ConnectorName != "" & currentObject.ConnectorName != null)
                {
                    bool validName =Util.CheckValidName(currentObject.ConnectorName);
                    if (!validName)
                    {
                        MessageBox.Show("Please enter valid Connector Name.\nThe connector name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        this.Hide();
                    }
                }
                else
                {
                    MessageBox.Show("Please enter the Connector Name");
                }
            }
        }

       

        public void BackBtn_click(object sender, RoutedEventArgs e)
        {
            //PropertyGrid.Visibility = Visibility.Hidden;
            //SelectProtocolGrid.Visibility = Visibility.Visible;
        }
        public void CancelBtn_click(object sender, RoutedEventArgs e)
        {
            reset();
            this.Close();
            CancelBtn.Background = Brushes.Red;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            if (flag == false) { reset(); }
            CancelBtn.Background=Brushes.Red;
        }

        public void reset()
        {
            ModbusEthernet = null;
            ABCL = null;
            ModbusSerial = null;
        }
        #region Nextbutton Click Event
        /// <summary>
        /// OKbutton Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        #endregion End Of Nextbutton Click Event

        #region Cancelbutton Click Event
        /// <summary>
        /// Cancelbutton Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancelbutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion End Of Cancelbutton Click Event

        private void ProtocolTypeCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int noOfProtocols = ConfigurationSettingsUI.elpisServer.ConnectorCollection == null ? 1 : ConfigurationSettingsUI.elpisServer.ConnectorCollection.Count + 1;
            if (ConnectorTypeCmbBox.SelectedIndex == 0)
            {
                SelectedProtocolIndex = ConnectorTypeCmbBox.SelectedIndex;
                obj = ConnectorFactory.GetConnector(ConnectorType.ModbusEthernet);
                ModbusEthernet = obj as ModbusEthernetConnector;
                //int noOfProtocols = ConfigurationSettingsUI.elpisServer.ProtocolCollection.Count + 1;
                //int noOfProtocols= ConfigurationSettingsUI.elpisServer.ProtocolCollection == null ? 1 : ConfigurationSettingsUI.elpisServer.ProtocolCollection.Count + 1;
                ModbusEthernet.ConnectorName = "Connector" + noOfProtocols;
                ModbusEthernet.TypeofConnector = ConnectorType.ModbusEthernet;
                if (ProtocolPropertyGrid != null)
                    ProtocolPropertyGrid.SelectedObject = ModbusEthernet;                
            }

            else if (ConnectorTypeCmbBox.SelectedIndex == 1)
            {                
                PropertyGrid.Visibility = Visibility.Visible;
                obj = ConnectorFactory.GetConnector(ConnectorType.ModbusSerial);
                ModbusSerial = obj as ModbusSerialConnector;                
                ModbusSerial.TypeofConnector =ConnectorType.ModbusSerial;                
                ModbusSerial.ConnectorName= "Connector" + noOfProtocols;
                if(ProtocolPropertyGrid !=null)
                    ProtocolPropertyGrid.SelectedObject = ModbusSerial;     
            }

            else if (ConnectorTypeCmbBox.SelectedIndex == 2)
            {
                PropertyGrid.Visibility = Visibility.Visible;
                obj = ConnectorFactory.GetConnector(ConnectorType.ABMicroLogixEthernet);
                ABMicrologixE = obj as ABMicrologixEthernetConnector;
                ABMicrologixE.TypeofConnector = ConnectorType.ABMicroLogixEthernet;
                ABMicrologixE.ConnectorName = "Connector" + noOfProtocols;
                if (ProtocolPropertyGrid != null)
                    ProtocolPropertyGrid.SelectedObject = ABMicrologixE;
            }
            else if(ConnectorTypeCmbBox.SelectedIndex==3)
            {
                // PropertyGrid.Visibility = Visibility.Visible;
                //SelectedProtocolIndex = ConnectorTypeCmbBox.SelectedIndex;
                obj = ConnectorFactory.GetConnector(ConnectorType.TcpSocket);
                TcpSocketConnector = obj as TcpSocketConnector;
                TcpSocketConnector.TypeofConnector = ConnectorType.TcpSocket;
                TcpSocketConnector.ConnectorName = "Ethernet" + noOfProtocols;
                if (ProtocolPropertyGrid != null)
                    ProtocolPropertyGrid.SelectedObject = TcpSocketConnector;
            }
           

            //else if (ConnectorTypeCmbBox.SelectedIndex == 2)
            //{
            //    PropertyGrid.Visibility = Visibility.Visible;
            //    obj = ConnectorFactory.GetConnector(ConnectorType.ABControlLogic);
            //    ABCL = obj as ABControlLogicConnector;
            //    ABCL.TypeofConnector = ConnectorType.ABControlLogic;
            //    ABCL.ConnectorName = "Connector" + noOfProtocols;
            //    if (ProtocolPropertyGrid != null)
            //        ProtocolPropertyGrid.SelectedObject = ABCL;
            //}

        }
    }
    #endregion End Of AddNewConnectorUI Class
}
#endregion End Of Elpis OPC Server Namespace