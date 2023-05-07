#region Namespaces
using Elpis.Windows.OPC.Server;
using Microsoft.VisualBasic;
using NDI.SLIKDA.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

#endregion Namespaces

#region Elpis OPC Server Namespace
namespace ElpisOpcServer
{

    #region ConfigurationPresentation Window Class for All Code behinds

    /// <summary>
    /// Interaction logic for ConfigurationPresentation.xaml
    /// </summary>
    [Serializable()]
    public partial class ConfigurationSettingsUI : UserControl, INotifyPropertyChanged
    {
        #region All Member variables and Properties

        #region Public Members

        //observable collections property changed function of event handler should specified by the keyword [field:NonSerialized()]
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        //only single time we are creating the object for this ElpisServer
        public static ElpisServer elpisServer { get; set; }

        public Tag tag { get; set; }
        ObservableCollection<LoggerViewModel> CurrrentLogsCollection { get; set; }

        public static bool isEdited { get; set; }

        AboutPageUI about { get; set; }
        #endregion Public Members


        #endregion End Of All Memeber variables and Properties

        #region Constructor
        /// <summary>
        /// Constructor Definition
        /// </summary>
        public ConfigurationSettingsUI()
        {
            InitializeComponent();
            //if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            //{
            //    return;
            //}


            elpisServer = new ElpisServer();

            about = new AboutPageUI();
            //04/09/17
            TaglListBox.Items.Clear();
            SortList();

        }
        #endregion End Of Constructor
        public void Clear(object sender, RoutedEventArgs e) { }

        #region All Memeber Functions And Events

        #region OnPropertyChanged Method
        /// <summary>
        /// OnPropertyChanged Method For All Properties 
        /// </summary>
        /// <param name="Property">Name Of the Property</param>
        public void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }
        }
        #endregion End Of OnPropertyChanged Method

        #region Window Loaded Event for SLIKDA Registration
        /// <summary>
        /// Window loaded event creating SLIKDA control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigurationSettingsUI_Loaded(object sender, RoutedEventArgs e)
        {
            #region Load 
            try
            {
                //if (!ConfigurationSettingsUI.elpisServer.runTimeDisplay.Contains("Stop"))
                //{
                //    ConfigurationSettingsUI.elpisServer.OpenLastLoadedProject();
                //}
                if (ConfigurationSettingsUI.elpisServer.ConnectorCollection != null)
                {
                    if (ConfigurationSettingsUI.elpisServer.ConnectorCollection.Count != 0)
                    {
                        ConnectorListBox.ItemsSource = ConfigurationSettingsUI.elpisServer.ConnectorCollection;
                        //ProtocolListBox.DisplayMemberPath = "ConnectorName";
                        NewDeviceMenuItem.IsEnabled = true;
                        NewTagMenuItem.IsEnabled = true;
                        gridPropertylbl.Visibility = Visibility.Visible;
                        ConnectorListBox.SelectedItem = elpisServer.ConnectorCollection[0];
                        PropertiesPropertyGrid.SelectedObject = ConnectorListBox.Items[0];
                        PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                        border.BorderBrush = Brushes.DarkOrange;
                        propertyDisplayLbl.Content = ConfigurationSettingsUI.elpisServer.ConnectorCollection[0].Name + "  Properties";
                        //propertyDisplayLbl.Background = Brushes.White;                   

                    }
                    else
                    {
                        gridPropertylbl.Visibility = Visibility.Hidden;
                        PropertiesPropertyGrid.SelectedObject = null;
                        PropertiesPropertyGrid.BorderBrush = Brushes.White;
                        border.BorderBrush = Brushes.White;
                        NewTagMenuItem.IsEnabled = false;
                        NewDeviceMenuItem.IsEnabled = false;
                    }

                }
                #endregion Load 

                #region Add logs

                ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Configuration", @"Elpis OPC Server/Runtime",
                    "Configuration session started by " + WindowsIdentity.GetCurrent().Name + " as Default User.", LogStatus.Information);
                LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;

                //ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Configuration", @"Elpis OPC Server\Runtime",
                //    "Configuration session started by " + WindowsIdentity.GetCurrent().Name + " as Default User.", LogStatus.Error);
                //ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Configuration", @"Elpis OPC Server\Runtime",
                //    "Configuration session started by " + WindowsIdentity.GetCurrent().Name + " as Default User.", LogStatus.Warning);
                //LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;
                //LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;
                if (ConfigurationSettingsUI.elpisServer.ConnectorCollection != null)
                {
                    foreach (var connector in ConfigurationSettingsUI.elpisServer.ConnectorCollection)
                    {
                        ConnectorBase currentConnector = connector as ConnectorBase;
                        ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Configuration", currentConnector.TypeofConnector.ToString(),
                        currentConnector.ConnectorName + " device driver loaded successfully.", LogStatus.Information);

                    }
                }

                #endregion Add logs
            }
            catch (Exception)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                ElpisServer.Addlogs("Configuration", @"ConfigurationSettings", "Problem in loading configuration settings", LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }
        #endregion End Of Window Loaded Event      


        #region All Window Events

        #region NewProtocolMenuItem Click Event
        /// <summary>
        /// NewProtocolMenuItem Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewProtocolMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isCreated;
                AddNewConnectorUI newConnectorWindow = new AddNewConnectorUI();
                newConnectorWindow.ShowDialog();
                if (newConnectorWindow.ConnectorTypeCmbBox.SelectedIndex == 0)
                {
                    if (newConnectorWindow.ModbusEthernet != null)
                    {
                        if (string.IsNullOrEmpty(newConnectorWindow.ModbusEthernet.ConnectorName))
                        {
                            MessageBox.Show("Connector name cannot be empty, Enter the Connector name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        for (int i = 0; i < ConnectorListBox.Items.Count; i++)
                        {
                            dynamic modbus = ConnectorListBox.Items[i] as dynamic;
                            if (modbus.ConnectorName.ToLower() == newConnectorWindow.ModbusEthernet.ConnectorName.ToLower())
                            {
                                MessageBox.Show("Connector with same name already exists, create connector with different name.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                        }
                        newConnectorWindow.obj.Name = newConnectorWindow.ModbusEthernet.ConnectorName;
                        isCreated = elpisServer.NewConnector(newConnectorWindow.obj, ConnectorType.ModbusEthernet);
                        if (isCreated)
                        {
                            //enable context menu for devices
                            NewDeviceMenuItem.IsEnabled = true;
                            ConnectorListBox.ItemsSource = elpisServer.ConnectorCollection;
                            // ConnectorListBox.DisplayMemberPath = "ConnectorName";
                            ConnectorListBox.SelectedItem = elpisServer.ConnectorCollection[elpisServer.ConnectorCollection.Count - 1];
                            //ListBoxItem item = ProtocolListBox.ItemContainerGenerator.ContainerFromIndex(ProtocolListBox.SelectedIndex) as ListBoxItem;
                            //item.Focus();
                            //this.ConnectorListBox.ScrollIntoView(ConnectorListBox.SelectedItem);
                            bool status = this.ConnectorListBox.Focus();
                            if (newConnectorWindow.ModbusEthernet.DeviceCollection != null)
                                DeviceListBox.ItemsSource = newConnectorWindow.ModbusEthernet.DeviceCollection;
                            else
                            {
                                newConnectorWindow.ModbusEthernet.DeviceCollection = new ObservableCollection<DeviceBase>();
                                DeviceListBox.ItemsSource = newConnectorWindow.ModbusEthernet.DeviceCollection;
                            }
                            //DeviceListBox.DisplayMemberPath = "DeviceName";
                            propertyDisplayLbl.Content = elpisServer.ConnectorCollection[elpisServer.ConnectorCollection.Count - 1].Name + "- Properties";

                            //28 02 2017 

                            elpisServer.LoadDataLogCollection("Configuration", newConnectorWindow.ModbusEthernet.TypeofConnector.ToString(),
                                "Connector: " + newConnectorWindow.ModbusEthernet.ConnectorName + " has created successfully.", LogStatus.Information);

                            LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;
                        }
                        else
                        {
                            var currentCollection = elpisServer.LoadDataLogCollection("Configuration", newConnectorWindow.ModbusEthernet.TypeofConnector.ToString(),
                               "Unable to create the Connector, check the list of Connectors.", LogStatus.Error);
                        }
                    }
                }
                else if (newConnectorWindow.ConnectorTypeCmbBox.SelectedIndex == 1)
                {
                    if (newConnectorWindow.ModbusSerial != null)
                    {
                        if (string.IsNullOrEmpty(newConnectorWindow.ModbusSerial.ConnectorName))
                        {
                            MessageBox.Show("Connector name cannot be empty, Enter the Connector name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        for (int i = 0; i < ConnectorListBox.Items.Count; i++)
                        {
                            dynamic modbusSerial = ConnectorListBox.Items[i] as dynamic;
                            if (modbusSerial.ConnectorName.ToLower() == newConnectorWindow.ModbusSerial.ConnectorName.ToLower())
                            {
                                MessageBox.Show("Connector with same name already exists, create connector with different name.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                        }
                        newConnectorWindow.obj.Name = newConnectorWindow.ModbusSerial.ConnectorName;
                        isCreated = elpisServer.NewConnector(newConnectorWindow.obj, ConnectorType.ModbusSerial);
                        if (isCreated)
                        {
                            //enable context menu for devices
                            NewDeviceMenuItem.IsEnabled = true;

                            ConnectorListBox.ItemsSource = elpisServer.ConnectorCollection;
                            // ConnectorListBox.DisplayMemberPath = "ConnectorName";

                            DeviceListBox.ItemsSource = newConnectorWindow.ModbusSerial.DeviceCollection;
                            //DeviceListBox.DisplayMemberPath = "DeviceName";

                            var currentCollection = elpisServer.LoadDataLogCollection("Configuration", newConnectorWindow.ModbusSerial.TypeofConnector.ToString(),
                                "Connector: " + newConnectorWindow.ModbusSerial.ConnectorName + " has created successfully.", LogStatus.Information);
                            LoggerUserControl.ListViewLogger.ItemsSource = currentCollection;

                        }
                        else
                        {
                            var currentCollection = elpisServer.LoadDataLogCollection("Configuration", newConnectorWindow.ModbusSerial.TypeofConnector.ToString(),
                               "Unable to create the Connector, check the list of Connectors.", LogStatus.Error);
                        }
                    }
                }
                LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;
            }
            catch (Exception)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                ElpisServer.Addlogs("Configuration", @"ElpisServer/Configuration", "Problem in creating Connector", LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }

        #endregion End Of NewProtocolMenuItem Click Event


        #region NewDeviceMenuItem Click Event
        /// <summary>
        /// NewDeviceMenuItem Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewDeviceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isCreated;
                var selectedProtocol = ConnectorListBox.SelectedItem as ConnectorBase;
                if (selectedProtocol != null)
                {
                    //isEdited = false;
                    AddNewDeviceUI newDeviceWindow = new AddNewDeviceUI(selectedProtocol);
                    //newDeviceWindow.modbusEthernetDevice.ProtocolAssignment = selectedProtocol.ProtocolName;
                    newDeviceWindow.ShowDialog();
                    if (newDeviceWindow.CancelBtn.Background != Brushes.Red)
                    {
                        #region Create Tag group
                        if (newDeviceWindow.rdbtnDevice.IsChecked == false)
                        {
                            var device = DeviceFactory.GetDeviceByName(((DeviceBase)newDeviceWindow.cmboxDevice.SelectedItem).DeviceName, selectedProtocol.DeviceCollection);
                            DeviceBase deviceObject = DeviceFactory.GetDevice(device);
                            if (device != null)
                            {
                                var group = newDeviceWindow.DevicePropertyGrid.SelectedObject as TagGroup;
                                //var item1 = newDeviceWindow.DevicePropertyGrid.SelectedObject;
                                group.DeviceName = deviceObject.DeviceName;
                                if (group.TagsCollection == null)
                                    group.TagsCollection = new ObservableCollection<Tag>();
                                if (deviceObject.GroupCollection == null)
                                    deviceObject.GroupCollection = new ObservableCollection<TagGroup>();
                                List<string> groupsNameCollection = deviceObject.GroupCollection.Select(n => n.GroupName.ToLower()).ToList();
                                if (groupsNameCollection.Contains(group.GroupName.ToLower()))
                                {
                                    MessageBox.Show("Tag Group with same name already exists, please create with another name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                                else
                                {
                                    deviceObject.GroupCollection.Add(group);
                                    DeviceListBox.ItemsSource = null;
                                    DeviceListBox.ItemsSource = selectedProtocol.DeviceCollection;
                                    TreeViewItem tvItem = DeviceListBox.ItemContainerGenerator.ContainerFromItem(deviceObject) as TreeViewItem;
                                    if (tvItem != null)
                                    {
                                        tvItem.ExpandSubtree();
                                        TreeViewItem curItem = tvItem.Items[(tvItem.Items.Count) - 1] as TreeViewItem;
                                        foreach (var item in tvItem.Items)
                                        {
                                            //TreeViewItem child = DeviceListBox.ItemContainerGenerator.ContainerFromItem(deviceObject) as TreeViewItem;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var device1 = DeviceListBox.Items[0] as DeviceBase;
                                var group1 = new TagGroup();
                                var item = newDeviceWindow.DevicePropertyGrid.SelectedObject;
                                group1.GroupName = ((TagGroup)item).GroupName;
                                group1.TagsCollection = new ObservableCollection<Tag>();
                                device1.GroupCollection.Add(group1);
                            }
                            DeviceListBox.ItemsSource = null;
                            DeviceListBox.ItemsSource = selectedProtocol.DeviceCollection;
                            TreeViewItem tvParent = DeviceListBox.ItemContainerGenerator.ContainerFromItem(deviceObject) as TreeViewItem;
                            if (tvParent != null)
                            {
                                tvParent.IsSelected = true;
                                tvParent.IsExpanded = true;


                                //    foreach (var item in tvParent.Items)
                                //{
                                //    TreeViewItem child = item as TreeViewItem;
                                //}
                                //TreeViewItem ch= tvParent.ItemContainerGenerator.ContainerFromIndex(tvParent.Items.Count - 1) as TreeViewItem;
                                //ch.IsSelected = true;
                            }
                        }
                        #endregion Create Tag group

                        #region Create Device
                        else
                        {
                            if (selectedProtocol.TypeofConnector == ConnectorType.ModbusEthernet)
                            {
                                if (newDeviceWindow.modbusEthernetDevice != null)
                                {
                                    if (string.IsNullOrEmpty(newDeviceWindow.modbusEthernetDevice.DeviceName))
                                    {
                                        MessageBox.Show("Device name cannot be empty, Enter the Device name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                    if (string.IsNullOrEmpty(newDeviceWindow.modbusEthernetDevice.IPAddress))
                                    {
                                        MessageBox.Show("IP Address can not be Null, Enter the IP Address", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                    if (string.IsNullOrEmpty(newDeviceWindow.modbusEthernetDevice.Port.ToString()))
                                    {
                                        MessageBox.Show("Port Number can not be null, Enter the Port Number", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                    //selectedProtocol.DeviceCollection.Count;//change this one into 
                                    for (int i = 0; i < DeviceListBox.Items.Count; i++)
                                    {
                                        DeviceBase device = DeviceListBox.Items[i] as DeviceBase;
                                        if (device.DeviceName.ToLower() == newDeviceWindow.modbusEthernetDevice.DeviceName)
                                        {
                                            MessageBox.Show("Device with the same name is already exists.\nCreate with another name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        }
                                    }
                                    try
                                    {
                                        newDeviceWindow.obj.Name = newDeviceWindow.modbusEthernetDevice.DeviceName;
                                        isCreated = elpisServer.NewDevice(selectedProtocol, ConnectorType.ModbusEthernet, newDeviceWindow.obj);
                                        if (isCreated)
                                        {
                                            //enables the context menu for tags
                                            NewTagMenuItem.IsEnabled = true;
                                            DeviceListBox.ItemsSource = selectedProtocol.DeviceCollection;
                                            //DeviceListBox.DisplayMemberPath = "DeviceName";
                                            //DeviceListBox.SelectedItem = selectedProtocol.DeviceCollection[selectedProtocol.DeviceCollection.Count - 1];
                                            var tvItem = DeviceListBox.ItemContainerGenerator.ContainerFromItem(newDeviceWindow.obj) as TreeViewItem;
                                            if (tvItem != null)
                                            {
                                                tvItem.IsSelected = true;

                                            }

                                            elpisServer.LoadDataLogCollection("Configuration", newDeviceWindow.modbusEthernetDevice.DeviceType.ToString(),
                                                "Device: " + newDeviceWindow.modbusEthernetDevice.DeviceName +
                                                " has created successfully.", LogStatus.Information);
                                            LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;

                                        }
                                        else
                                        {
                                            elpisServer.LoadDataLogCollection("Configuration", newDeviceWindow.modbusEthernetDevice.DeviceType.ToString(),
                                                                               "Unable to create the device, check the list of devices.", LogStatus.Error);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string errMsg = ex.Message;
                                    }
                                }
                            }

                            else if (selectedProtocol.TypeofConnector == ConnectorType.ModbusSerial)
                            {
                                // newDeviceWindow.ShowDialog();

                                if (newDeviceWindow.modbusSerialDevice != null)
                                {
                                    if (string.IsNullOrEmpty(newDeviceWindow.modbusSerialDevice.DeviceName))
                                    {
                                        MessageBox.Show("Device name cannot be empty, Enter the Device name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                    try
                                    {
                                        newDeviceWindow.obj.Name = newDeviceWindow.modbusSerialDevice.DeviceName;
                                        isCreated = elpisServer.NewDevice(selectedProtocol, ConnectorType.ModbusSerial, newDeviceWindow.obj);
                                        if (isCreated)
                                        {
                                            //enables the context menu for tags
                                            NewTagMenuItem.IsEnabled = true;
                                            DeviceListBox.ItemsSource = selectedProtocol.DeviceCollection;
                                            //DeviceListBox.DisplayMemberPath = "DeviceName";
                                            //DeviceListBox.SelectedItem = selectedProtocol.DeviceCollection[selectedProtocol.DeviceCollection.Count - 1];
                                            var tvItem = DeviceListBox.ItemContainerGenerator.ContainerFromItem(newDeviceWindow.obj) as TreeViewItem;
                                            if (tvItem != null)
                                            {
                                                tvItem.IsSelected = true;

                                            }

                                            var currentCollection = elpisServer.LoadDataLogCollection("Configuration", newDeviceWindow.modbusSerialDevice.DeviceType.ToString(),
                                                "Device: " + newDeviceWindow.modbusSerialDevice.DeviceName + " has created successfully.", LogStatus.Information);
                                            LoggerUserControl.ListViewLogger.ItemsSource = currentCollection;
                                        }
                                        else
                                        {
                                            elpisServer.LoadDataLogCollection("Configuration", newDeviceWindow.modbusSerialDevice.DeviceType.ToString(),
                                                                               "Unable to create the device, check the list of devices.", LogStatus.Error);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string errMsg = ex.Message;
                                    }
                                }

                            }
                        }

                        #endregion Create Device
                    }
                }
                else
                {
                    MessageBox.Show("Select the appropriate Connector first!!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;
            }
            catch (Exception)
            {

            }
        }

        #endregion End Of NewDeviceMenuItem Click Event

        public ObservableCollection<LoggerViewModel> LoadLogs()
        {

            return CurrrentLogsCollection;
        }
        #region NewTagMenuItem Click Event
        /// <summary>
        /// NewTagMenuItem Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //for Tags menuitem  
        private void NewTagMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isCreated;
                var selectedDevice = DeviceListBox.SelectedItem as DeviceBase; ;
                if (DeviceListBox.SelectedItem != null && DeviceListBox.SelectedItem.GetType().Name == "TagGroup")
                {
                    DeviceBase deviceObj = DeviceFactory.GetDeviceByName(((TagGroup)DeviceListBox.SelectedItem).DeviceName, ((ConnectorBase)ConnectorListBox.SelectedItem).DeviceCollection);
                    dynamic deviceObject = DeviceFactory.GetDevice(deviceObj);
                    selectedDevice = deviceObject;
                }
                else
                {
                    selectedDevice = DeviceListBox.SelectedItem as DeviceBase;

                }


                if (selectedDevice != null)
                {
                    #region testing 
                    ObservableCollection<TagGroup> Groups = new ObservableCollection<TagGroup>();
                    if (selectedDevice.GroupCollection == null)
                        selectedDevice.GroupCollection = new ObservableCollection<TagGroup>();
                    Groups = selectedDevice.GroupCollection;
                    Elpis.Windows.OPC.Server.Tag.TagGroupItemsSource.GroupsTags = Groups;
                    AddNewTagUI newCreateTagWindow = new AddNewTagUI(selectedDevice, Groups);
                    #endregion
                    // AddNewTagUI newCreateTagWindow = new AddNewTagUI(selectedDevice);
                    newCreateTagWindow.ShowDialog();
                    if (newCreateTagWindow.tag != null)
                    {

                        if (string.IsNullOrEmpty(newCreateTagWindow.tag.TagName))
                        {
                            MessageBox.Show("Tag name cannot be empty", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (string.IsNullOrEmpty(newCreateTagWindow.tag.Address))
                        {
                            MessageBox.Show("Address cannot be empty", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (newCreateTagWindow.tag.TagName != null)
                        {
                            if (newCreateTagWindow.tag.SelectedGroup == null || newCreateTagWindow.tag.SelectedGroup == "None")
                            {
                                for (int i = 0; i < TaglListBox.Items.Count; i++)
                                {
                                    //if (((System.Windows.FrameworkElement)(TaglListBox.Items[i])).Tag != null)
                                    {
                                        Tag listTag = TaglListBox.Items[i] as Tag;
                                        if (listTag.TagName.ToLower() == newCreateTagWindow.tag.TagName.ToLower())
                                        {
                                            MessageBox.Show("Tag with same name already exists in the same device.\nUse another name to create a tag", "OPC Elpis Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        }
                                    }

                                }
                            }
                            else
                            {
                                //for add Tag to the Group. Cheeck the name of the Group is Exists or not
                                var tagList = Groups.Where(g => g.GroupName == newCreateTagWindow.tag.SelectedGroup).Select(g => g.TagsCollection); //from g in Groups where g.GroupName.Contains(newCreateTagWindow.tag.SelectedGroup) select g.TagsCollection;
                                foreach (var list in tagList)
                                {
                                    foreach (var item in list)
                                    {
                                        Tag listTag = item as Tag;

                                        if (listTag.TagName.ToLower() == newCreateTagWindow.tag.TagName.ToLower())
                                        {
                                            MessageBox.Show("Tag with same name already exists in the same Tag Group.\nUse another name to create a tag", "OPC Elpis Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        }
                                    }

                                }
                            }

                            //newCreateTagWindow.obj.Name = newCreateTagWindow.tag.TagName;
                            //MainWindow.opcEngineMainWindowViewModel.NewTag(newCreateTagWindow.tag, deviceViewModel);
                            isCreated = elpisServer.NewTag(selectedDevice, newCreateTagWindow.tag);
                            if (isCreated)
                            {
                                if (selectedDevice.TagsCollection != null)
                                {
                                    if (selectedDevice.TagsCollection.Count > 0)
                                    {
                                        TaglListBox.ClearValue(ItemsControl.ItemsSourceProperty);
                                        TaglListBox.Items.Clear();
                                        TaglListBox.ItemsSource = selectedDevice.TagsCollection;
                                        //TaglListBox.DisplayMemberPath = "TagName";
                                        TaglListBox.SelectedItem = null;
                                        TaglListBox.SelectedItem = selectedDevice.TagsCollection[selectedDevice.TagsCollection.Count - 1];
                                    }
                                }
                                elpisServer.LoadDataLogCollection("Configuration", selectedDevice.DeviceType.ToString(), "Tag: " +
                                     newCreateTagWindow.tag.TagName + " has created successfully.", LogStatus.Information);
                                LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;
                                                                                                            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(currentCollection);
                                                                                                            //view.SortDescriptions.Add(new SortDescription("Age", ListSortDirection.Ascending));
                            }
                            else
                            {
                                elpisServer.LoadDataLogCollection("Configuration", selectedDevice.DeviceName,
                                                                   "Unable to create the tag, check the list of tags.", LogStatus.Error);
                            }
                        }
                        else
                            MessageBox.Show("Tag name cannot be empty", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                }
                else
                {
                    MessageBox.Show("Select the appropriate device and Connector first!!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                ElpisServer.Addlogs("Configuration", @"Elpis/Configuration/TagCreation", "Problem in creating tag", LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }
        #endregion End Of NewTagMenuItem Click Event

        #region ProtocolListBox SelectionChanged Event
        /// <summary>
        /// ProtocolListBox SelectionChanged Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProtocolListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceListBox.ItemsSource = null;
            var selectedProtocol = ConnectorListBox.SelectedItem as ConnectorBase;
            if (selectedProtocol != null)
            {
                if (selectedProtocol.DeviceCollection != null)
                {
                    DeviceListBox.ItemsSource = selectedProtocol.DeviceCollection;
                    //DeviceListBox.SelectedItem = null;
                }
                propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedProtocol.ConnectorName);
                //propertyDisplayLbl.Background = Brushes.Magenta;
                gridPropertylbl.Visibility = Visibility.Visible;
                border.BorderBrush = Brushes.DarkOrange;
                PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                PropertiesPropertyGrid.BorderThickness = new Thickness(1, 0, 1, 1);

            }

            PropertiesPropertyGrid.SelectedObject = selectedProtocol;
            PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
            border.BorderBrush = Brushes.DarkOrange;
        }
        #endregion End Of ProtocolListBox SelectionChanged Event

        #region ProtocolListBox GotFocus Event
        /// <summary>
        /// ProtocolListBox GotFocus Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProtocolListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ConnectorListBox.SelectedItem != null)
            {
                DeviceListBox.ItemsSource = null;
                var selectedProtocol = ConnectorListBox.SelectedItem as ConnectorBase;
                if (selectedProtocol != null)
                {
                    if (selectedProtocol.DeviceCollection != null)
                        DeviceListBox.ItemsSource = selectedProtocol.DeviceCollection;
                    TaglListBox.ItemsSource = null;
                }
                expProperty.BorderBrush = Brushes.Transparent;
                gridPropertylbl.Visibility = Visibility.Visible;
                PropertiesPropertyGrid.SelectedObject = selectedProtocol;
                propertyDisplayLbl.Content = ConfigurationSettingsUI.elpisServer.ConnectorCollection[0].Name + "- Properties";
                PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                border.BorderBrush = Brushes.DarkOrange;
            }
        }
        #endregion End Of ProtocolListBox GotFocus Event

        #region DeviceListBox SelectionChanged Event
        /// <summary>
        /// DeviceListBox SelectionChanged Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TaglListBox.ItemsSource = null;

            var selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
            if (selectedDevice != null)
            {
                propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedDevice.DeviceName);
                //propertyDisplayLbl.Background = Brushes.LightGreen;
                gridPropertylbl.Visibility = Visibility.Visible;

                PropertiesPropertyGrid.SelectedObject = selectedDevice;
                PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                border.BorderBrush = Brushes.DarkBlue;
                PropertiesPropertyGrid.BorderThickness = new Thickness(1);
                if (selectedDevice.TagsCollection != null)
                {
                    TaglListBox.Items.Clear();
                    TaglListBox.ItemsSource = selectedDevice.TagsCollection;
                }
            }
        }

        #endregion End Of DeviceListBox SelectionChanged Event

        #region DeviceListBox_GotFocus Event
        /// <summary>
        /// DeviceListBox_GotFocus Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceListBox_GotFocus(object sender, RoutedEventArgs e)
        {

            if (DeviceListBox.SelectedItem != null)
            {
                if (DeviceListBox.SelectedItem.ToString() == "Elpis.Windows.OPC.Server.TagGroup")
                {
                    var selectedGroup = DeviceListBox.SelectedItem as TagGroup;
                    if (selectedGroup != null)
                    {
                        if (selectedGroup.TagsCollection != null)
                        {
                            TaglListBox.ItemsSource = selectedGroup.TagsCollection;
                        }
                        else
                            TaglListBox.ItemsSource = null;
                        gridPropertylbl.Visibility = Visibility.Visible;
                        PropertiesPropertyGrid.SelectedObject = selectedGroup;
                        expProperty.BorderBrush = Brushes.Transparent;
                        propertyDisplayLbl.Content = selectedGroup.GroupName + "- Properties";
                        PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                        border.BorderBrush = Brushes.DarkBlue;

                    }
                }
                else
                {
                    var selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
                    if (selectedDevice != null)
                    {
                        if (selectedDevice.TagsCollection != null)
                            TaglListBox.ItemsSource = selectedDevice.TagsCollection;
                        else
                            TaglListBox.ItemsSource = null;
                        gridPropertylbl.Visibility = Visibility.Visible;
                        PropertiesPropertyGrid.SelectedObject = selectedDevice;
                        propertyDisplayLbl.Content = selectedDevice.DeviceName + "- Properties";
                        PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                        expProperty.BorderBrush = Brushes.Transparent;
                        border.BorderBrush = Brushes.DarkBlue;

                    }
                }
            }
            else
            {
                //DeviceListBox.ContextMenu = DeviceListBox.Resources["outContext"] as ContextMenu;
            }
        }

        
        #endregion End Of DeviceListBox_GotFocus Event

        #region TaglListBox GotFocus Event
        /// <summary>
        /// TaglListBox GotFocus Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaglListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            tag = TaglListBox.SelectedItem as Tag;
            if (tag != null)
            {
                gridPropertylbl.Visibility = Visibility.Visible;
                PropertiesPropertyGrid.SelectedObject = tag;
                expProperty.BorderBrush = Brushes.Transparent;
                propertyDisplayLbl.Content = tag.TagName + "- Properties";
                PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                border.BorderBrush = Brushes.Magenta;
            }

        }
        #endregion End Of TaglListBox GotFocuss Event

        #region TaglListBox SelectionChanged Event
        /// <summary>
        /// TaglListBox SelectionChanged Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaglListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tag = TaglListBox.SelectedItem as Tag;
            if (tag != null)
            {
                propertyDisplayLbl.Content = string.Format("{0}- Properties", tag.TagName);
                //propertyDisplayLbl.Background = Brushes.LightSkyBlue;
                gridPropertylbl.Visibility = Visibility.Visible;
                PropertiesPropertyGrid.SelectedObject = tag;
                PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                border.BorderBrush = Brushes.Magenta;
            }

            //PropertiesPropertyGrid.SelectedObject = tag;

        }
        #endregion End Of TaglListBox SelectionChanged Event

        #region txtBlockControlForTxtBox MouseLeftButtonDown Event
        /// <summary>
        /// txtBlockControlForTxtBox MouseLeftButtonDown Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBlockControlForTxtBox_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            //PropertyInfoViewModel propInfo = PropertyView.SelectedItem as PropertyInfoViewModel;
            //if (propInfo != null)
            //{
            //    if (e.ClickCount == 2)
            //    {


            //        if (propInfo.VisibilityType == ControlType.TextBlock)
            //        {
            //            propInfo.TextBlockVisibility = Visibility.Hidden;
            //            propInfo.TextBoxVisibility = Visibility.Visible;

            //        }

            //        else if (propInfo.VisibilityType == ControlType.ComboBox)
            //        {
            //            propInfo.TextBlockVisibility = Visibility.Hidden;
            //            propInfo.TextBoxVisibility = Visibility.Hidden;
            //            propInfo.ComboBoxVisibility = Visibility.Visible;


            //        }
            //    }
            //    if (propInfo.VisibilityType == ControlType.ComboBox)
            //    {
            //        propInfo.TextBlockVisibility = Visibility.Hidden;
            //        propInfo.TextBoxVisibility = Visibility.Hidden;
            //        propInfo.ComboBoxVisibility = Visibility.Visible;
            //    }
            //}
        }
        #endregion End Of txtBlockControlForTxtBox MouseLeftButtonDown Event



        #region TaglListBox LostFocus Event
        /// <summary>
        /// TaglListBox LostFocus Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaglListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Tag oldTag = TaglListBox.SelectedItem as Tag;
            //if (oldTag != null)
            //{
            //    oldTag.TagName = oldTag.PropertyInfoViewModelCollectionTags[0].PropertyValue.ToString();
            //    oldTag.Description = oldTag.PropertyInfoViewModelCollectionTags[1].PropertyValue.ToString();
            //    oldTag.Address = oldTag.PropertyInfoViewModelCollectionTags[2].PropertyValue.ToString();
            //    //oldTag.DataType = int.Parse(oldTag.PropertyInfoViewModelCollectionTags[3].PropertyValue);
            //    //oldTag.DataAccess = int.Parse(oldTag.PropertyInfoViewModelCollectionTags[3].PropertyValue);

            //    oldTag.ScanRate = oldTag.PropertyInfoViewModelCollectionTags[5].PropertyValue.ToString();
            //    //oldTag.DataType = DataAndAccessType(oldTag.PropertyInfoViewModelCollection[3].DataAndAccessTypeCollection[0]);
            //}
        }
        #endregion End Of TaglListBox LostFocus Event

        #region txtBoxControl_PreviewTextInput Event
        /// <summary> 
        /// txtBoxControl PreviewTextInput Event For Allowing only integers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxControl_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            //PropertyInfoViewModel propinfo = PropertyView.SelectedItem as PropertyInfoViewModel;
            //if (propinfo.PropertyName == "IP Address:" || propinfo.PropertyName == "Port:" || propinfo.PropertyName == "Address:" || propinfo.PropertyName == "Scan Rate:")
            //{
            //    e.Handled = IsNumericText(e.Text);
            //}
        }
        #endregion End Of txtBoxControl_PreviewTextInput Event

        #region IsNumericText Method
        /// <summary>
        /// IsNumericText Method for Regular Expression
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool IsNumericText(string str)
        {
            //otherwise we can use this pattern : @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$"
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }
        #endregion End Of IsNumericText Method

        #region Ribbon events

        private void ribbonNewBtn_Click(object sender, RoutedEventArgs e)
        {
            //ContextMenu cm = new ContextMenu();
            if (ConnectorListBox.ItemsSource == null) { return; }

            System.Windows.Forms.DialogResult dr = (System.Windows.Forms.DialogResult)MessageBox.Show(" Save changes to Untitled *?", "Elpis OPC Server", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (dr.ToString() == "Cancel")
            {
                return;
            }
            else if (dr.ToString() == "No")
            {
                ResetOldUI();
                ConnectorListBox.ContextMenu.IsEnabled = true;
                propertyDisplayLbl.Content = "";
                try
                {

                    ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Configuration", @"Elpis OPC Server\Configuration",
                      "Created backup of project : " + elpisServer.CurrentProjectFilePath, LogStatus.Information);
                    LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;//elpisServer.LoggerCollection;


                    ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Configuration", @"Elpis OPC Server\Configuration",
                       "Runtime Project has been reset.", LogStatus.Information);
                    LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;
                    return;
                }
                catch (Exception)
                {
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    ElpisServer.Addlogs("Configuration", @"Elpis/Configuration", "Problem in creating new Configuration.", LogStatus.Error);
                    //}), DispatcherPriority.Normal, null);
                }

            }
            else
            {
                elpisServer.SaveAs();
                ResetOldUI();
                propertyDisplayLbl.Content = string.Empty;
                //DeviceListBox.ContextMenu.IsEnabled = false;
                //TaglListBox.ContextMenu.IsEnabled = false;
            }
        }

        public void ResetOldUI()
        {
            ConnectorListBox.ItemsSource = null;
            elpisServer.ConnectorCollection = null;
            DeviceListBox.ItemsSource = null;
            ///elpisServer.
            TaglListBox.ItemsSource = null;

            elpisServer.TagDictionary.Clear();
            elpisServer.ConnectionHelperObj.tcpClientDictionary.Clear();
            elpisServer.OpcTags.Clear();
            //10-10-2017
            elpisServer.ScanrateGroup.Clear();
            elpisServer.ScanrateClientGroup.Clear();



        }

        private void SaveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            elpisServer.SaveAs();
        }

        private void ribbonSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            elpisServer.SaveLastLoadedProject();
        }

        public void ribbonOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<IConnector> ServerElements = elpisServer.OpenUserProjectFile();
                if (ServerElements != null)
                {
                    ResetOldUI();
                    elpisServer.TagCount = 0;
                    elpisServer.ConnectorCollection = ServerElements;  // elpisServer.OpenUserProjectFile();
                    if (elpisServer.ConnectorCollection != null)
                    {
                        if (elpisServer.ConnectorCollection.Count > 0)
                        {
                            NewDeviceMenuItem.IsEnabled = true;
                            NewTagMenuItem.IsEnabled = true;

                            ConnectorListBox.ItemsSource = elpisServer.ConnectorCollection;
                            ConnectorListBox.DisplayMemberPath = "ConnectorName";
                            ConnectorListBox.SelectedItem = 0;


                            ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Configuration", @"Elpis OPC Server\Configuration",
                       "Runtime Project replaced.| New Project= " + elpisServer.OpenedProjectFilePath, LogStatus.Information);
                            LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;


                        }
                        else
                        {
                            NewDeviceMenuItem.IsEnabled = false;
                            NewTagMenuItem.IsEnabled = false;
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                ElpisServer.Addlogs("Configuration", @"Elpis/Configuration", "Problem in opening the configuration file.", LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }

        #endregion Ribbon events

        private void backbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            //((MainWindow)System.Windows.Application.Current.MainWindow).Width = 800;
            //MainWindow.opcEngineMainWindowViewModel.LastLoadedProject();
            elpisServer.SaveLastLoadedProject();
        }

        private void exitbtn_Click(object sender, RoutedEventArgs e)
        {
            elpisServer.SaveLastLoadedProject();
            Window parent = Window.GetWindow(this);
            parent.Close();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            // Find the window that contains the control
            Window window = Window.GetWindow(this);

            // Minimize
            window.WindowState = WindowState.Minimized;
        }



        private void ImportFromCSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.opcEngineMainWindowViewModel.ImportFromCsv();

        }

        #endregion End Of All Window Events

        #endregion End Of All Memeber Functions And Events

        #region Delete Functions
        /// <summary>
        /// Delete the selected connector from the connector list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteConnectorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedConnector = ConnectorListBox.SelectedItem as ConnectorBase;
                if (selectedConnector != null)
                {
                    ConnectorListBox.SelectedItem = null;
                    elpisServer.DeleteConnector(selectedConnector);
                    propertyDisplayLbl.Content = string.Empty;
                    //propertyDisplayLbl.Background = Brushes.White;              
                    gridPropertylbl.Visibility = Visibility.Hidden;
                    PropertiesPropertyGrid.BorderBrush = Brushes.White;
                    border.BorderBrush = Brushes.White;
                    propertyDisplayLbl.Content = "";

                }
                else
                {
                    MessageBox.Show("Not selected any Connector.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (Exception)
            {
                elpisServer.LoadDataLogCollection("Configuration", @"Elpis/Configuration", "Problem in deleting Connector", LogStatus.Error);
            }
        }

        /// <summary>
        /// Delete selected Device form the device list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteDeviceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceListBox.SelectedItem == null)
                {
                    MessageBox.Show("Not selected any Device or Tag Group", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                else if (DeviceListBox.SelectedItem.ToString().Contains("TagGroup"))
                {
                    try
                    {
                        var selectedGroup = DeviceListBox.SelectedItem as TagGroup;
                        if (selectedGroup != null)
                        {
                            var selectedProtocol = ConnectorListBox.SelectedItem as ConnectorBase;
                            elpisServer.DeleteTagGroup(selectedGroup, selectedProtocol.DeviceCollection);
                            propertyDisplayLbl.Content = string.Empty;
                            //propertyDisplayLbl.Background = Brushes.White;
                            gridPropertylbl.Visibility = Visibility.Hidden;
                            PropertiesPropertyGrid.SelectedObject = null;

                            PropertiesPropertyGrid.BorderBrush = Brushes.White;
                            border.BorderBrush = Brushes.White;
                        }
                        else
                        {
                            MessageBox.Show("Not selected any Device or Tag Group", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                    }
                    catch (Exception)
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        ElpisServer.Addlogs("Configuration", @"Elpis/Configuration", "Problem in delete tag group.", LogStatus.Error);
                        //}), DispatcherPriority.Normal, null);
                    }
                }
                else
                {
                    try
                    {
                        ConnectorBase connector = ConnectorListBox.SelectedItem as ConnectorBase;
                        var selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
                        if (selectedDevice != null)
                        {
                            elpisServer.DeleteDevice(selectedDevice);
                            propertyDisplayLbl.Content = string.Empty;
                            //propertyDisplayLbl.Background = Brushes.White;
                            gridPropertylbl.Visibility = Visibility.Hidden;
                            DeviceListBox.ItemsSource = connector.DeviceCollection;
                            PropertiesPropertyGrid.SelectedObject = null;

                            PropertiesPropertyGrid.BorderBrush = Brushes.White;
                            border.BorderBrush = Brushes.White;
                        }
                    }
                    catch (Exception)
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        ElpisServer.Addlogs("Configuration", @"ElpisServer/Configuration/Delete Device", "Problem in deleting device", LogStatus.Error);
                        //}), DispatcherPriority.Normal, null);
                    }

                }
            }
            catch (Exception)
            {
                ElpisServer.Addlogs("Configuration", @"ElpisServer/Configuration/Delete Device", "Problem in deleting device", LogStatus.Error);
            }

        }

        /// <summary>
        ///Delete selected tag from the tag list. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteTagMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tag = TaglListBox.SelectedItem as Tag;
                if (tag != null)
                {
                    elpisServer.DeleteTag(tag);
                    propertyDisplayLbl.Content = string.Empty;
                    //propertyDisplayLbl.Background = Brushes.White;                    
                    PropertiesPropertyGrid.BorderBrush = Brushes.White;
                    gridPropertylbl.Visibility = Visibility.Hidden;
                    PropertiesPropertyGrid.SelectedObject = null;
                    PropertiesPropertyGrid.BorderBrush = Brushes.White;
                    border.BorderBrush = Brushes.White;
                }
                else
                {
                    MessageBox.Show("Not selected any Tag.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (Exception ex)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                ElpisServer.Addlogs("Configuration", @"ElpisServer/Configuration/DeleteTag", ex.Message, LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }
        #endregion Delete Functions

        private void ConnectorPropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //check server is running or not.
            PropertiesPropertyGrid.IsReadOnly = false;
            var selectedConnector = ConnectorListBox.SelectedItem as ConnectorBase;
            int index = ConnectorListBox.SelectedIndex;
            if (selectedConnector != null)
            {
                try
                {
                    string old_ConnectorName = selectedConnector.ConnectorName;
                    ConnectorType old_Connectortype = selectedConnector.TypeofConnector;
                    expProperty.BorderBrush = Brushes.Black;
                    propertyDisplayLbl.Content = "Property Editor- " + selectedConnector.ConnectorName;
                    EditElementinPropertyGrid(selectedConnector);

                    #region Open New window for editing the connector properties.
                    //if (false)
                    //{
                    //    AddNewProtocolUI newProtocolWindow = new AddNewProtocolUI(selectedProtocol);
                    //    newProtocolWindow.SelectProtocolGrid.Visibility = Visibility.Hidden;
                    //    //newProtocolWindow.ProtocolPropertyGrid.SelectedObject = selectedProtocol;
                    //    newProtocolWindow.Title = "Property Editor- " + selectedProtocol.ConnectorName;
                    //    //propertyDisplayLbl.Content = "Property Editor- " + selectedProtocol.ProtocolName;
                    //    PropertiesPropertyGrid.SelectedObject = null;
                    //    propertyDisplayLbl.Content = "";
                    //    gridPropertylbl.Visibility = Visibility.Hidden;
                    //    // propertyDisplayLbl.Background = Brushes.White;
                    //    PropertiesPropertyGrid.BorderBrush = Brushes.White;
                    //    border.BorderBrush = Brushes.White;
                    //    newProtocolWindow.ShowDialog();

                    //    // IProtocol currentProtocol = ProtocolListBox.SelectedItem as IProtocol;  //selectedProtocol as IProtocol;
                    //    if (newProtocolWindow.CancelBtn.Background == Brushes.Red)
                    //    {
                    //        selectedProtocol.ConnectorName = old_ConnectorName;
                    //        selectedProtocol.ProtocolType = old_Connectortype;
                    //        //Set the PropertyGrid Item
                    //        gridPropertylbl.Visibility = Visibility.Visible;
                    //        PropertiesPropertyGrid.SelectedObject = selectedProtocol;
                    //        PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                    //        border.BorderBrush = Brushes.DarkOrange;
                    //        propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedProtocol.ConnectorName);
                    //        // propertyDisplayLbl.Background = Brushes.Magenta;
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        //PropertiesPropertyGrid.SelectedObject = selectedProtocol; ;
                    //        //propertyDisplayLbl.Content = "Property Editor- " + selectedProtocol.ProtocolName;
                    //        //propertyDisplayLbl.Background = Brushes.Magenta;
                    //        for (int i = 0; i < ProtocolListBox.Items.Count; i++)
                    //        {
                    //            if (i != index)
                    //            {
                    //                ConnectorBase connector = ProtocolListBox.Items[i] as ConnectorBase;
                    //                if (connector.ConnectorName.ToLower() == selectedProtocol.ConnectorName.ToLower())
                    //                {
                    //                    selectedProtocol.ConnectorName = old_ConnectorName;
                    //                    gridPropertylbl.Visibility = Visibility.Visible;
                    //                    PropertiesPropertyGrid.SelectedObject = selectedProtocol;
                    //                    PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                    //                    border.BorderBrush = Brushes.DarkOrange;
                    //                    propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedProtocol.ConnectorName);
                    //                    //propertyDisplayLbl.Background = Brushes.Magenta;
                    //                    MessageBox.Show("Connector with same name already exists, create connector with different name.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //                    // ProtocolListBox.SelectedIndex = index;
                    //                    return;
                    //                }
                    //                else
                    //                {

                    //                }
                    //            }

                    //        }
                    //        isEdited = elpisServer.EditProtocol(selectedProtocol);
                    //        if (isEdited)
                    //        {
                    //            gridPropertylbl.Visibility = Visibility.Visible;
                    //            PropertiesPropertyGrid.SelectedObject = selectedProtocol;
                    //            PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                    //            border.BorderBrush = Brushes.DarkOrange;
                    //            propertyDisplayLbl.Content = string.Format("{0} - Properties", selectedProtocol.ConnectorName);
                    //            //propertyDisplayLbl.Background = Brushes.Magenta;
                    //            Dictionary<string, TcpClient> DummyTcpClientList = new Dictionary<string, TcpClient>();
                    //            // DummyTcpClientList= elpisServer.connectionHelper.tcpClientDictionary;
                    //            foreach (var item in elpisServer.connectionHelper.tcpClientDictionary)
                    //            {
                    //                DummyTcpClientList.Add(item.Key, item.Value);
                    //            }

                    //            foreach (var item in DummyTcpClientList)
                    //            {
                    //                ChangekeyValueinList(item, old_ConnectorName + ".", selectedProtocol.ConnectorName);

                    //            }
                    //            Dictionary<ISLIKTag, int> DummyTagList = new Dictionary<ISLIKTag, int>();
                    //            foreach (var item in elpisServer.TagDictionary)
                    //            {
                    //                DummyTagList.Add(item.Key, item.Value);

                    //            }
                    //            foreach (var tagItem in DummyTagList)
                    //            {

                    //                ChangeTagDictionary(tagItem, old_ConnectorName + ".", selectedProtocol.ConnectorName);

                    //            }
                    //            //foreach (var item in elpisServer.connectionHelper.ModbusIPMasterCollection)
                    //            //{//TODO for UPDATE Collection

                    //            //}                    
                    //            return;
                    //        }

                    //        //
                    //    }
                    //}
                    #endregion
                }
                catch (Exception)
                {

                }
            }
            else
            {
                MessageBox.Show(" Not selected Connector in list or No Connector is founded in list.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            //ProtocolListBox.SelectedIndex = index; 
        }

        private void ChangeTagDictionary(KeyValuePair<ISLIKTag, int> tagItem, string old_ConnectorName, string connectorName)
        {
            if (tagItem.Key.Name.ToString().Contains(old_ConnectorName))
            {
                ISLIKTag newKey = tagItem.Key;
                int newValue = tagItem.Value;
                string newName = newKey.Name.Replace(old_ConnectorName, connectorName + ".");
                //newKey.Name = newName;
                elpisServer.TagDictionary.Remove(tagItem.Key);
                //newKey.Name = newName;
                Tag tag = new Tag();
                AccessPermissionsEnum readWriteAccess = AccessPermissionsEnum.sdaReadAccess | AccessPermissionsEnum.sdaWriteAccess;
                // elpisServer.TagDictionary.Add(newKey, newValue);
                elpisServer.OpcTags.Remove(tagItem.Key.Name);
                elpisServer.OpcTags.Add(newName, (int)readWriteAccess, 0, 192, DateTime.Now, null);


                string[] DeviceTagNames = newName.Split('.');
                if (DeviceTagNames.Length == 4)
                {
                    DataType type = FindingDataType(ConnectorListBox.SelectedItem, DeviceTagNames);// Find Data type Based on the Selected protocol, device,tag.
                    switch (type)
                    {
                        case DataType.Boolean:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.Boolean;

                            break;
                        case DataType.Integer:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.Integer;

                            break;
                        case DataType.Short:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.Short;

                            break;
                        case DataType.String:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.String;

                            break;
                        case DataType.Double:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.Double;

                            break;
                    }
                    elpisServer.TagDictionary.Add(elpisServer.OpcTags[newName], newValue);

                }
            }
        }

        private DataType FindingDataType(dynamic selectedItem, string[] deviceTagNames)
        {
            if (selectedItem.DeviceCollection != null)
            {
                for (int i = 0; i < selectedItem.DeviceCollection.Count; i++)
                {
                    DeviceBase device = selectedItem.DeviceCollection[i] as DeviceBase;
                    if (device.DeviceName.ToLower() == deviceTagNames[2].ToLower())
                    {
                        if (device.TagsCollection != null)
                        {
                            for (int j = 0; j < device.TagsCollection.Count; j++)
                            {
                                Tag tag = device.TagsCollection[j] as Tag;
                                if (tag.TagName.ToLower() == deviceTagNames[3].ToLower())
                                {
                                    return tag.DataType;// as DataType;
                                }
                            }
                        }


                    }
                }
            }
            return 0;
        }

        private void ChangekeyValueinList(KeyValuePair<string, TcpClient> item, string oldConnectorName, string newName)
        {
            if (item.Key.ToString().Contains(oldConnectorName))
            {
                StringBuilder newKey = new StringBuilder();
                newKey.Append(item.Key);
                TcpClient newValue = item.Value as TcpClient;
                newKey = newKey.Replace(oldConnectorName, newName + ".");
                elpisServer.ConnectionHelperObj.tcpClientDictionary.Remove(item.Key);
                elpisServer.ConnectionHelperObj.tcpClientDictionary.Add(newKey.ToString(), newValue);

            }
        }

        private void DevicePropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //if (elpisServer.slikServerObject.ServerStatus!=ServerStatusEnum.sdaStatusRunning) //(elpisServer.runTimeDisplay != "Stop Runtime")
            //{
            PropertiesPropertyGrid.IsReadOnly = false;
            if (DeviceListBox.SelectedItem == null)
            {
                MessageBox.Show("No Device or Group is Selected in the list.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (DeviceListBox.SelectedItem.ToString().Contains("TagGroup"))
            {
                try
                {
                    propertyDisplayLbl.Content = "Property Editor- " + ((TagGroup)DeviceListBox.SelectedItem).GroupName;
                    EditTagGroup(DeviceListBox.SelectedItem as TagGroup);
                }
                catch (Exception)
                {
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    ElpisServer.Addlogs("Configuration", @"ElpisServer/Configuration", "problem in editing Tag Group", LogStatus.Error);
                    //}), DispatcherPriority.Normal, null);
                }
            }
            else
            {
                try
                {
                    var selectedItem = DeviceListBox.SelectedItem as DeviceBase;
                    propertyDisplayLbl.Content = "Property Editor- " + selectedItem.DeviceName;
                    EditElementinPropertyGrid(selectedItem);
                    #region old code for open new dialog box for editing Devive.
                    if (false)
                    {
                        //var selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
                        //if (selectedDevice != null)
                        //{
                        //    string device_Name = selectedDevice.DeviceName;
                        //    int curPort;
                        //    string curId = string.Empty;
                        //    string curModel = string.Empty;
                        //    if (selectedDevice.DeviceType == DeviceType.ModbusEthernet) //TODO in future we have to add different connector types 
                        //    {
                        //        curPort = ((ModbusEthernetDevice)selectedDevice).Port;
                        //        curId = ((ModbusEthernetDevice)selectedDevice).IPAddress;
                        //        curModel = selectedDevice.Model;
                        //    }
                        //    int index = 0;// DeviceListBox.SelectedIndex;
                        //    var selectedProtocol = ConnectorListBox.SelectedItem as ConnectorBase;
                        //    AddNewDeviceUI newDeviceWindow = new AddNewDeviceUI(selectedProtocol, selectedDevice);

                        //    newDeviceWindow.Title = "Property Editor- " + selectedDevice.DeviceName;
                        //    //propertyDisplayLbl.Content = "Property Editor- " + selectedDevice.DeviceName;
                        //    gridPropertylbl.Visibility = Visibility.Hidden;
                        //    PropertiesPropertyGrid.SelectedObject = null;
                        //    PropertiesPropertyGrid.BorderBrush = Brushes.White;
                        //    border.BorderBrush = Brushes.White;
                        //    propertyDisplayLbl.Content = "";
                        //    // propertyDisplayLbl.Background = Brushes.White;
                        //    newDeviceWindow.ShowDialog();
                        //    if (newDeviceWindow.CancelBtn.Background != Brushes.Red)
                        //    {

                        //        newDeviceWindow.DevicePropertyGrid.SelectedObject = selectedDevice;
                        //        //IDevice currentDevice = selectedDevice as IDevice;
                        //        for (int i = 0; i < DeviceListBox.Items.Count; i++)
                        //        {
                        //            if (i != index)
                        //            {
                        //                IDevice device = DeviceListBox.Items[i] as IDevice;
                        //                if (device.Name.ToLower() == selectedDevice.DeviceName.ToLower())
                        //                {
                        //                    selectedDevice.DeviceName = device_Name;
                        //                    gridPropertylbl.Visibility = Visibility.Visible;
                        //                    PropertiesPropertyGrid.SelectedObject = selectedDevice;
                        //                    PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                        //                    border.BorderBrush = Brushes.DarkBlue;
                        //                    propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedDevice.DeviceName);
                        //                    //propertyDisplayLbl.Background = Brushes.LightGreen;
                        //                    MessageBox.Show("Device with the same name is already exists.\nCreate with another name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //                    return;
                        //                }
                        //                else
                        //                {
                        //                    //currentDevice.Name = selectedDevice.DeviceName;
                        //                    gridPropertylbl.Visibility = Visibility.Visible;
                        //                    PropertiesPropertyGrid.SelectedObject = selectedDevice;
                        //                    PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                        //                    border.BorderBrush = Brushes.DarkBlue;
                        //                    propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedDevice.DeviceName);
                        //                }
                        //            }
                        //        }
                        //        isEdited = elpisServer.EditDevice(selectedProtocol, selectedDevice);
                        //        if (isEdited)
                        //        {
                        //            // PropertiesPropertyGrid.SelectedObject = selectedDevice;
                        //            gridPropertylbl.Visibility = Visibility.Visible;
                        //            PropertiesPropertyGrid.SelectedObject = selectedDevice;
                        //            PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                        //            border.BorderBrush = Brushes.DarkBlue;
                        //            propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedDevice.DeviceName);
                        //            // propertyDisplayLbl.Background = Brushes.LightGreen;
                        //            // check taglist is empty or not
                        //            Dictionary<ISLIKTag, int> DummyTagList = new Dictionary<ISLIKTag, int>();
                        //            foreach (var item in elpisServer.TagDictionary)
                        //            {
                        //                DummyTagList.Add(item.Key, item.Value);

                        //            }
                        //            foreach (var tagItem in DummyTagList)
                        //            {

                        //                //ChangeTagDictionary(tagItem, device_Name + ".", selectedDevice.DeviceName);
                        //                ChangeTagValue(tagItem, device_Name + ".", selectedDevice.DeviceName);
                        //            }

                        //        }
                        //    }
                        //    else
                        //    {
                        //        selectedDevice.DeviceName = device_Name;
                        //        selectedDevice.Model = curModel;
                        //        if (selectedDevice.DeviceType == DeviceType.ABControlLogic) //TODO Add conditions for different Connectors.
                        //        {
                        //            ((ModbusEthernetDevice)selectedDevice).Port = curPort;
                        //            ((ModbusEthernetDevice)selectedDevice).IPAddress = curId;

                        //        }
                        //        //set propertygrid
                        //        gridPropertylbl.Visibility = Visibility.Visible;
                        //        PropertiesPropertyGrid.SelectedObject = selectedDevice;
                        //        PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                        //        border.BorderBrush = Brushes.DarkBlue;
                        //        propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedDevice.DeviceName);
                        //        //propertyDisplayLbl.Background = Brushes.LightGreen;
                        //        return;
                        //    }
                        //}
                        //else
                        //{
                        //    MessageBox.Show("No Device is Selected in the list.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        //}
                    }
                    #endregion old code for open new dialog box for editing Devive.
                }
                catch (Exception)
                {
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    ElpisServer.Addlogs("Configuration", @"ElpisServer/Configuration", "problem in editing device", LogStatus.Error);
                    //}), DispatcherPriority.Normal, null);
                }
            }
            //}
            //else
            //{
            //    MessageBox.Show("Server is Running not able to Edit/Delete Tag", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            //}

        }

        private void EditElementinPropertyGrid(object selectedItem)
        {
            expProperty.IsExpanded = false; expProperty.BringIntoView();
            expProperty.IsExpanded = true;
            expProperty.BorderBrush = Brushes.Black;
            expProperty.BorderThickness = new Thickness(1);
            PropertiesPropertyGrid.SelectedObject = selectedItem;
            //PropertiesPropertyGrid.SelectedProperty = selectedItem.GroupName;
        }

        private void EditTagGroup(TagGroup selectedItem)
        {
            expProperty.IsExpanded = false; expProperty.BringIntoView();
            expProperty.IsExpanded = true;
            expProperty.BorderBrush = Brushes.Black;
            expProperty.BorderThickness = new Thickness(1);
            PropertiesPropertyGrid.SelectedObject = selectedItem;
            PropertiesPropertyGrid.SelectedProperty = selectedItem.GroupName;
            //expProperty.BorderBrush = Brushes.Transparent;
        }

        private void ChangeTagValue(KeyValuePair<ISLIKTag, int> tagItem, string device_Name, string newDeviceName)
        {
            if (tagItem.Key.Name.ToString().Contains(device_Name))
            {
                ISLIKTag newKey = tagItem.Key;
                int newValue = tagItem.Value;
                string newName = newKey.Name.Replace(device_Name, newDeviceName + ".");
                //newKey.Name = newName;
                elpisServer.TagDictionary.Remove(tagItem.Key);
                //newKey.Name = newName;
                //Tags tag = new Tags();
                AccessPermissionsEnum readWriteAccess = AccessPermissionsEnum.sdaReadAccess | AccessPermissionsEnum.sdaWriteAccess;
                // elpisServer.TagDictionary.Add(newKey, newValue);
                elpisServer.OpcTags.Remove(tagItem.Key.Name);
                elpisServer.OpcTags.Add(newName, (int)readWriteAccess, 0, 192, DateTime.Now, null);


                string[] DeviceTagNames = newName.Split('.');
                if (DeviceTagNames.Length == 4)
                {
                    DataType type = FindingDataType(ConnectorListBox.SelectedItem, DeviceTagNames);//Find Datatype Based onthe Selected protocol, device,tag.
                    switch (type)
                    {
                        case DataType.Boolean:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.Boolean;

                            break;
                        case DataType.Integer:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.Integer;

                            break;
                        case DataType.Short:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.Short;

                            break;
                        case DataType.String:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.String;

                            break;
                        case DataType.Double:
                            elpisServer.OpcTags[newName].DataType = (short)VariantType.Double;

                            break;
                    }
                    elpisServer.TagDictionary.Add(elpisServer.OpcTags[newName], newValue);
                }
            }
        }

        /// <summary>
        /// Editing the selected tag in the tag list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagPropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PropertiesPropertyGrid.IsReadOnly = false;
            try
            {
                var selectedTag = TaglListBox.SelectedItem as Tag;
                if (selectedTag != null)
                {
                    string tag_Name = selectedTag.TagName;
                    int index = TaglListBox.SelectedIndex;
                    string address = selectedTag.Address;
                    DataType dataType = selectedTag.DataType;
                    int scanRate = selectedTag.ScanRate;
                    propertyDisplayLbl.Content = "Property Editor- " + selectedTag.TagName;
                    #region Open new window when user clicks edit button in Tags.
                    if (false)
                    {
                        //AddNewTagUI newTagWindow = new AddNewTagUI(DeviceListBox.SelectedItem);
                        //newTagWindow.TagPropertyGrid.SelectedObject = selectedTag;
                        //newTagWindow.Title = "Property Editor- " + selectedTag.TagName;
                        ////propertyDisplayLbl.Content = "Property Editor- " + selectedTag.TagName;
                        //gridPropertylbl.Visibility = Visibility.Hidden;
                        //PropertiesPropertyGrid.SelectedObject = null;
                        //PropertiesPropertyGrid.BorderBrush = Brushes.White;
                        //border.BorderBrush = Brushes.White;
                        //propertyDisplayLbl.Content = "";
                        ////propertyDisplayLbl.Background = Brushes.White;
                        //newTagWindow.ShowDialog();
                        //if (newTagWindow.CancelBtn.Background == Brushes.Red)
                        //{
                        //    try
                        //    {
                        //        selectedTag.Address = address;
                        //        selectedTag.DataType = dataType;
                        //        selectedTag.ScanRate = scanRate;
                        //        selectedTag.TagName = tag_Name;
                        //        //set the Propertygrid
                        //        gridPropertylbl.Visibility = Visibility.Visible;
                        //        PropertiesPropertyGrid.SelectedObject = selectedTag;
                        //        PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                        //        border.BorderBrush = Brushes.Magenta;
                        //        propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedTag.TagName);
                        //        // propertyDisplayLbl.Background = Brushes.SkyBlue;
                        //        return;
                        //    }
                        //    catch (Exception ex)
                        //    {

                        //    }
                        //}
                        //else
                        //{
                        //    if (DeviceListBox.SelectedItem.ToString() == "OPCEngine.GroupingTags")
                        //    {
                        //        var selectedProtocol = ProtocolListBox.SelectedItem as ConnectorBase;
                        //        var device = DeviceFactory.GetDeviceByName(((GroupingTags)DeviceListBox.SelectedItem).DeviceName, selectedProtocol.DeviceCollection);
                        //        DeviceBase deviceObject = DeviceFactory.GetDeviceObjbyDevice(device);
                        //        isEdited = elpisServer.EditTag(deviceObject, TaglListBox.SelectedItem);
                        //    }
                        //    else
                        //    {
                        //        isEdited = elpisServer.EditTag(DeviceListBox.SelectedItem, TaglListBox.SelectedItem);
                        //    }

                        //    if (isEdited)
                        //    {
                        //        gridPropertylbl.Visibility = Visibility.Visible;
                        //        PropertiesPropertyGrid.SelectedObject = selectedTag;
                        //        PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                        //        border.BorderBrush = Brushes.Magenta;
                        //    }
                        //    gridPropertylbl.Visibility = Visibility.Visible;
                        //    PropertiesPropertyGrid.SelectedObject = selectedTag;
                        //    PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                        //    border.BorderBrush = Brushes.Magenta;
                        //    propertyDisplayLbl.Content = string.Format("{0}- Properties", selectedTag.TagName);
                        //    //propertyDisplayLbl.Background = Brushes.SkyBlue;
                        //    for (int i = 0; i < TaglListBox.Items.Count; i++)
                        //    {
                        //        if (i != index)
                        //        {
                        //            Tags pre_tag = TaglListBox.Items[i] as Tags;
                        //            if (pre_tag.TagName.ToLower() == selectedTag.TagName.ToLower())
                        //            {
                        //                selectedTag.TagName = tag_Name;
                        //                MessageBox.Show("Tag with same name already exists in the same device.\nUse another name to create a tag", "OPC Elpis Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //                return;
                        //            }
                        //        }
                        //    }

                        //    ITag currentTag = selectedTag as ITag;
                        //    currentTag.Name = selectedTag.TagName;
                        //}
                    }
                    #endregion
                    expProperty.BorderBrush = Brushes.Black;
                    EditElementinPropertyGrid(selectedTag);

                }
                else
                {
                    MessageBox.Show("No Tag is Selected in the list.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    //TaglListBox.SelectedItem = null;
                    // ProtocolListBox.SelectedItem = null;
                }

            }
            catch (Exception ex)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                ElpisServer.Addlogs("Configuration", @"ElpisServer/Configuration/DeleteTag", ex.Message, LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
        }


        /// <summary>
        /// When protocol list box lost focus, this event raised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProtocolListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //var editor = sender as ListBox;

            //var s = new Style(typeof(ListBoxItem));
            ////var enableSetter = new Setter { Property = IsEnabledProperty, Value = editor.IsSelectable };
            ////s.Setters.Add(enableSetter);

            //editor.ItemContainerStyle = s;

        }

        private void TaglListBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (PropertiesPropertyGrid.IsReadOnly)
            {
                //if (PropertiesPropertyGrid.SelectedObjectTypeName.Contains("Device"))
                //{
                //    return;
                //}
                //ListBox listbox = sender as ListBox;
                //if (listbox != null)
                //{
                //    if (listbox.Name == "ConnectorListBox")
                //    {
                //        if (ConnectorListBox.SelectedItem != null)
                //        {
                //            // propertyDisplayLbl.Background = Brushes.Magenta;
                //            propertyDisplayLbl.Content = string.Format("{0}- Properties", ((IConnector)ConnectorListBox.SelectedItem).Name);//ProtocolListBox.SelectedItem;
                //            gridPropertylbl.Visibility = Visibility.Visible;
                //            PropertiesPropertyGrid.SelectedObject = ConnectorListBox.SelectedItem;
                //            PropertiesPropertyGrid.Focus();
                //            PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                //            border.BorderBrush = Brushes.DarkOrange;
                //        }

                //    }
                //    else if (listbox.Name == "DeviceListBox")
                //    {
                //        if (DeviceListBox.SelectedItem != null)
                //        {
                //            // propertyDisplayLbl.Background = Brushes.LightGreen;
                //            propertyDisplayLbl.Content = string.Format("{0}- Properties", ((DeviceBase)DeviceListBox.SelectedItem).Name); //DeviceListBox.SelectedItem;
                //            PropertiesPropertyGrid.SelectedObject = DeviceListBox.SelectedItem;
                //            PropertiesPropertyGrid.Focus();
                //            PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                //            border.BorderBrush = Brushes.DarkBlue;
                //        }
                //    }
                //    else if (listbox.Name == "TaglListBox")
                //    {
                if (TaglListBox.Items.Count > 0)
                {
                    if (TaglListBox.SelectedItem != null) //&& ((System.Windows.FrameworkElement)TaglListBox.SelectedItem).Tag!=null)
                    {
                        // propertyDisplayLbl.Background = Brushes.SkyBlue;
                        propertyDisplayLbl.Content = string.Format("{0}- Properties", ((Tag)((System.Windows.Controls.Primitives.Selector)sender).SelectedItem).TagName); //DeviceListBox.SelectedItem;
                        gridPropertylbl.Visibility = Visibility.Visible;
                        PropertiesPropertyGrid.SelectedObject = TaglListBox.SelectedItem;
                        PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                        border.BorderBrush = Brushes.Magenta;
                    }
                    else
                    {
                        propertyDisplayLbl.Content = string.Format("{0}- Properties", ((Tag)((System.Windows.Controls.Primitives.Selector)sender).Items[0]).TagName); //DeviceListBox.SelectedItem;
                        gridPropertylbl.Visibility = Visibility.Visible;
                        TaglListBox.SelectedItem = (Tag)((System.Windows.Controls.Primitives.Selector)sender).Items[0];
                        PropertiesPropertyGrid.SelectedObject = TaglListBox.SelectedItem;
                        PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                        border.BorderBrush = Brushes.Magenta;
                    }
                }
                //else
                //{
                //    if (ConnectorListBox.Items.Count > 0)
                //    {
                //        propertyDisplayLbl.Content = string.Format("{0}- Properties", ((IConnector)ConnectorListBox.SelectedItem).Name);//ProtocolListBox.SelectedItem;
                //        gridPropertylbl.Visibility = Visibility.Visible;
                //        PropertiesPropertyGrid.SelectedObject = ConnectorListBox.SelectedItem;
                //        PropertiesPropertyGrid.Focus();
                //        PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                //        border.BorderBrush = Brushes.DarkOrange;
                //    }
                //}

                // }

                //  }
            }

            else
            {
                PropertiesPropertyGrid.IsReadOnly = true;
                // MessageBox.Show("To save changes press TAB or Enter Key..", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        /// <summary>
        /// Handle the event when click on any property on property grid.
        /// </summary>
        /// <param name="seneder"></param>
        /// <param name="e"></param>
        private void PropertiesPropertyGrid_SelectedPropertyItemChanged(object seneder, RoutedPropertyChangedEventArgs<Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemBase> e)
        {
            //if((((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.NewValue).PropertyType).Name=="ConnectorType")
            //{
            //    ((System.Windows.UIElement)e.OriginalSource).IsEnabled = false;
            //}
            isChanged = false;
            if (e.NewValue != null)
            {
                //spDesc.Background = propertyDisplayLbl.Background;
                //lblDescription.Visibility = Visibility.Visible;
                //tblDescription.Text = e.NewValue.Description;
            }
            else
            {
                // spDesc.Background = Brushes.LightGray;
                //tblDescription.Text = string.Empty;
                //lblDescription.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Updating the property grid selected object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertiesPropertyGrid_SelectedObjectChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            PropertiesPropertyGrid.IsReadOnly = true;
            expProperty.BorderBrush = Brushes.Transparent;
            if (e.OldValue != null && e.OldValue.GetType().Name == "TagGroup")
            {
                TagGroup group = e.OldValue as TagGroup;
                foreach (var item in group.TagsCollection)
                {
                    item.SelectedGroup = group.GroupName;
                }
            }
            isChanged = false;
            if (e.NewValue != null && e.NewValue.ToString() == "Elpis.Windows.OPC.Server.Tag")
            {
                if (DeviceListBox.SelectedItem != null)
                {
                    if (DeviceListBox.SelectedItem.ToString().Contains("TagGroup"))
                    {
                        var device = DeviceFactory.GetDeviceByName(((TagGroup)DeviceListBox.SelectedItem).DeviceName, ((ConnectorBase)ConnectorListBox.SelectedItem).DeviceCollection);
                        DeviceBase deviceObject = DeviceFactory.GetDevice(device);
                        // Elpis.Windows.OPC.Server.Tag.TagGroupItemsSource.GroupsTags = deviceObject.GroupCollection;
                    }
                    else
                    {
                        // Elpis.Windows.OPC.Server.Tag.TagGroupItemsSource.GroupsTags = ((DeviceBase)DeviceListBox.SelectedItem).GroupCollection;
                    }
                }
            }

            //if(ConnectorListBox.Items.Count>0)
            //{
            //    ConnectorBase selectedConnector = ConnectorListBox.SelectedItem as ConnectorBase;
            //    DeviceListBox.ItemsSource = selectedConnector.DeviceCollection;
            //    TaglListBox.ItemsSource = null;
            //}
        }



        public bool isChanged = false;
        //private bool ignoreResize;

        /// <summary>
        /// Update the edited property value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertiesPropertyGrid_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            try
            {
                if (e.NewValue == null || e.NewValue.ToString() == "")
                {
                    isChanged = false;
                }
                if (!isChanged)
                {
                    try
                    {
                        if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObjectTypeName == "TagGroup")
                        {
                            TagGroup group = ((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObject as TagGroup;
                            //To Edit the Tag Group
                            TagGroupPropertiesEditor(((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObject, e, group);
                        }

                        else if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObjectTypeName.Contains("Connector"))
                        {
                            //To Edit the Connector
                            ConnectorPropertiesEditor(((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObject, e);
                        }

                        else if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObjectTypeName.Contains("Device"))
                        {
                            //To Edit the Device
                            DevicePropertiesEditor(((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObject, e);
                        }

                        else if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObjectTypeName == "Tag")
                        {
                            //To Edit the Tag
                            TagPropertiesEditor(((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObject, e);
                            elpisServer.SaveLastLoadedProject();
                        }
                        else
                        {
                            //unknown 
                        }
                    }
                    catch (Exception ex)
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        ElpisServer.Addlogs("Configuration", @"Elpis/Communication", ex.Message, LogStatus.Error);
                        //}), DispatcherPriority.Normal, null);
                    }
                }


                else
                {
                    isChanged = false;
                }
            }
            catch (Exception)
            {
               
            }
            //PropertiesPropertyGrid.IsReadOnly = true;
            //expProperty.BorderBrush = Brushes.Transparent;
        }

        private void TagGroupPropertiesEditor(object selectedObject, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e, TagGroup group)
        {

            bool validName = Regex.IsMatch(group.GroupName, "^[A-Za-z0-9_]{1,40}$");
            if (validName)
            {
                if (group.GroupName.ToLower() == "none")
                {
                    MessageBox.Show("Please enter valid Group Name.\nThe Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    isChanged = true;
                    group.GroupName = e.OldValue.ToString().Trim();
                }
                else
                {
                    try
                    {
                        var selectedProtocol = ConnectorListBox.SelectedItem as ConnectorBase;
                        isEdited = elpisServer.EditGroup(group, selectedProtocol.DeviceCollection, e.OldValue.ToString());
                        if (isEdited)
                        {
                            gridPropertylbl.Visibility = Visibility.Visible;
                            PropertiesPropertyGrid.SelectedObject = group;
                            PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                            border.BorderBrush = Brushes.DarkBlue;
                            return;
                        }
                        else
                        {
                            group.GroupName = e.OldValue.ToString().Trim();
                            isChanged = true;
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        ElpisServer.Addlogs("Configuration", @"Elpis/Communication", "Problem in editing Tag Group", LogStatus.Error);
                        //}), DispatcherPriority.Normal, null);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter valid Group Name.\nThe Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                isChanged = true;
                group.GroupName = e.OldValue.ToString().Trim();
            }
        }

        private void TagPropertiesEditor(object selectedObject, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            var selectedConnector = ConnectorListBox.SelectedItem as ConnectorBase;
            var selectedTag = TaglListBox.SelectedItem as Tag;
            if (selectedTag != null)
            {
                if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "Address")
                {
                    if (Regex.IsMatch(selectedTag.Address.ToString(), @"^[0-9]+$"))
                    {
                        DeviceBase selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
                        string key = null;
                        if (selectedTag.SelectedGroup == null || selectedTag.SelectedGroup == "None")
                            key = string.Format("User.{0}.{1}.{2}", selectedDevice.ConnectorAssignment, selectedDevice.DeviceName, selectedTag.TagName);
                        else
                            key = string.Format("User.{0}.{1}.{2}.{3}", selectedDevice.ConnectorAssignment, selectedDevice.DeviceName, selectedTag.SelectedGroup, selectedTag.TagName);

                        ISLIKTag slikdaTag = elpisServer.OpcTags[key];
                        elpisServer.TagDictionary[slikdaTag] = int.Parse(selectedTag.Address);
                        isChanged = true;
                        return;
                    }
                    else
                    {
                        isChanged = true;
                        selectedTag.Address = e.OldValue.ToString();
                        return;
                    }
                }
                if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "TagName")
                {
                    bool validName = Regex.IsMatch(selectedTag.TagName, "^[A-Za-z0-9_]{1,40}$");
                    if (validName)
                    {
                        if (DeviceListBox.SelectedItem.ToString().Contains("TagGroup"))
                        {
                            var device = DeviceFactory.GetDeviceByName(((TagGroup)DeviceListBox.SelectedItem).DeviceName, selectedConnector.DeviceCollection);
                            DeviceBase deviceObject = DeviceFactory.GetDevice(device);
                            isEdited = elpisServer.EditTag(deviceObject, selectedTag);
                        }
                        else
                        {
                            isEdited = elpisServer.EditTag(DeviceListBox.SelectedItem as DeviceBase, selectedTag);
                        }

                        if (isEdited)
                        {
                            gridPropertylbl.Visibility = Visibility.Visible;
                            PropertiesPropertyGrid.SelectedObject = selectedTag;
                            PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                            border.BorderBrush = Brushes.Magenta;
                            isChanged = true;

                            //Adding a record to log window
                            elpisServer.LoadDataLogCollection("Configuration", "Tag Properties", selectedTag.TagName.ToString() + " Properties have been updated successfully.", LogStatus.Information);
                            LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;
                        }
                        else
                        {
                            isChanged = true;
                            if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "Address")
                                selectedTag.Address = e.OldValue.ToString();
                        }
                    }
                    else
                    {
                        isChanged = true;
                        selectedTag.TagName = e.OldValue.ToString(); //((OPCEngine.Tag)selectedTag).TagName;
                        MessageBox.Show("Please enter valid Tag Name.\nThe Tag Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }

                if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "SelectedGroup")
                {
                    DeviceBase device = null;
                    //var tagGroup = device.GroupCollection;
                    Tag tag = TaglListBox.SelectedItem as Tag;
                    string key = null;
                    if (e.OldValue == null || e.OldValue.ToString() == null || e.OldValue.ToString() == "None")
                    {
                        device = DeviceListBox.SelectedItem as DeviceBase;
                        key = string.Format("User.{0}.{1}.{2}", device.ConnectorAssignment, device.DeviceName, tag.TagName);
                    }
                    else
                    {
                        var tagGroup = DeviceListBox.SelectedItem as TagGroup;
                        device = selectedConnector.DeviceCollection.FirstOrDefault(d => d.DeviceName == tagGroup.DeviceName);
                        key = string.Format("User.{0}.{1}.{2}.{3}", device.ConnectorAssignment, device.DeviceName, e.OldValue.ToString(), tag.TagName);
                    }

                    //changedGroup.TagsCollection.Add(tag);
                    //string key = string.Format("User.{0}.{1}.{2}", device.ConnectorAssignment, device.DeviceName, tag.TagName);

                    bool isMoved = elpisServer.NewTag(device, tag);
                    if (isMoved)
                    {
                        if (e.NewValue.ToString() == null || e.NewValue.ToString() == "None")
                        {
                            TagGroup changedGroup = device.GroupCollection.FirstOrDefault(g => g.GroupName.ToLower() == e.OldValue.ToString().ToLower());
                            changedGroup.TagsCollection.Remove(tag);
                        }
                        else if (e.OldValue.ToString() != "None" && e.NewValue.ToString() != "None")
                        {
                            TagGroup changedGroup = device.GroupCollection.FirstOrDefault(g => g.GroupName.ToLower() == e.OldValue.ToString().ToLower());
                            changedGroup.TagsCollection.Remove(tag);
                        }
                        else
                        {

                            device.TagsCollection.Remove(tag);
                        }

                        ISLIKTag slikdaTag = elpisServer.OpcTags[key];
                        elpisServer.OpcTags.Remove(key);
                        string scanRateKey = string.Format("{0}.{1}.{2}", tag.ScanRate, device.ConnectorAssignment, device.DeviceName);
                        List<ISLIKTag> slikdaTagList = elpisServer.ScanrateGroup[scanRateKey];
                        slikdaTagList.Remove(slikdaTag);
                        elpisServer.TagDictionary.Remove(slikdaTag);
                        isChanged = true;
                    }
                    else
                    {
                        isChanged = true;
                        tag.SelectedGroup = e.OldValue.ToString();
                        //TaglListBox.SelectedItem = tag;
                        PropertiesPropertyGrid.SelectedObject = null;

                        PropertiesPropertyGrid.SelectedObject = tag;
                        return;
                    }
                    return;
                }
            }
        }

        private void DevicePropertiesEditor(object selectedObject, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            try
            {
                var selectedConnector = ConnectorListBox.SelectedItem as ConnectorBase;

                DeviceBase deviceBase = DeviceListBox.SelectedItem as DeviceBase;
                if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "DeviceName")
                {
                    bool validName = Regex.IsMatch(deviceBase.DeviceName, "^[A-Za-z0-9_]{1,40}$");
                    if (validName)
                    {
                        isEdited = elpisServer.EditDevice(selectedConnector, deviceBase);
                        if (isEdited)
                        {
                            isChanged = true;
                            gridPropertylbl.Visibility = Visibility.Visible;
                            PropertiesPropertyGrid.SelectedObject = deviceBase;
                            PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                            border.BorderBrush = Brushes.DarkBlue;
                        }
                    }

                    else
                    {
                        isChanged = true;
                        deviceBase.DeviceName = ((DeviceBase)deviceBase).Name;
                        MessageBox.Show("Please enter valid Device Name.\nThe Device Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                else if ((DeviceListBox.SelectedItem as DeviceBase).DeviceType == DeviceType.ModbusEthernet)
                {
                    ModbusEthernetDevice selectedDevice = DeviceListBox.SelectedItem as ModbusEthernetDevice;
                    if (selectedDevice != null)
                    {
                        string ip = Regex.IsMatch(selectedDevice.IPAddress, @"^[0 - 9]{ 1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$") ? e.OldValue.ToString() : selectedDevice.IPAddress;
                        string port = Regex.IsMatch(selectedDevice.Port.ToString(), @"^[0 - 9]+$") ? e.OldValue.ToString() : selectedDevice.Port.ToString();

                        if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "IPAddress")
                        {
                            isChanged = true;
                            selectedDevice.IPAddress = Regex.IsMatch(selectedDevice.IPAddress, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$") ? selectedDevice.IPAddress : e.OldValue.ToString(); // e.OldValue.ToString();
                            return;
                        }
                        if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "Port")
                        {
                            isChanged = true;
                            selectedDevice.Port = ushort.Parse(Regex.IsMatch(selectedDevice.Port.ToString(), @"^[1-9][0-9]*$") ? selectedDevice.Port.ToString() : e.OldValue.ToString());
                            return;
                        }
                        if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "RetryCount")
                        {
                            isChanged = true;
                            selectedDevice.RetryCount = uint.Parse(Regex.IsMatch(selectedDevice.RetryCount.ToString(), @"^[1-9][0-9]*$") ? selectedDevice.RetryCount.ToString() : e.OldValue.ToString());
                            return;
                        }
                        //isChanged = true;
                    }
                }


                else if ((DeviceListBox.SelectedItem as DeviceBase).DeviceType == DeviceType.ModbusSerial)
                {
                    ModbusSerialDevice selectedDevice = DeviceListBox.SelectedItem as ModbusSerialDevice;
                    if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "BaudRate")
                    {
                        isChanged = true;
                        selectedDevice.BaudRate = int.Parse(Regex.IsMatch(selectedDevice.BaudRate.ToString(), @"^[1-9][0-9]*$") ? selectedDevice.BaudRate.ToString() : e.OldValue.ToString());
                        return;
                    }

                    if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "RetryCount")
                    {
                        isChanged = true;
                        selectedDevice.RetryCount = uint.Parse(Regex.IsMatch(selectedDevice.RetryCount.ToString(), @"^[1-9][0-9]*$") ? selectedDevice.RetryCount.ToString() : e.OldValue.ToString());
                        return;
                    }

                    if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "COMPort")
                    {
                        isChanged = true;
                        selectedDevice.COMPort = (Regex.IsMatch(selectedDevice.COMPort.ToString(), @"^COM[0-9][0-9]*$") ? selectedDevice.COMPort.ToString() : e.OldValue.ToString());
                        //  UpdateModbusSerialMasterList(selectedDevice);
                        return;
                    }

                    if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "COMPort")
                    {
                        isChanged = true;
                        selectedDevice.DataBits = int.Parse(Regex.IsMatch(selectedDevice.DataBits.ToString(), @"^[0-9][0-9]*$") ? selectedDevice.DataBits.ToString() : e.OldValue.ToString());
                        return;
                    }

                }
                //}
                //    else
                //    {
                //    // selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
                //}
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

        }

        private void UpdateModbusSerialMasterList(ModbusSerialDevice device)
        {
            string key = string.Format("{0}.{1}", device.ConnectorAssignment, device.DeviceName);
            elpisServer.ConnectionHelperObj.ModbusSerialMasterCollection.Remove(key);
            elpisServer.CreateModbusSerialMaster(device.ConnectorAssignment, device.DeviceName);
        }

        private void ConnectorPropertiesEditor(object selectedObject, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            var selectedConnector = ConnectorListBox.SelectedItem as ConnectorBase;
            if (selectedConnector != null)
            {
                try
                {
                    //To Edit the Protocol                    
                    if (selectedConnector != null)
                    {
                        if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "ConnectorType")
                        {
                            selectedConnector.TypeofConnector = (ConnectorType)e.OldValue;
                        }

                        //propertyDisplayLbl.Background = Brushes.Magenta;
                        bool validName = Regex.IsMatch(selectedConnector.ConnectorName, "^[A-Za-z0-9_]{1,40}$");
                        if (validName)
                        {
                            isEdited = elpisServer.EditConnector(selectedConnector);
                            if (isEdited)
                            {
                                gridPropertylbl.Visibility = Visibility.Visible;
                                PropertiesPropertyGrid.SelectedObject = selectedConnector;
                                PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                                border.BorderBrush = Brushes.DarkOrange;
                                return;
                            }
                        }
                        else
                        {
                            isChanged = true;
                            selectedConnector.ConnectorName = ((IConnector)selectedConnector).Name;
                            MessageBox.Show("Please enter valid Connector Name.\nThe connector name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        //else
                        //{
                        //    MessageBox.Show("Property Editor having some errors, check each row and enter values.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        //}
                    }
                }
                catch (Exception)
                {

                }
            }
        }






        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// <summary>
        /// Update the edited property value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertiesPropertyGrid_PropertyValueChanged1(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            try
            {
                if (e.NewValue == null || e.NewValue.ToString() == "")
                {
                    isChanged = false;
                }
                if (!isChanged)
                {
                    bool isEdited;

                    if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObjectTypeName == "TagGroup")
                    {
                        TagGroup group = ((Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)sender).SelectedObject as TagGroup;

                        bool validName = Regex.IsMatch(group.GroupName, "^[A-Za-z0-9_]{1,40}$");
                        if (validName)
                        {
                            if (group.GroupName.ToLower() == "none")
                            {
                                MessageBox.Show("Please enter valid Group Name.\nThe Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                                isChanged = true;
                                group.GroupName = e.OldValue.ToString().Trim();
                            }
                            else
                            {
                                try
                                {
                                    var selectedProtocol = ConnectorListBox.SelectedItem as ConnectorBase;
                                    isEdited = elpisServer.EditGroup(group, selectedProtocol.DeviceCollection, e.OldValue.ToString());
                                    if (isEdited)
                                    {
                                        gridPropertylbl.Visibility = Visibility.Visible;
                                        PropertiesPropertyGrid.SelectedObject = group;
                                        PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                                        border.BorderBrush = Brushes.DarkBlue;
                                        return;
                                    }
                                    else
                                    {
                                        isChanged = true;
                                        group.GroupName = e.OldValue.ToString().Trim();
                                        return;
                                    }
                                }
                                catch (Exception)
                                {
                                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    //{
                                    ElpisServer.Addlogs("Configuration", @"Elpis/Communication", "Problem in editing Tag Group", LogStatus.Error);
                                    //}), DispatcherPriority.Normal, null);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please enter valid Group Name.\nThe Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                            isChanged = true;
                            group.GroupName = e.OldValue.ToString().Trim();
                        }
                    }

                    else
                    {
                        try
                        {
                            //To Edit the Protocol
                            var selectedConnector = ConnectorListBox.SelectedItem as ConnectorBase;
                            if (selectedConnector != null)
                            {
                                //propertyDisplayLbl.Background = Brushes.Magenta;
                                bool validName = Regex.IsMatch(selectedConnector.ConnectorName, "^[A-Za-z0-9_]{1,40}$");
                                if (validName)
                                {
                                    isEdited = elpisServer.EditConnector(selectedConnector);
                                    if (isEdited)
                                    {
                                        gridPropertylbl.Visibility = Visibility.Visible;
                                        PropertiesPropertyGrid.SelectedObject = selectedConnector;
                                        PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                                        border.BorderBrush = Brushes.DarkOrange;
                                        return;
                                    }
                                }
                                else
                                {
                                    isChanged = true;
                                    selectedConnector.ConnectorName = ((IConnector)selectedConnector).Name;
                                    MessageBox.Show("Please enter valid Connector Name.\nThe connector name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                                //else
                                //{
                                //    MessageBox.Show("Property Editor having some errors, check each row and enter values.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                                //}
                            }

                            //To Edit the Device
                            // var selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
                            try
                            {
                                if ((DeviceListBox.SelectedItem as DeviceBase).DeviceType == DeviceType.ModbusEthernet)
                                {
                                    ModbusEthernetDevice selectedDevice = DeviceListBox.SelectedItem as ModbusEthernetDevice;
                                    if (selectedDevice != null)
                                    {
                                        string ip = Regex.IsMatch(selectedDevice.IPAddress, @"^[0 - 9]{ 1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$") ? e.OldValue.ToString() : selectedDevice.IPAddress;
                                        string port = Regex.IsMatch(selectedDevice.Port.ToString(), @"^[0 - 9]+$") ? e.OldValue.ToString() : selectedDevice.Port.ToString();
                                        //propertyDisplayLbl.Background = Brushes.LightGreen;
                                        if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "DeviceName")
                                        {
                                            bool validName = Regex.IsMatch(selectedDevice.DeviceName, "^[A-Za-z0-9_]{1,40}$");
                                            if (validName)
                                            {
                                                isEdited = elpisServer.EditDevice(selectedConnector, selectedDevice);
                                                if (isEdited)
                                                {
                                                    isChanged = true;
                                                    gridPropertylbl.Visibility = Visibility.Visible;
                                                    PropertiesPropertyGrid.SelectedObject = selectedDevice;
                                                    PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                                                    border.BorderBrush = Brushes.DarkBlue;
                                                }
                                            }

                                            else
                                            {
                                                isChanged = true;
                                                selectedDevice.DeviceName = ((DeviceBase)selectedDevice).Name;
                                                MessageBox.Show("Please enter valid Device Name.\nThe Device Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                            return;
                                        }
                                        else
                                        {
                                            //isChanged = true;
                                            if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "IPAddress")
                                            {
                                                isChanged = true;
                                                selectedDevice.IPAddress = Regex.IsMatch(selectedDevice.IPAddress, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$") ? selectedDevice.IPAddress : e.OldValue.ToString(); // e.OldValue.ToString();
                                                return;
                                            }
                                            if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "Port")
                                            {
                                                isChanged = true;
                                                selectedDevice.Port = ushort.Parse(Regex.IsMatch(selectedDevice.Port.ToString(), @"^[0-9]+$") ? selectedDevice.Port.ToString() : e.OldValue.ToString());
                                                return;
                                            }
                                            if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "RetryCount")
                                            {
                                                isChanged = true;
                                                selectedDevice.RetryCount = uint.Parse(Regex.IsMatch(selectedDevice.RetryCount.ToString(), @"^[1-9][0-9]*$") ? selectedDevice.RetryCount.ToString() : e.OldValue.ToString());
                                                return;
                                            }
                                            //isChanged = true;
                                        }

                                    }


                                }
                                //}
                                //    else
                                //    {
                                //    // selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
                                //}
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                            }

                            //To Edit the Tag
                            var selectedTag = TaglListBox.SelectedItem as Tag;
                            if (selectedTag != null)
                            {
                                if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "Address")
                                {
                                    if (Regex.IsMatch(selectedTag.Address.ToString(), @"^[0-9]+$"))
                                    {
                                        DeviceBase selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
                                        string key = null;
                                        if (selectedTag.SelectedGroup == null || selectedTag.SelectedGroup == "None")
                                            key = string.Format("User.{0}.{1}.{2}", selectedDevice.ConnectorAssignment, selectedDevice.DeviceName, selectedTag.TagName);
                                        else
                                            key = string.Format("User.{0}.{1}.{2}.{3}", selectedDevice.ConnectorAssignment, selectedDevice.DeviceName, selectedTag.SelectedGroup, selectedTag.TagName);

                                        ISLIKTag slikdaTag = elpisServer.OpcTags[key];
                                        elpisServer.TagDictionary[slikdaTag] = int.Parse(selectedTag.Address);
                                        isChanged = true;
                                        return;
                                    }
                                    else
                                    {
                                        isChanged = true;
                                        selectedTag.Address = e.OldValue.ToString();
                                        return;
                                    }
                                }
                                if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "TagName")
                                {
                                    bool validName = Regex.IsMatch(selectedTag.TagName, "^[A-Za-z0-9_]{1,40}$");
                                    if (validName)
                                    {
                                        if (DeviceListBox.SelectedItem.ToString().Contains("TagGroup"))
                                        {
                                            var device = DeviceFactory.GetDeviceByName(((TagGroup)DeviceListBox.SelectedItem).DeviceName, selectedConnector.DeviceCollection);
                                            DeviceBase deviceObject = DeviceFactory.GetDevice(device);
                                            isEdited = elpisServer.EditTag(deviceObject, selectedTag);
                                        }
                                        else
                                        {
                                            isEdited = elpisServer.EditTag(DeviceListBox.SelectedItem as DeviceBase, selectedTag);
                                        }

                                        if (isEdited)
                                        {
                                            gridPropertylbl.Visibility = Visibility.Visible;
                                            PropertiesPropertyGrid.SelectedObject = selectedTag;
                                            PropertiesPropertyGrid.BorderBrush = Brushes.Magenta;
                                            border.BorderBrush = Brushes.Magenta;
                                            isChanged = true;

                                            //Adding a record to log window
                                            elpisServer.LoadDataLogCollection("Configuration", "Tag Properties", selectedTag.TagName.ToString() + " Properties have been updated successfully.", LogStatus.Information);
                                            LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;
                                        }
                                        else
                                        {
                                            isChanged = true;
                                            if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "Address")
                                                selectedTag.Address = e.OldValue.ToString();
                                        }
                                    }
                                    else
                                    {
                                        isChanged = true;
                                        selectedTag.TagName = e.OldValue.ToString(); //((OPCEngine.Tag)selectedTag).TagName;
                                        MessageBox.Show("Please enter valid Tag Name.\nThe Tag Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }

                                }

                                if (((Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)e.OriginalSource).PropertyName == "SelectedGroup")
                                {
                                    DeviceBase device = null;
                                    //var tagGroup = device.GroupCollection;
                                    Tag tag = TaglListBox.SelectedItem as Tag;
                                    string key = null;
                                    if (e.OldValue == null || e.OldValue.ToString() == null || e.OldValue.ToString() == "None")
                                    {
                                        device = DeviceListBox.SelectedItem as DeviceBase;
                                        key = string.Format("User.{0}.{1}.{2}", device.ConnectorAssignment, device.DeviceName, tag.TagName);
                                    }
                                    else
                                    {
                                        var tagGroup = DeviceListBox.SelectedItem as TagGroup;
                                        device = selectedConnector.DeviceCollection.FirstOrDefault(d => d.DeviceName == tagGroup.DeviceName);
                                        key = string.Format("User.{0}.{1}.{2}.{3}", device.ConnectorAssignment, device.DeviceName, e.OldValue.ToString(), tag.TagName);
                                    }

                                    if (e.NewValue.ToString() == null || e.NewValue.ToString() == "None")
                                    {

                                    }
                                    else
                                    {
                                        TagGroup changedGroup = device.GroupCollection.FirstOrDefault(g => g.GroupName.ToLower() == e.NewValue.ToString().ToLower());
                                    }

                                    //changedGroup.TagsCollection.Add(tag);
                                    //string key = string.Format("User.{0}.{1}.{2}", device.ConnectorAssignment, device.DeviceName, tag.TagName);
                                    bool isMoved = elpisServer.NewTag(device, tag);
                                    if (isMoved)
                                    {
                                        device.TagsCollection.Remove(tag);
                                        ISLIKTag slikdaTag = elpisServer.OpcTags[key];
                                        elpisServer.OpcTags.Remove(key);
                                        elpisServer.TagDictionary.Remove(slikdaTag);
                                        isChanged = true;
                                    }
                                    else
                                    {
                                        tag.SelectedGroup = e.OldValue.ToString();
                                    }
                                    return;
                                }


                            }
                        }
                        catch (Exception ex)
                        {
                            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            //{
                            ElpisServer.Addlogs("Configuration", @"Elpis/Communication", ex.Message, LogStatus.Error);
                            //}), DispatcherPriority.Normal, null);
                        }
                    }

                }
                else
                {
                    isChanged = false;
                }
            }
            catch (Exception)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                ElpisServer.Addlogs("Configuration", @"Elpis/Communication", "Problem in property grid value changed", LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }

        }

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void SortList()
        {
            ConnectorListBox.Items.SortDescriptions.Add(new SortDescription("ConnectorName", ListSortDirection.Ascending));
            DeviceListBox.Items.SortDescriptions.Add(new SortDescription("DeviceName", ListSortDirection.Ascending));
            TaglListBox.Items.SortDescriptions.Add(new SortDescription("TagName", ListSortDirection.Ascending));
        }
        public void DefaultListSelection()
        {
            dynamic selectedItem = ConnectorListBox.Items[0];
            ConnectorListBox.SelectedIndex = 0;
            gridPropertylbl.Visibility = Visibility.Visible;
            PropertiesPropertyGrid.SelectedObject = selectedItem;
            PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
            border.BorderBrush = Brushes.DarkOrange;
        }

        //private void btnTagGroup_Click(object sender, RoutedEventArgs e)
        //{
        //   // bool isCreated;
        //    var selectedDevice = DeviceListBox.SelectedItem as DeviceBase;
        //    if (selectedDevice != null)
        //    {
        //        AddNewTagGroup newCreateTagWindow = new AddNewTagGroup(selectedDevice);
        //        newCreateTagWindow.ShowDialog();
        //        if (newCreateTagWindow.tag != null)
        //        {
        //        //    if (newCreateTagWindow.tag.TagName != null)
        //        //    {
        //        //        if (string.IsNullOrEmpty(newCreateTagWindow.tag.TagName))
        //        //        {
        //        //            MessageBox.Show("Tag name cannot be empty", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        //            return;
        //        //        }
        //        //        if (string.IsNullOrEmpty(newCreateTagWindow.tag.Address))
        //        //        {
        //        //            MessageBox.Show("Address cannot be empty", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        //            return;
        //        //        }
        //        //        for (int i = 0; i < TaglListBox.Items.Count; i++)
        //        //        {
        //        //            //if (((System.Windows.FrameworkElement)(TaglListBox.Items[i])).Tag != null)
        //        //            {
        //        //                Tags listTag = TaglListBox.Items[i] as Tags;
        //        //                if (listTag.TagName.ToLower() == newCreateTagWindow.tag.TagName.ToLower())
        //        //                {
        //        //                    MessageBox.Show("Tag with same name already exists in the same device.\nUse another name to create a tag", "OPC Elpis Server", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        //                }
        //        //            }

        //        //        }

        //        //        newCreateTagWindow.obj.Name = newCreateTagWindow.tag.TagName;
        //        //        //MainWindow.opcEngineMainWindowViewModel.NewTag(newCreateTagWindow.tag, deviceViewModel);
        //        //        isCreated = elpisServer.NewTag(selectedDevice, newCreateTagWindow.tag);
        //        //        if (isCreated)
        //        //        {
        //        //            if (selectedDevice.TagsCollection != null)
        //        //            {
        //        //                TaglListBox.ClearValue(ItemsControl.ItemsSourceProperty);
        //        //                TaglListBox.Items.Clear();
        //        //                TaglListBox.ItemsSource = selectedDevice.TagsCollection;
        //        //                TaglListBox.DisplayMemberPath = "TagName";
        //        //                TaglListBox.SelectedItem = null;
        //        //                TaglListBox.SelectedItem = selectedDevice.TagsCollection[selectedDevice.TagsCollection.Count - 1];
        //        //            }

        //        //            elpisServer.LoadDataLogCollection("Configuration", selectedDevice.Type, "Tag: " +
        //        //                 newCreateTagWindow.tag.TagName + " has created successfully.", LogStatus.Information);
        //        //            LoggerUserControl.ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;// elpisServer.LoggerCollection;
        //        //            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(currentCollection);
        //        //            //view.SortDescriptions.Add(new SortDescription("Age", ListSortDirection.Ascending));
        //        //        }
        //        //        else
        //        //        {
        //        //            elpisServer.LoadDataLogCollection("Configuration", selectedDevice.DeviceName,
        //        //                                               "Unable to create the tag, check the list of tags.", LogStatus.Error);
        //        //        }
        //        //    }
        //        //    else
        //        //        MessageBox.Show("Tag name cannot be empty", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
        //       }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Select the appropriate device and Connector first!!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //}
        #region about window
        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            Window aboutWindow = new Window();
            aboutWindow.Content = about;
            aboutWindow.WindowStyle = WindowStyle.ToolWindow;
            aboutWindow.Show();
            //about.Visibility = Visibility.Visible;
            //MainWindow2.contentPresenter.Content = about;
        }
        #endregion

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)e.OriginalSource).Name == "searchBox")
            {
                if (ConnectorListBox.ItemsSource != null)
                {
                    CollectionViewSource.GetDefaultView(ConnectorListBox.ItemsSource).Refresh();
                    SetPropertyGridObject("Connector");
                }
            }
            else if (((System.Windows.FrameworkElement)e.OriginalSource).Name == "searchBoxDevice")
            {
                if (ConnectorListBox.ItemsSource != null && DeviceListBox.ItemsSource != null)
                {
                    CollectionViewSource.GetDefaultView(DeviceListBox.ItemsSource).Refresh();
                    SetPropertyGridObject("Device");
                }
            }
            else if (((System.Windows.FrameworkElement)e.OriginalSource).Name == "searchBoxTag")
            {
                if (ConnectorListBox.ItemsSource != null && DeviceListBox.ItemsSource != null && TaglListBox.ItemsSource != null)
                {
                    CollectionViewSource.GetDefaultView(TaglListBox.ItemsSource).Refresh();
                    SetPropertyGridObject("Tag");
                }
            }
        }

        private void searchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((System.Windows.FrameworkElement)e.OriginalSource).Name == "searchBox")
            {
                if (ConnectorListBox.ItemsSource != null)
                {
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ConnectorListBox.ItemsSource);
                    view.Filter = ListFullSource;
                    //SetPropertyGridObject("Connector");
                }
                else
                {
                    propertyDisplayLbl.Content = "";
                    gridPropertylbl.Visibility = Visibility.Hidden;
                }
            }
            else if (((System.Windows.FrameworkElement)e.OriginalSource).Name == "searchBoxDevice")
            {
                if (ConnectorListBox.SelectedItem != null && DeviceListBox.ItemsSource != null)
                {
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(DeviceListBox.ItemsSource);
                    view.Filter = ListFullSourceD;
                }
                else
                {
                    propertyDisplayLbl.Content = "";
                    gridPropertylbl.Visibility = Visibility.Hidden;
                }
            }
            else if (((System.Windows.FrameworkElement)e.OriginalSource).Name == "searchBoxTag")
            {
                if (ConnectorListBox.SelectedItem != null && DeviceListBox.SelectedItem != null && TaglListBox.ItemsSource != null)
                {
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(TaglListBox.ItemsSource);
                    view.Filter = ListFullSourceT;
                }
                else
                {
                    propertyDisplayLbl.Content = "";
                    gridPropertylbl.Visibility = Visibility.Hidden;
                }
            }
        }

        private bool ListFullSource(object itemList)
        {
            //SetPropertyGridObject("Connector");
            if (String.IsNullOrEmpty(searchBox.Text))
                return true;
            else
            {
                return ((ConnectorBase)itemList).ConnectorName.IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }

        }

        private void SetPropertyGridObject(string item)
        {
            switch (item)
            {
                case "Connector":
                    if (PropertiesPropertyGrid.SelectedObject != null)
                    {
                        propertyDisplayLbl.Content = string.Format("{0}- Properties", ((ConnectorBase)PropertiesPropertyGrid.SelectedObject).ConnectorName);
                        gridPropertylbl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        propertyDisplayLbl.Content = "";
                        gridPropertylbl.Visibility = Visibility.Hidden;
                    }
                    break;
                case "Device":
                    if (PropertiesPropertyGrid.SelectedObject != null)
                    {
                        //propertyDisplayLbl.Content=string.Format("{0}- Properties",)
                        gridPropertylbl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        propertyDisplayLbl.Content = "";
                        gridPropertylbl.Visibility = Visibility.Hidden;
                    }
                    break;
                case "Tag":
                    if (PropertiesPropertyGrid.SelectedObject != null)
                    {
                        //propertyDisplayLbl.Content=string.Format("{0}- Properties",)
                        gridPropertylbl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        propertyDisplayLbl.Content = "";
                        gridPropertylbl.Visibility = Visibility.Hidden;
                    }
                    break;
            }

        }

        private bool ListFullSourceD(object itemList)
        {
            //SetPropertyGridObject("Device");
            if (String.IsNullOrEmpty(searchBoxDevice.Text))
                return true;
            else
                return ((DeviceBase)itemList).DeviceName.IndexOf(searchBoxDevice.Text, StringComparison.OrdinalIgnoreCase) >= 0;

        }
        private bool ListFullSourceT(object itemList)
        {
            //SetPropertyGridObject("Tag");
            if (String.IsNullOrEmpty(searchBoxTag.Text))
                return true;
            else
                return ((Tag)itemList).TagName.IndexOf(searchBoxTag.Text, StringComparison.OrdinalIgnoreCase) >= 0;

        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            stackPanelConnector.Visibility = Visibility.Hidden;
            stackPanelSerachC.Visibility = Visibility.Visible;
        }

        private void btnCancelSerach_Click(object sender, RoutedEventArgs e)
        {
            stackPanelConnector.Visibility = Visibility.Visible;
            stackPanelSerachC.Visibility = Visibility.Hidden;
            searchBox.Text = string.Empty;
        }

        private void btnCancelSerachD_Click(object sender, RoutedEventArgs e)
        {
            spDevice.Visibility = Visibility.Visible;
            spSerachDevice.Visibility = Visibility.Hidden;
            searchBoxDevice.Text = string.Empty;
        }

        private void btnSearchD_Click(object sender, RoutedEventArgs e)
        {
            spDevice.Visibility = Visibility.Hidden;
            spSerachDevice.Visibility = Visibility.Visible;
        }

        private void btnSearchT_Click(object sender, RoutedEventArgs e)
        {
            spTag.Visibility = Visibility.Hidden;
            spSerachTag.Visibility = Visibility.Visible;
        }

        private void btnCancelSerachT_Click(object sender, RoutedEventArgs e)
        {
            spTag.Visibility = Visibility.Visible;
            spSerachTag.Visibility = Visibility.Hidden;
            searchBoxTag.Text = string.Empty;
        }

        private void DeviceListBox_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void DeviceListBox_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //GroupingTags item = DeviceListBox.SelectedItem as GroupingTags;

            //if (DeviceListBox.SelectedItem != null)
            //{
            //    if(DeviceListBox.SelectedItem.ToString()=="OPCEngine.GroupingTags")
            //    {
            //        DeviceListBox.ContextMenu = DeviceListBox.Resources["GroupContext"] as ContextMenu;
            //    }
            //    else
            //    {
            //        DeviceListBox.ContextMenu = DeviceListBox.Resources["DeviceContext"] as ContextMenu;
            //    }                
            //}
            //else
            //{
            //    DeviceListBox.ContextMenu = DeviceListBox.Resources["outContext"] as ContextMenu;
            //}
            if (e.NewValue != null)
            {
                if (e.NewValue.ToString() != "Elpis.Windows.OPC.Server.TagGroup")
                {
                    TaglListBox.ItemsSource = ((DeviceBase)e.NewValue).TagsCollection;
                    propertyDisplayLbl.Content = ((DeviceBase)e.NewValue).DeviceName + "- Properties";
                }
                else
                {
                    TaglListBox.ItemsSource = ((TagGroup)e.NewValue).TagsCollection;
                    propertyDisplayLbl.Content = ((TagGroup)e.NewValue).GroupName + "- Properties";
                }
                gridPropertylbl.Visibility = Visibility.Visible;
                PropertiesPropertyGrid.SelectedObject = e.NewValue;

                PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                border.BorderBrush = Brushes.DarkBlue;
            }
        }

        /// <summary>
        /// Create a new Tag Group under Device. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewTagGroupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddNewGroup newGroupWindow = new AddNewGroup(ConnectorListBox.SelectedItem as ConnectorBase);
                if (newGroupWindow.TagGroupPropertGrid.SelectedObject != null)
                {
                    newGroupWindow.ShowDialog();
                    if (newGroupWindow.CancelBtn.Background != Brushes.Red)
                    {
                        var device = DeviceFactory.GetDeviceByName(((DeviceBase)newGroupWindow.cmboxDevice.SelectedItem).DeviceName, ((Elpis.Windows.OPC.Server.ConnectorBase)ConnectorListBox.SelectedItem).DeviceCollection);
                        DeviceBase deviceObject = DeviceFactory.GetDevice(device);
                        if (device != null)
                        {
                            var group = newGroupWindow.TagGroupPropertGrid.SelectedObject as TagGroup;
                            //var item1 = newDeviceWindow.DevicePropertyGrid.SelectedObject;
                            group.DeviceName = deviceObject.DeviceName;
                            group.TagsCollection = new ObservableCollection<Tag>();
                            if (deviceObject.GroupCollection == null)
                                deviceObject.GroupCollection = new ObservableCollection<TagGroup>();
                            List<string> groupsNameCollection = deviceObject.GroupCollection.Select(n => n.GroupName.ToLower()).ToList();
                            if (groupsNameCollection.Contains(group.GroupName.ToLower()))
                            {
                                MessageBox.Show("Tag Group with same name already exists, please create with another name", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            else
                            {
                                deviceObject.GroupCollection.Add(group);
                                DeviceListBox.ItemsSource = null;
                                DeviceListBox.ItemsSource = ((ConnectorBase)ConnectorListBox.SelectedItem).DeviceCollection;
                                TreeViewItem tvItem = DeviceListBox.ItemContainerGenerator.ContainerFromItem(deviceObject) as TreeViewItem;
                                if (tvItem != null && tvItem.Items.Count > 0)
                                {
                                    tvItem.IsSelected = true;
                                    tvItem.IsExpanded = true;
                                }
                            }
                        }
                        newGroupWindow.Close();
                    }
                }

                //AddNewDeviceUI newDeviceWindow = new AddNewDeviceUI(ProtocolListBox.SelectedItem);
                ////newDeviceWindow.modbusEthernetDevice.ProtocolAssignment = selectedProtocol.ProtocolName;
                //newDeviceWindow.ShowDialog();
                //if (newDeviceWindow.rdbtnDevice.IsChecked == false)
                //{

                //    var device = DeviceFactory.GetDeviceByName(((OPCEngine.DeviceBase)newDeviceWindow.cmboxDevice.SelectedItem).DeviceName, ((OPCEngine.ConnectorBase)ProtocolListBox.SelectedItem).DeviceCollection);
                //    DeviceBase deviceObject = DeviceFactory.GetDeviceObjbyDevice(device);
                //    if (device != null)
                //    {
                //        var group = new GroupingTags();
                //        //var item1 = newDeviceWindow.DevicePropertyGrid.SelectedObject;
                //        group.GroupName = ((OPCEngine.GroupingTags)newDeviceWindow.DevicePropertyGrid.SelectedObject).GroupName;
                //        group.TagsCollection = new ObservableCollection<Tags>();
                //        deviceObject.GroupCollection.Add(group);
                //    }
                //    else
                //    {
                //        var device1 = DeviceListBox.Items[0] as DeviceBase;
                //        var group1 = new GroupingTags();
                //        var item = newDeviceWindow.DevicePropertyGrid.SelectedObject;
                //        group1.GroupName = ((OPCEngine.GroupingTags)item).GroupName;
                //        group1.TagsCollection = new ObservableCollection<Tags>();
                //        device1.GroupCollection.Add(group1);
                //    }
                //}
            }
            catch (Exception ex)
            {
                ElpisServer.Addlogs("Configuration", @"Configuration/TagGroup", ex.Message, LogStatus.Error);
            }
        }

        private void expProperty_Collapsed(object sender, RoutedEventArgs e)
        {
            expProperty.BorderBrush = Brushes.Transparent;
            // this.PropertiesPropertyGrid.Width = this.PropertiesPropertyGrid.ActualWidth - 250;
            //this.ProtocolListBox.Width = this.ProtocolListBox.ActualWidth + 100;
            //this.TaglListBox.Width = this.TaglListBox.ActualWidth + 100;
            //this.DeviceListBox.Width = this.DeviceListBox.ActualWidth + 100;
        }

        private void expProperty_Expanded(object sender, RoutedEventArgs e)
        {
            if (propertyDisplayLbl != null)
            {
                if (propertyDisplayLbl.ToString().Contains("Editor"))
                {
                    expProperty.BorderBrush = Brushes.Black;
                }
            }
        }

        private void DeviceListBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // TaglListBox.ItemsSource = null;
                if (PropertiesPropertyGrid.IsReadOnly)
                {
                    if (DeviceListBox.SelectedItem != null)
                    {
                        if (DeviceListBox.SelectedItem.GetType().Name == "TagGroup")
                            propertyDisplayLbl.Content = string.Format("{0}- Properties", ((TagGroup)DeviceListBox.SelectedItem).GroupName);
                        else
                            propertyDisplayLbl.Content = string.Format("{0}- Properties", ((DeviceBase)DeviceListBox.SelectedItem).Name); //DeviceListBox.SelectedItem;
                        PropertiesPropertyGrid.SelectedObject = DeviceListBox.SelectedItem;
                        PropertiesPropertyGrid.Focus();
                        PropertiesPropertyGrid.BorderBrush = Brushes.DarkBlue;
                        border.BorderBrush = Brushes.DarkBlue;
                    }
                    //else
                    //{

                    //    if (ConnectorListBox.SelectedItem != null)
                    //    {
                    //        // propertyDisplayLbl.Background = Brushes.Magenta;
                    //        propertyDisplayLbl.Content = string.Format("{0}- Properties", ((IConnector)ConnectorListBox.SelectedItem).Name);//ProtocolListBox.SelectedItem;
                    //        gridPropertylbl.Visibility = Visibility.Visible;
                    //        PropertiesPropertyGrid.SelectedObject = ConnectorListBox.SelectedItem;
                    //        PropertiesPropertyGrid.Focus();
                    //        PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                    //        border.BorderBrush = Brushes.DarkOrange;
                    //    }
                    //    //DeviceListBox.ContextMenu = DeviceListBox.Resources["outContext"] as ContextMenu;
                    //}
                }
                else
                {
                    PropertiesPropertyGrid.IsReadOnly = true;

                    PropertiesPropertyGrid.SelectedObject = null;
                    ConnectorListBox.SelectedIndex = 0;
                    propertyDisplayLbl.Content = string.Format("{0}- Properties", ((IConnector)ConnectorListBox.SelectedItem).Name);//ProtocolListBox.SelectedItem;
                    gridPropertylbl.Visibility = Visibility.Visible;
                    PropertiesPropertyGrid.SelectedObject = ConnectorListBox.SelectedItem;
                    PropertiesPropertyGrid.Focus();
                    PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                    border.BorderBrush = Brushes.DarkOrange;
                    // MessageBox.Show("To save changes press TAB or Enter Key..", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                //  string message = ex.Message;
            }
        }

        private DateTime start { get; set; }
        DispatcherTimer t { get; set; }
        /// <summary>
        /// Setting the demo period of the server after clicking the start button.
        /// </summary>
        public void TimeSetter()
        {
            //DemoTimer timer = new DemoTimer();           

            t = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background,
               t_Tick, Dispatcher.CurrentDispatcher);
            t.IsEnabled = true;
            start = DateTime.Now;
            t.Start();
            spServerRunningTime.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the how much time is available in demo period.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void t_Tick(object sender, EventArgs e)
        {
            TimeSpan time = new TimeSpan(0, 30, 0); //set how much time server run at demo version.
            if ((time - (DateTime.Now - start)) > new TimeSpan(0, 0, 0))
            {
                txtTime.Foreground = Brushes.Black;
                TimeSpan remainTime = time - (DateTime.Now - start);
                int hh = remainTime.Hours;
                int mm = remainTime.Minutes;
                int ss = remainTime.Seconds;
                txtTime.Text = string.Format("{0}:{1}:{2}", hh, mm, ss);
                //txtTime.Text = remainTime.ToString("hh:mm:ss"); //Convert.ToString(time - (DateTime.Now - start));
                //txtTime.Foreground = Brushes.Black;
                //txtTime.Text = Convert.ToString(time - (DateTime.Now - start));
            }
            else
            {
                t.Stop();
                t.IsEnabled = false;
                //elpisServer.StartStop();
                ElpisServer.isDemoExpired = true;
                ((System.Windows.Threading.DispatcherTimer)sender).IsEnabled = false;
                txtTime.Text = "Demo version is expired.";
                txtTime.Foreground = Brushes.Red;
                MessageBox.Show(@"'Elpis OPC Sever' demo period is expired, restart server again.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void PropertiesPropertyGrid_ClearPropertyItem(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemEventArgs e)
        {
            e.PropertyItem.ToolTip = "To Edit press TAB or Enter";
        }


        private void PropertiesPropertyGrid_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

        }

        private void ConnectorListBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PropertiesPropertyGrid.IsReadOnly)
            {

                if (ConnectorListBox.SelectedItem != null)
                {
                    // propertyDisplayLbl.Background = Brushes.Magenta;
                    PropertiesPropertyGrid.SelectedObject = null;
                    propertyDisplayLbl.Content = string.Format("{0}- Properties", ((IConnector)ConnectorListBox.SelectedItem).Name);//ProtocolListBox.SelectedItem;
                    gridPropertylbl.Visibility = Visibility.Visible;
                    PropertiesPropertyGrid.SelectedObject = ConnectorListBox.SelectedItem;
                    PropertiesPropertyGrid.Focus();
                    PropertiesPropertyGrid.BorderBrush = Brushes.DarkOrange;
                    border.BorderBrush = Brushes.DarkOrange;
                }
            }
            else
            {
                PropertiesPropertyGrid.IsReadOnly = true;
                //MessageBox.Show("To save changes press TAB or Enter Key..", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        //private void RibbonMinimizeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (RibbonMain.IsMinimized == false)
        //    {                
        //        RibbonMain.IsMinimized = true;
        //        MinimizeButton.SmallImageSource = new BitmapImage(new Uri("Images/navigate-down.ico", UriKind.Relative));
        //        gridConfiguration.Height = (this.ActualHeight - RibbonMain.ActualHeight) + 50;
        //    }
        //    else if (RibbonMain.IsMinimized == true)
        //    {                
        //        RibbonMain.IsMinimized = false;
        //        MinimizeButton.SmallImageSource = new BitmapImage(new Uri("Images/navigate-up.ico", UriKind.Relative));
        //        gridConfiguration.Height = (this.ActualHeight - RibbonMain.ActualHeight) - 100;
        //    }
        //}

        //private void RibbonMain_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //     if (e.OriginalSource is TextBlock)
        //    {
        //        if (ignoreResize || !(((TextBlock)e.OriginalSource).Text == "Home"))
        //        {
        //            ignoreResize = false;
        //            return;
        //        }
        //    }
        //    else if (e.OriginalSource is Border)
        //    {
        //        if (ignoreResize || ((Border)e.OriginalSource).DataContext == null)
        //        {
        //            ignoreResize = false;
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        ignoreResize = false;
        //        return;
        //    }
        //    if (RibbonMain.IsMinimized == true)
        //    {
        //        //Sets minimize true and update the minimize button icon and category tree control height
        //        RibbonMain.IsMinimized = true;
        //        MinimizeButton.SmallImageSource = new BitmapImage(new Uri("/SymbolFactory3;component/Images/navigate-down.ico", UriKind.Relative));
        //        gridConfiguration.Height = (this.ActualHeight - RibbonMain.ActualHeight) + 50;
        //    }
        //    else if (RibbonMain.IsMinimized == false)
        //    {
        //        //Sets minimize false and resets the minimize button icon and category tree control height
        //        RibbonMain.IsMinimized = false;
        //        MinimizeButton.SmallImageSource = new BitmapImage(new Uri("/SymbolFactory3;component/Images/navigate-up.ico", UriKind.Relative));
        //        gridConfiguration.Height = (this.ActualHeight - RibbonMain.ActualHeight) - 100;
        //    }
        //}
    }

    #endregion End Of ConfigurationPresentation Window Class for All Code behinds


}
#endregion End Of Elpis OPC Server Namespace
