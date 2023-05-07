using Elpis.Windows.OPC.Server;
using Microsoft.Win32;
using OPCEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for LoggerViewUI.xaml
    /// </summary>
    
    public partial class LoggerViewUI : UserControl
    {
        public LoggerViewUI()
        {
            InitializeComponent();
            string path = GetLogFilePath();
            if(path!=null)
            {
                tbxPath.Text = path;
            }
            else
            {
                tbxPath.Text = Directory.GetCurrentDirectory() + @"\Elpis_OPC_Log";       //TODO:  --Done  Change path from config file
            }
            //tbxPath.Text = Directory.GetCurrentDirectory() + @"\Elpis_OPC_Log";       //TODO Change path from config file
            
        }

        private string GetLogFilePath()
        {
            try
            {
                var logPath = from d in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("LogFilePath") select d;
                foreach (var path in logPath)
                {
                    return path.Value.ToString();
                }
            }
            catch(Exception)
            {
               
            }
            return null;
        }

        private ListView _lvTempList = new ListView();
        public ListView lvTempList
        {
            get { return _lvTempList; }
            set { _lvTempList = value; }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add("Module");
            data.Add("All");
            data.Add("Configuration");
            data.Add("UA Configuration");
            data.Add("UA Certificate");
            data.Add("Internet Of Things");

            var comboBox = sender as ComboBox;

            comboBox.ItemsSource = data;

            comboBox.SelectedIndex = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewLogger.ItemsSource = null;
            if (comboBoxSectionList.SelectedIndex == 2)
            {
                ListViewLogger.ItemsSource = ConfigurationSettingsUI.elpisServer.ConfigurationLogCollection;
            }
            else if (comboBoxSectionList.SelectedIndex == 3)
            {
                ListViewLogger.ItemsSource = ConfigurationSettingsUI.elpisServer.UAConfigurationLogCollection;
            }
            else if (comboBoxSectionList.SelectedIndex == 4)
            {
                ListViewLogger.ItemsSource = ConfigurationSettingsUI.elpisServer.UACertificateLogCollection;
            }
            else if (comboBoxSectionList.SelectedIndex == 5)
            {
                ListViewLogger.ItemsSource = ConfigurationSettingsUI.elpisServer.IoTLogCollection;
            }
            else
                ListViewLogger.ItemsSource = ElpisServer.LoggerCollection;//ConfigurationSettingsUI.elpisServer.LoggerCollection;
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            ListViewLogger.ItemsSource = null;
            ConfigurationSettingsUI.elpisServer.ClearLog();
        }

        private void exportToCSV_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationSettingsUI.elpisServer.ExportToCSV(ListViewLogger);
        }

        private void copyToClipBoard_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationSettingsUI.elpisServer.CopyToClipBoard(ListViewLogger);
        }

        private void saveAsTextFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationSettingsUI.elpisServer.SaveLogsAsText(ListViewLogger);
        }

        private void closebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }


        private void minbtn_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.WindowState = WindowState.Minimized;
        }

        private void HandleUncheckedError(object sender, RoutedEventArgs e)
        {

        }

        private void HandleCheckError(object sender, RoutedEventArgs e)
        {

        }

        private void HandleUncheckedInfo(object sender, RoutedEventArgs e)
        {

        }

        private void HandleCheckWarning(object sender, RoutedEventArgs e)
        {

        }

        private void HandleCheckInfo(object sender, RoutedEventArgs e)
        {

        }

        private void HandleUncheckedWarning(object sender, RoutedEventArgs e)
        {

        }

        //private void ListViewLogger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {

        //    }
        //}

        private void DateColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            //GridViewColumnHeader column = (sender as GridViewColumnHeader);
            //string sortBy = column.Tag.ToString();
            //if (listViewSortCol != null)
            //{
            //    AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
            //    lvUsers.Items.SortDescriptions.Clear();


            //    ListSortDirection newDir = ListSortDirection.Ascending;
            //    if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
            //        newDir = ListSortDirection.Descending;

            //    listViewSortCol = column;
            //    listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            //    AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            //    lvUsers.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));


            //}
        }

        private void toggleBtnError_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewLogger.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource);
                view.Filter = ListEventTypeError;
                CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource).Refresh();
                lvTempList.ItemsSource = CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource);
            }
        }

        private bool ListEventTypeError(object itemList)
        {
            bool bStatus = true;
            //if (toggleBtnError.IsChecked != true)
            //{
            //    if (toggleBtnInfo.IsChecked == true)
            //    {
            //        if (toggleBtnWarning.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning");
            //        }
            //        else
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information");
            //        }
            //    }
            //    else
            //    {
            //        if (toggleBtnWarning.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning");
            //        }
            //        else
            //        {
            //            bStatus = false;
            //        }
            //    }
            //}
            //else
            //{
            //    if (toggleBtnInfo.IsChecked == true)
            //    {
            //        if (toggleBtnWarning.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning");
            //        }
            //        else
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error")
            //               || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information");
            //        }
            //    }
            //    else
            //    {
            //        if (toggleBtnWarning.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning")
            //                 || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error");
            //        }
            //        else
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error");
            //        }
            //    }
            //}
            return bStatus;
        }

        private void toggleBtnInfo_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewLogger.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource);
                view.Filter = ListEventTypeInfo;
                CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource).Refresh();
            }
        }

        private bool ListEventTypeInfo(object itemList)
        {
            bool bStatus = true;
            //if (toggleBtnInfo.IsChecked != true)
            //{
            //    if (toggleBtnError.IsChecked == true)
            //    {
            //        if (toggleBtnWarning.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error");
            //        }
            //        else
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error");
            //        }
            //    }
            //    else
            //    {
            //        if (toggleBtnWarning.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning");
            //        }
            //        else
            //        {
            //            bStatus = false;
            //        }
            //    }
            //}
            //else
            //{
            //    if (toggleBtnError.IsChecked == true)
            //    {
            //        if (toggleBtnWarning.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning");
            //        }
            //        else
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error")
            //               || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information");
            //        }
            //    }
            //    else
            //    {
            //        if (toggleBtnWarning.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information");
            //        }
            //        else
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information");
            //        }
            //    }
            //}

            return bStatus;
        }

        private void toggleBtnWarning_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewLogger.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource);
                view.Filter = ListEventTypeWarning;
                CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource).Refresh();
            }
        }

        private bool ListEventTypeWarning(object itemList)
        {
            bool bStatus = true;
            //if (toggleBtnWarning.IsChecked != true)
            //{
            //    if (toggleBtnInfo.IsChecked == true)
            //    {
            //        if (toggleBtnError.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error");
            //        }
            //        else
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information");
            //        }
            //    }
            //    else
            //    {
            //        if (toggleBtnError.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error");
            //        }
            //        else
            //        {
            //            bStatus = false;
            //        }
            //    }
            //}
            //else
            //{
            //    if (toggleBtnInfo.IsChecked == true)
            //    {
            //        if (toggleBtnError.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information")
            //                || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning");
            //        }
            //        else
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Warning")
            //               || (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Information");
            //        }
            //    }
            //    else
            //    {
            //        if (toggleBtnError.IsChecked == true)
            //        {
            //            bStatus = (((OPCEngine.LoggerViewModel)itemList).EventType.ToString() == "Error")
            //                || (((LoggerViewModel)itemList).EventType.ToString() == "Warning");
            //        }
            //        else
            //        {
            //            bStatus = (((LoggerViewModel)itemList).EventType.ToString() == "Warning");
            //        }
            //    }
            //}

            return bStatus;
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (ListViewLogger.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource);
                view.Filter = ListFullSource;
            }
        }

        private bool ListFullSource(object itemList)
        {
            if (String.IsNullOrEmpty(searchBox.Text))
                return true;
            else
                return (((LoggerViewModel)itemList).Source.IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (((LoggerViewModel)itemList).Module.IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (((LoggerViewModel)itemList).Event.IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (((LoggerViewModel)itemList).Date.IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (((LoggerViewModel)itemList).EventType.ToString().IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ListViewLogger.ItemsSource != null)
                CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource).Refresh();
        }

        private void combobox_filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewLogger != null)
            {
                if (ListViewLogger.ItemsSource != null)
                {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource);
                view.Filter = ListEventType;
                CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource).Refresh();
                lvTempList.ItemsSource = CollectionViewSource.GetDefaultView(ListViewLogger.ItemsSource);
                }
            }
        }

        private bool ListEventType(object itemList)
        {
            bool status = true;
            if(combobox_filter.SelectedIndex==2)
            {
                status = (((LoggerViewModel)itemList).EventType.ToString() == "Error");
            }
            else if(combobox_filter.SelectedIndex==3)
            {
                status = (((LoggerViewModel)itemList).EventType.ToString() == "Information");
            }
            else if(combobox_filter.SelectedIndex==4)
            {
                status = (((LoggerViewModel)itemList).EventType.ToString() == "Warning");
            }
            else
            {
                status = (((LoggerViewModel)itemList).EventType.ToString() == "Error")
                            || (((LoggerViewModel)itemList).EventType.ToString() == "Information")
                            || (((LoggerViewModel)itemList).EventType.ToString() == "Warning");
            }
            return status;
        }
    }
}
