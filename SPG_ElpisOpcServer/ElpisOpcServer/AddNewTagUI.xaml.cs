#region Namespaces
using Elpis.Windows.OPC.Server;
using OPCEngine;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
#endregion End Of Namespaces

#region Elpis OPC Server Namespace
namespace ElpisOpcServer
{

    #region AddNewTagUI  Class
    /// <summary>
    /// Interaction logic for CreateTag.xaml
    /// </summary>
    public partial class AddNewTagUI : Window
    {
        //private ObservableCollection<TagGroup> groups;
        #region Private Field
        public dynamic SelectedDevice { get; set; }
        public Tag tag { get; set; }
        //public OPCEngine.ITag obj { get; set; }

        public bool flag { get; set; }

        #endregion End Of Private Field

        #region Constructor
        /// <summary>
        /// Constructor        
        /// </summary>
        public AddNewTagUI(dynamic Device)
        {
            InitializeComponent();
            SelectedDevice = Device;
            tag = new Tag();
            tag.DataType = DataType.Short;
           // tag.SelectedGroup = "None";
            //obj = tag;
            TagPropertyGrid.SelectedObject = tag;
        }

        public AddNewTagUI(dynamic Device, ObservableCollection<TagGroup> groups)
        {
            InitializeComponent();
            SelectedDevice = Device;
            tag = new Tag();
            tag.DataType = DataType.Short;
            //tag.SelectedGroup = "--Select--";
            tag.SelectedGroup = "PumpTest";
            //tag.TagGroup = groups;
            // obj = tag;
            tag.ScanRate = SLIKDAUACONFIG.GetDefaultTagScanRate();
            TagPropertyGrid.SelectedObject = tag;
        }
        #endregion End Of Constructor

        #region FinishBtn Click
        /// <summary>
        ///  CreateTagButton Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishBtn_click(object sender, RoutedEventArgs e)
        {
              Tag tagtObject = TagPropertyGrid.SelectedObject as Tag;
            if (this.Title == "Add New Tag")
            {
                if (tagtObject.MaxValue != 0 && tagtObject.MinValue != tagtObject.MaxValue && tagtObject.MaxValue != tagtObject.MinValue)
                {
                    if (tagtObject.SelectedGroup != "--Select--")
                    {
                        if (tagtObject.TagName != "" & tagtObject.TagName != null)
                        {
                            bool validName = Util.CheckValidName(tagtObject.TagName);


                            if (validName)
                            {
                                bool isNewTag;
                                if (((Tag)tagtObject).SelectedGroup == null || ((Tag)tagtObject).SelectedGroup == "None")
                                {
                                    isNewTag = ConfigurationSettingsUI.elpisServer.IsNewTag(SelectedDevice, tagtObject);
                                }
                                else
                                {
                                    isNewTag = ConfigurationSettingsUI.elpisServer.IsNewTag(SelectedDevice, ((DeviceBase)SelectedDevice).GroupCollection, tagtObject);
                                }
                                if (isNewTag == true)
                                {
                                    if (tagtObject.Address != "" & tagtObject.Address != null)
                                    {
                                        //if(currentObject.Address.)
                                        // bool isString=Regex.IsMatch(currentObject.Address, @"^[a-zA-Z]+$");
                                        if (SelectedDevice.DeviceType == DeviceType.ABMicroLogixEthernet)
                                        {
                                            if (SelectedDevice.DeviceModelType == AllenBbadleyModel.MicroLogix)
                                            {
                                                if (!(tagtObject.Address.StartsWith("N", StringComparison.CurrentCultureIgnoreCase) || tagtObject.Address.StartsWith("F", StringComparison.CurrentCultureIgnoreCase)))
                                                {
                                                    MessageBox.Show("Please Enter the valid Tag Address. Tag Address starts with N,F etc..");
                                                    return;
                                                }
                                                else
                                                {
                                                    if (tagtObject.Address.StartsWith("N", StringComparison.CurrentCultureIgnoreCase))
                                                    {
                                                        tagtObject.DataType = DataType.Integer;
                                                    }
                                                    else if (tagtObject.Address.StartsWith("F", StringComparison.CurrentCultureIgnoreCase))
                                                    {
                                                        tagtObject.DataType = DataType.Float;
                                                    }
                                                }
                                            }

                                        }
                                        else if (SelectedDevice.DeviceType == DeviceType.ModbusEthernet || SelectedDevice.DeviceType == DeviceType.ModbusSerial)
                                        {
                                            bool isNumber = Regex.IsMatch(tagtObject.Address, "^[0-9]+$");

                                            if (isNumber == false)
                                            {
                                                MessageBox.Show("Please Enter the correct Tag Address");
                                                return;
                                            }
                                        }
                                        //if (currentObject.ScanRate != "" & currentObject.ScanRate != null)
                                        //{
                                        flag = true;
                                        //this.Close();
                                        this.Hide();
                                        //}
                                        //else
                                        //{
                                        //    MessageBox.Show("Please enter the scan rate");
                                        //}
                                    }
                                    else
                                    {
                                        MessageBox.Show("Please enter the Tag Address");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Please enter different Tag Name");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please enter valid Tag Name.\nThe Tag Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 15 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            //else
                        }
                        else
                        {
                            MessageBox.Show("Please enter Tag Name.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    else
                    {
                        MessageBox.Show("Please choose \'Selected Group\' from the list.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("please enter the Max or Min value or do not enter the same value of minrange and maxrange");
                }

                
            
            //else
            //{
            //    MessageBox.Show("Enter Required Fields.");
            //}
            }
            else
            {
                if (tagtObject.TagName != "" & tagtObject.TagName != null)
                {
                    bool validName = Util.CheckValidName(tagtObject.TagName);
                    if (validName)
                    {
                        if (tagtObject.Address != "" & tagtObject.Address != null)
                        {
                            bool isNumber = Regex.IsMatch(tagtObject.Address, "^[0-9]+$");

                            if (isNumber == false)
                            {
                                MessageBox.Show("Please Enter the correct Tag Address");
                                return;
                            }
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Please enter the Tag Address");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid Tag Name.\nThe Tag Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else
                {
                    MessageBox.Show("Enter Required Fields.");
                }
            }
        }

        #endregion End Of FinishBtn Click

        #region CancelButton Click Event
        /// <summary>
        ///  CreateTagButton Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_click(object sender, RoutedEventArgs e)
        {
            Reset();
            this.Close();
            CancelBtn.Background = Brushes.Red;
        }
        #endregion End Of CancelButton Click Event

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (flag == false) { Reset(); }
            CancelBtn.Background = Brushes.Red;
        }
        #region Reset Function
        public void Reset()
        {
            tag = null;
        }
        #endregion Reset Function
    }
    #endregion End Of AddNewTagUI Class
}

#endregion End Of Elpis OPC Server Namespace